using Ganss.Xss;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker.Handlers
{
    class OldMetadataHandler
    {
        /// <summary>
        /// Cache max duration in days
        /// </summary>
        static int cacheDuration = 60;

        /// <summary>
        /// Cached GPU Data
        /// </summary>
        static JObject cachedGPUData;

        /// <summary>
        /// Cached OS Data
        /// </summary>
        static OSClassRoot cachedOSData;

        /// <summary>
        /// Finds the GPU, the version and queries up to date information
        /// </summary>
        public static (List<NvidiaDriver> nvidiaDrivers, string error, string releaseNotes) GetDriverMetadata(GPU gpu, string driverType)
        {
            // Populate pfId from ZenitH-AT's nvidia-data
            (gpu, bool success) = PopulateGpuMetadata(gpu);

            if (!success) return (null, "Could not lookup GPU in ZenitH-AT's nvidia-data repo", null);
            int osId = GetOsId();
            if (osId == 0) return (null, "No matching operating system was found in the metadata.", null);

            // Use AJAX API
            (List<NvidiaDriver> nvidiaDrivers, string releaseNotes) = GetDriverInfo(gpu, osId, driverType);
            if (nvidiaDrivers == null || nvidiaDrivers.Count == 0) return (null, "No driver matches the configured driver family.", null);

            // Return found
            return (nvidiaDrivers, null, releaseNotes);
        }

        // Uses ZenitH-AT's nvidia-data repo to get pfId from GPU name
        public static (GPU gpu, bool success) PopulateGpuMetadata(GPU gpu, bool forceRecache = false)
        {
            // Lookup GPU name
            (bool success, int pfId) = GetPfIdFromGpuName(gpu.name, gpu.isNotebook);

            if (success)
            {
                gpu.pfId = pfId;
            }
            else
            {
                // Invert isNotebook switch, perhaps it is an eGPU?
                (success, pfId) = GetPfIdFromGpuName(gpu.name, !gpu.isNotebook);

                if (success)
                {
                    gpu.isNotebook = !gpu.isNotebook;
                    gpu.pfId = pfId;
                }
                else
                {
                    gpu.isValidated = false;
                }
            }

            // If GPU lookup was successful
            if (success)
            {
                return (gpu, true);
            }
            else
            {

                // If no GPU could be validated, then force recaching of OldMetadataHandler once, and loop again.
                // This fixes issues related with outdated cache
                if (!forceRecache)
                {
                    PrepareCache(true);
                    return PopulateGpuMetadata(gpu, true);
                }
                else
                {
                    // Could not find GPU driver metadata in repo
                    return (null, false);
                }
            }
        }

        public static void PrepareCache(bool forceRecache = false)
        {
            var gpuData = GetCachedMetadata("gpu-data.json", forceRecache);
            var osData = GetCachedMetadata("os-data.json", forceRecache);

            // Validate GPU Data JSON
            try
            {
                cachedGPUData = JObject.Parse(gpuData);
            }
            catch
            {
                gpuData = GetCachedMetadata("gpu-data.json", true);
                cachedGPUData = JObject.Parse(gpuData);
            }

            // Validate OS JSON
            try
            {
                cachedOSData = JsonConvert.DeserializeObject<OSClassRoot>(osData);
            }
            catch
            {
                osData = GetCachedMetadata("os-data.json", true);
                cachedOSData = JsonConvert.DeserializeObject<OSClassRoot>(osData);
            }
        }

        public static (List<NvidiaDriver> nvidiaDrivers, string releaseNotes) GetDriverInfo(GPU gpu, int osId, string driverType)
        {
            List<NvidiaDriver> nvidiaDrivers = new();
            int recommendedDriverIdx = -1;
            JArray driversFound = GetDriversFromNvidiaAjax(gpu.pfId, osId);
            if (driversFound == null || driversFound.Count == 0) return (null, null);

            // Driver type (upCRD)
            // - 0 is Game Ready Driver (GRD), and/or notebook, and/or quadro (RTX enterprise)
            // - 1 is Studio Driver (SD)
            int driverTypeInt = driverType == "grd" ? 0 : 1;

            for (int i = 0; i < driversFound.Count; i++)
            {
                if (driversFound[i] is not JObject entry || entry["downloadInfo"] is not JObject driver) continue;
                string version = driver["Version"]?.ToString();
                string downloadUrl = driver["DownloadURL"]?.ToString();
                if (!Version.TryParse(version, out _)
                    || !Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri uri)
                    || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                    || !DateTime.TryParse(driver["ReleaseDateTime"]?.ToString(), out DateTime releaseDate)) continue;

                // To identify Quadro New Feature Branch (NFB) drivers, check if IsFeaturePreview is set to 1
                bool isFeaturePreview = driver["IsFeaturePreview"].ToString() == "1";

                // Get driver type and label based on download URL + isFeaturePreview
                (string driverTypeKey, string driverTypeLabel) = GetDriverTypeKey(downloadUrl, isFeaturePreview);

                // Extract PDF URL from OtherNotes
                string otherNotes = Uri.UnescapeDataString(driver["OtherNotes"].ToString());
                string pdfUrl = ExtractPdfUrlFromNotes(otherNotes);

                NvidiaDriver driverObj = new()
                {
                    title = $"{version} - Type: {driverTypeLabel}",
                    version = version,
                    type = driverTypeKey,
                    typeLabel = driverTypeLabel,
                    downloadUrl = downloadUrl,
                    pdfUrl = pdfUrl,
                    fileSizeEst = driver["DownloadURLFileSize"].ToString(),
                    releaseDate = releaseDate
                };

                // Set recommended driver if unset, and the driver matches driverType
                if (recommendedDriverIdx == -1 && driver["IsCRD"].ToString() == driverTypeInt.ToString())
                {
                    recommendedDriverIdx = i;
                    driverObj.recommended = true;
                }

                nvidiaDrivers.Add(driverObj);
            }

            if (recommendedDriverIdx < 0) return (null, null);

            // Get raw release notes
            JObject downloadInfo = (JObject)driversFound[recommendedDriverIdx]["downloadInfo"];
            string tempNotes = Uri.UnescapeDataString(downloadInfo["ReleaseNotes"].ToString());

            // Load release notes
            HtmlAgilityPack.HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(tempNotes);

            // Remove image nodes
            var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var child in nodes) child.Remove();
            }

            // Remove all links
            try
            {
                var hrefNodes = htmlDocument.DocumentNode.SelectNodes("//a").Where(x => x.Attributes.Contains("href"));
                foreach (var child in hrefNodes) child.Remove();
            }
            catch { }

            // Save the cleaned release notes
            tempNotes = htmlDocument.DocumentNode.OuterHtml;

            // Sanitize tempNotes and set releaseNotes
            HtmlSanitizer sanitizer = new();
            string releaseNotes = sanitizer.Sanitize(tempNotes);

            return (nvidiaDrivers, releaseNotes);
        }

        // Extracts driver PDF URL from NVIDIA OtherNotes
        private static string ExtractPdfUrlFromNotes(string otherNotes)
        {
            // Load otherNotes into HtmlAgilityPack
            HtmlAgilityPack.HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(otherNotes);

            IEnumerable<HtmlNode> node = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));

            // Loop all nodes and find the one that contains "release-notes.pdf" in the href attribute
            foreach (HtmlNode child in node)
            {
                if (child.Attributes["href"].Value.Contains("release-notes.pdf"))
                {
                    return child.Attributes["href"].Value.Trim();
                }
            }

            return null;
        }

        private static JArray GetDriversFromNvidiaAjax(int pfId, int osId)
        {
            try
            {
                // Construct driver URL
                string ajaxDriverURL = MainConsole.nvidiaAjaxURL;

                // Get 10 latest drivers, International language
                ajaxDriverURL += $"&pfid={pfId}&osID={osId}&dch=1&numberOfResults=10&languageCode=1078";

                // Sends a GET request, and parses the response into JObject
                string response = MainConsole.SendGetRequest(ajaxDriverURL);

                // Parse the response into JObject
                JObject nvResponse = JObject.Parse(response);

                // Success is count drivers found
                if (int.TryParse(nvResponse["Success"]?.ToString(), out int count) && count > 0
                    && nvResponse["IDS"] is JArray { Count: > 0 } drivers)
                {
                    return drivers;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MainConsole.Write("ERROR!");
                MainConsole.WriteLine();
                MainConsole.WriteLine("No NVIDIA driver was found for your system configuration.");
                MainConsole.WriteLine();
                MainConsole.WriteLine("Debugging information:");
                MainConsole.WriteLine($"pfId: {pfId}");
                MainConsole.WriteLine($"osId: {osId}");
            }
            catch (Exception ex)
            {
                MainConsole.Write("ERROR!");
                MainConsole.WriteLine();
                MainConsole.WriteLine("Unable to interact with NVIDIA API.");
                MainConsole.WriteLine();
                MainConsole.WriteLine(ex.ToString());
            }

            return null;
        }

        // Uses https://github.com/ZenitH-AT/nvidia-data to map Product Family ID (pfId) from GPU Name
        // pfId is required when calling NVIDIA Ajax API, as it does not take GPU Device ID
        // Requires the GPU name to be sanitized as per Zenith-AT's standard
        public static (bool, int) GetPfIdFromGpuName(string gpuName, bool isNotebook)
        {
            try
            {
                int gpuId = (int)cachedGPUData[isNotebook ? "notebook" : "desktop"][gpuName];
                return (true, gpuId);
            }
            catch
            {
                return (false, 0);
            }
        }

        // Maps NVIDIA Ajax metadata "Type" to TNUC driver type
        private static (string driverTypeKey, string driverTypeLabel) GetDriverTypeKey(string downloadUrl, bool isFeaturePreview)
        {
            if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri uri))
                return ("unknown", "Unknown");

            string path = Uri.UnescapeDataString(uri.AbsolutePath);
            string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Contains("Quadro_Certified", StringComparer.OrdinalIgnoreCase))
            {
                return isFeaturePreview
                    ? ("quadro-nfb", "New Feature Branch (RTX Enterprise)")
                    : ("quadro", "Quadro (RTX Enterprise)");
            }

            string[] tokens = Path.GetFileNameWithoutExtension(path).Split('-');
            bool notebook = tokens.Contains("notebook", StringComparer.OrdinalIgnoreCase);
            bool studio = tokens.Contains("nsd", StringComparer.OrdinalIgnoreCase);
            if (studio)
                return notebook ? ("sd-notebook", "Studio Driver (Notebook)") : ("sd", "Studio Driver");
            if (notebook) return ("notebook", "Notebook");
            if (tokens.Contains("desktop", StringComparer.OrdinalIgnoreCase))
                return ("grd", "Game Ready Driver");

            return ("unknown", "Unknown");
        }

        public static int GetOsId()
        {
            if (cachedOSData == null) return 0;
            // Get operating system ID
            string osVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
            string osBit = Environment.Is64BitOperatingSystem ? "64" : "32";
            int osId = 0;

            if (osVersion == "10.0" && Environment.OSVersion.Version.Build >= 22000)
            {
                foreach (OSClass os in cachedOSData)
                {
                    if (Regex.IsMatch(os.name, "Windows 11"))
                    {
                        osId = os.id;
                        break;
                    }
                }
            }
            else
            {
                foreach (OSClass os in cachedOSData)
                {
                    if (os.code == osVersion && Regex.IsMatch(os.name, osBit))
                    {
                        osId = os.id;
                        break;
                    }
                }
            }

            if (osId == 0)
            {
                MainConsole.Write("ERROR!");
                MainConsole.WriteLine();
                MainConsole.WriteLine("No NVIDIA driver was found for this operating system configuration. Make sure TNUC is updated.");
                MainConsole.WriteLine();
                MainConsole.WriteLine($"osVersion: {osVersion}");
            }

            return osId;
        }

        private static dynamic GetCachedMetadata(string fileName, bool forceRecache)
        {
            string dataPath = Path.Combine(ConfigurationHandler.configDirectoryPath, fileName);

            // If the cache exists and is not outdated, then it can be used
            if (File.Exists(dataPath) && !forceRecache)
            {
                DateTime lastUpdate = File.GetLastWriteTime(dataPath);
                var days = (DateTime.Now - lastUpdate).TotalDays;

                if (days < cacheDuration)
                {
                    try
                    {
                        return File.ReadAllText(dataPath);
                    }
                    catch
                    {

                    }
                }
            }

            // Delete corrupt/old file if it exists
            if (File.Exists(dataPath))
            {
                try
                {
                    File.Delete(dataPath);
                }
                catch
                {
                    // error
                }
            }

            // Download the file and cache it
            string rawData = MainConsole.SendGetRequest($"{MainConsole.gpuMetadataRepo}/{fileName}");

            try
            {
                File.AppendAllText(dataPath, rawData);
            }
            catch
            {
                // Unable to cache
            }

            return rawData;
        }
    }
}

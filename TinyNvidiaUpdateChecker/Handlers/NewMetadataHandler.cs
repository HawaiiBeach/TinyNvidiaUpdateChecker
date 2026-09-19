using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ganss.Xss;
using Newtonsoft.Json.Linq;
using TinyNvidiaUpdateChecker;

public class GpuDevice
{
    public string id { get; set; }
    public string vendorid { get; set; }
    public string ssid { get; set; }
    public string svid { get; set; }
    public string name { get; set; }
}

public class DriverVersion
{
    public string key { get; set; }
    public string version { get; set; }
    public List<string> os { get; set; }
    public string bit { get; set; }
    public string type { get; set; }
    public int dch { get; set; }
    public List<int> supports { get; set; }
}

public class CombinedGpuData
{
    public Dictionary<string, GpuDevice> devices { get; set; }
    public List<DriverVersion> versions { get; set; }
}

/// <summary>
/// This class handles the retrieval and processing of GPU metadata provided by TechPowerUp
/// </summary>
public class NewMetadataHandler
{
    private static CombinedGpuData _combinedGpuData;

    public static bool LoadCombinedJsonData()
    {
        try
        {
            string jsonString = MainConsole.SendGetRequest(MainConsole.experimentalGpuMetadataRepo);
            _combinedGpuData = JsonSerializer.Deserialize<CombinedGpuData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return _combinedGpuData?.devices != null && _combinedGpuData.versions != null;
        }
        catch
        {
            _combinedGpuData = null;
            return false;
        }
    }

    public static (GpuDevice matchedGpu, int deviceKey) FindGpuDetailsByDeviceId(string deviceId)
    {
        if (_combinedGpuData?.devices == null || string.IsNullOrWhiteSpace(deviceId)) return (null, 0);

        foreach (KeyValuePair<string, GpuDevice> entry in _combinedGpuData.devices)
        {
            GpuDevice device = entry.Value;
            if (device?.id != null && device.id.Equals(deviceId, StringComparison.OrdinalIgnoreCase) && int.TryParse(entry.Key, out int key))
            {
                return (device, key);
            }
        }
        return (null, 0);
    }

    private static bool IsMobileGpuIndex(int gpuIndex)
    {
        return _combinedGpuData?.devices?.TryGetValue(gpuIndex.ToString(), out GpuDevice device) == true
            && device != null
            && (device.name?.Contains("Laptop", StringComparison.OrdinalIgnoreCase) == true
                || device.name?.Contains("Notebook", StringComparison.OrdinalIgnoreCase) == true
                || device.name?.Contains("Max-Q", StringComparison.OrdinalIgnoreCase) == true);
    }

    public static List<NvidiaDriver> FindDriversForGpu(int gpuIndex, string driverType)
    {
        List<NvidiaDriver> nvidiaDrivers = new();
        NvidiaDriver latestDriver = null;
        NvidiaDriver latestNotebookDriver = null;
        Version latestParsedVersion = new(0, 0);
        Version latestNotebookVersion = new(0, 0);
        bool isMobileGpu = IsMobileGpuIndex(gpuIndex);
        bool preferNotebook = !string.Equals(driverType, "sd", StringComparison.OrdinalIgnoreCase) && isMobileGpu;

        if (_combinedGpuData?.versions == null) return nvidiaDrivers;

        foreach (DriverVersion driver in _combinedGpuData.versions)
        {
            // If the driver supports our GPU
            if (driver?.supports?.Contains(gpuIndex) == true)
            {
                // Ignore 32 bit drivers
                if (driver.bit == "32") continue;

                // Map experimental metadata "Type" to TNUC driver type
                (string driverTypeKey, string driverTypeLabel) = GetDriverTypeKey(driver.type);
                if (driverTypeKey == "unknown") continue;

                // For some reason, expermiental metadata repo is matching desktop GPUs with notebook drivers
                // Filter out notebook drivers for desktop GPUs
                // Studio drivers use unified desktop/notebook packages; GPU support was checked above.
                if (driverTypeKey != "sd")
                {
                    string[] packageTokens = (driver.key ?? string.Empty).Split('-');
                    bool notebookPackage = driverTypeKey == "notebook"
                        || packageTokens.Contains("notebook", StringComparer.OrdinalIgnoreCase);
                    bool desktopPackage = driverTypeKey == "grd"
                        || packageTokens.Contains("desktop", StringComparer.OrdinalIgnoreCase);
                    if (isMobileGpu ? !notebookPackage : !desktopPackage || notebookPackage) continue;
                }
                if (string.IsNullOrWhiteSpace(driver.key) || !Version.TryParse(driver.version, out _)) continue;

                string downloadUrl = $"https://international.download.nvidia.com/Windows/{driver.version}/{driver.key}.exe";

                NvidiaDriver driverObj = new()
                {
                    title = $"{driver.version} - Type: {driverTypeLabel}",
                    version = driver.version,
                    type = driverTypeKey,
                    typeLabel = driverTypeLabel,
                    downloadUrl = downloadUrl,
                    fileSizeEst = "unknown" // File size est not available
                };

                nvidiaDrivers.Add(driverObj);

                // Compares the most up to date version, and system compatible (GRD/SD/Notebook), then sets it as recommended
                bool matchesPreference = preferNotebook
                    ? string.Equals(driverType, "grd", StringComparison.OrdinalIgnoreCase) && driverTypeKey == "notebook"
                    : string.Equals(driverTypeKey, driverType, StringComparison.OrdinalIgnoreCase);
                if (matchesPreference && Version.TryParse(driver.version, out Version currentParsedVersion))
                {

                    // If we prefer notebook drivers and this is a notebook variant
                    if (preferNotebook && driverTypeKey == "notebook" && currentParsedVersion > latestNotebookVersion)
                    {
                        latestNotebookVersion = currentParsedVersion;
                        latestNotebookDriver = driverObj;
                    }

                    // Regular SD/GRD driver
                    if (currentParsedVersion > latestParsedVersion)
                    {
                        latestParsedVersion = currentParsedVersion;
                        latestDriver = driverObj;
                    }
                }
            }
        }

        // Mark the latest matching driver found as recommended
        NvidiaDriver recommendedDriver = latestNotebookDriver ?? latestDriver;

        // Metadata is ordered oldest to newest; fall back when no driver matches the preference.
        if (recommendedDriver == null && nvidiaDrivers.Count > 0)
        {
            recommendedDriver = nvidiaDrivers.LastOrDefault();
        }
        if (recommendedDriver != null) recommendedDriver.recommended = true;

        // Reverse list, because this metadata is sorted from oldest to newest, and we want the newest first
        nvidiaDrivers.Reverse();

        return nvidiaDrivers;
    }

    // Maps experimental metadata "Type" to TNUC driver type
    private static (string driverTypeKey, string driverTypeLabel) GetDriverTypeKey(string driverType)
    {
        switch (driverType?.ToLower())
        {
            case "desktop":
                return ("grd", "Game Ready Driver");
            case "studio":
                return ("sd", "Studio Driver");
            case "notebook":
                return ("notebook", "Notebook");
            default:
                return ("unknown", "Unknown");
        }
    }

    public static (List<NvidiaDriver> nvidiaDrivers, string errorCode, string releaseNotes) GetDriverMetadata(string deviceId, string driverType)
    {
        if (!LoadCombinedJsonData()) return (null, "Error parsing GPU metadata json.", null);
        (GpuDevice matchedGpu, int gpuIndex) = FindGpuDetailsByDeviceId(deviceId);

        if (matchedGpu != null)
        {
            // Finds all compatible drivers for GPU from metadata
            List<NvidiaDriver> nvidiaDrivers = FindDriversForGpu(gpuIndex, driverType);

            if (nvidiaDrivers.Count > 0)
            {
                // Release notes
                string releaseNotes = RetrieveReleaseNotes();

                // Return list
                return (nvidiaDrivers, null, releaseNotes);
            }
            else
            {
                string error = "No compatible driver was found for your GPU.";
                return (null, error, null);
            }
        }
        else
        {
            return (null, $"Your GPU is not supported by this experimental repo. Your device ID: {deviceId}", null);
        }
    }

    private static string RetrieveReleaseNotes()
    {
        try
        {
            // Parse code
            string releaseNotes = MainConsole.SendGetRequest(MainConsole.experimentalGpuMetadataRepoReleaseNotes);
            JObject parsed = JObject.Parse(releaseNotes);
            string html = parsed["result"].ToString();

            // Remove download forms
            html = Regex.Replace(html, @"<form[^>]*action\s*=\s*[""']?/download/.*?</form>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Only get the three latest release notes
            MatchCollection matches = Regex.Matches(html, @"<div class=""version[^""]*"" id=""changes-[^""]+"">.*?<\/div>", RegexOptions.Singleline);
            string limitedHtml = "";

            for (int i = 0; i < Math.Min(3, matches.Count); i++)
            {
                limitedHtml += matches[i].Value;
            }

            // Sanitize
            HtmlSanitizer sanitizer = new HtmlSanitizer();
            string sanitizedHtml = sanitizer.Sanitize(limitedHtml);

            string finalHtml = $"<html><head><meta charset=\"UTF-8\"></head><body>{sanitizedHtml}</body></html>";
            return finalHtml;
        }
        catch
        {
            return "Unable to retrieve release notes.";
        }
    }
}
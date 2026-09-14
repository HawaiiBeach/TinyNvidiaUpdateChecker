using System;
using System.Collections.Generic;
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
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static (GpuDevice matchedGpu, int deviceKey) FindGpuDetailsByDeviceId(string deviceId)
    {
        foreach (KeyValuePair<string, GpuDevice> entry in _combinedGpuData.devices)
        {
            GpuDevice device = entry.Value;
            if (device.id.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
            {
                return (device, int.Parse(entry.Key));
            }
        }
        return (null, 0);
    }

    public static List<NvidiaDriver> FindDriversForGpu(int gpuIndex, string driverType)
    {
        List<NvidiaDriver> nvidiaDrivers = new();
        Version latestParsedVersion = new(0, 0);

        foreach (DriverVersion driver in _combinedGpuData.versions)
        {
            // If the driver supports our GPU
            if (driver.supports.Contains(gpuIndex))
            {
                string driverTypeKey = GetDriverTypeKey(driver.type);
                string driverTypeLabel = driverTypeKey == "grd" ? "Game Ready Driver" : "Studio Driver";
                string downloadUrl = $"https://international.download.nvidia.com/Windows/{driver.version}/{driver.key}.exe";

                NvidiaDriver driverObj = new()
                {
                    title = $"{driver.version} - Type: {driverTypeLabel}",
                    version = driver.version,
                    type = driverTypeKey,
                    downloadUrl = downloadUrl,
                    fileSizeEst = "unknown" // File size est not available
                };

                nvidiaDrivers.Add(driverObj);

                // Does driver type match, set latest driver as recommended
                if ((driverType == "sd" && driverTypeKey == "sd") || driverType != "sd")
                {
                    if (Version.TryParse(driver.version, out Version currentParsedVersion))
                    {
                        if (currentParsedVersion > latestParsedVersion)
                        {
                            latestParsedVersion = currentParsedVersion;
                        }
                    }
                }
            }
        }

        // Mark latestParsedVersion as recommended
        NvidiaDriver latestDriver = nvidiaDrivers.Find(x =>
            x.type == driverType &&
            Version.TryParse(x.version, out Version v) &&
            v == latestParsedVersion);

        MainConsole.WriteLine(latestDriver.type);
        latestDriver.recommended = true;

        // Reverse list, because this metadata is sorted from oldest to newest, and we want the newest first
        nvidiaDrivers.Reverse();

        return nvidiaDrivers;
    }

    private static string GetDriverTypeKey(string driverType)
    {
        switch (driverType.ToLower())
        {
            case "desktop":
                return "grd";
            case "notebook":
                return "grd";
            case "studio":
                return "sd";
            default:
                return "grd";
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
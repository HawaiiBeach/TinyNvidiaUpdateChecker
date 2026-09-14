using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace TinyNvidiaUpdateChecker.Handlers
{
    public class GPUHandler
    {
        /// <summary>
        /// GPU name lookup powered by PCI Lookup
        /// </summary>
        static string PCILookupAPI = "https://www.pcilookup.com/api.php";

        /// <summary>
        /// Decides what GPU to use for driver metadata lookup. If multiple GPUs are found, the user is prompted to choose one. If no valid GPU is found, the function will return false and fall back to NewMetadataHandler.
        /// </summary>
        public static GPU GetGPU()
        {
            bool isNotebook = false;
            Regex nameRegex = new(@"(?<=NVIDIA )(.*(?= \([A-Z]+\))|.*(?= [0-9]+GB)|.*(?= with Max-Q Design)|.*(?= COLLECTORS EDITION)|.*)");
            List<int> notebookChassisTypes = [1, 8, 9, 10, 11, 12, 14, 18, 21, 31, 32];
            List<GPU> gpuList = [];

            // Check for notebook
            // TODO rewrite and identify GPUs properly
            if (MainConsole.overrideChassisType == 0)
            {
                foreach (var obj in new ManagementClass("Win32_SystemEnclosure").GetInstances())
                {
                    foreach (int chassisType in obj["ChassisTypes"] as ushort[])
                    {
                        isNotebook = notebookChassisTypes.Contains(chassisType);
                    }
                }
            }
            else
            {
                isNotebook = notebookChassisTypes.Contains(MainConsole.overrideChassisType);
            }

            // Scan computer for GPUs
            foreach (ManagementBaseObject gpu in new ManagementObjectSearcher("SELECT Name, DriverVersion, PNPDeviceID FROM Win32_VideoController").Get())
            {
                string rawGpuLabel = gpu["Name"].ToString();
                string rawVersion = gpu["DriverVersion"].ToString().Replace(".", string.Empty);
                string pnp = gpu["PNPDeviceID"].ToString();

                // Is it a GPU?
                if (pnp.Contains("&DEV_"))
                {
                    string[] split = pnp.Split("&DEV_");
                    string vendorId = split[0][^4..].ToLower();
                    string deviceId = split[1][..4];

                    // Are drivers installed for this GPU? If not Windows reports a generic GPU name which is not sufficient
                    if (Regex.IsMatch(rawGpuLabel, @"^NVIDIA") && nameRegex.IsMatch(rawGpuLabel))
                    {
                        string gpuLabel = nameRegex.Match(rawGpuLabel).Value.Trim().Replace("Super", "SUPER");
                        string cleanVersion = rawVersion.Substring(rawVersion.Length - 5, 5).Insert(3, ".");

                        gpuList.Add(new GPU(gpuLabel, cleanVersion, vendorId, deviceId, true, isNotebook));
                    }

                    // Name regex does not match, but the vendor is NVIDIA, revert to NewMetadataHandler
                    else if (vendorId == "10de")
                    {
                        // Use API lookup to find GPU label
                        // Otherwise, if system has multiple NVIDIA GPUs, the "choose GPU" dialog will show multiple GPUs with the generic driver
                        string gpuLabel = LookupGpuLabel(vendorId, deviceId, rawGpuLabel);
                        gpuList.Add(new GPU(gpuLabel, "000.00", vendorId, deviceId, false, isNotebook));
                    }
                }
            }

            int gpuCount = gpuList.Where(x => x.isValidated).Count();

            // Was any validated GPU found?
            if (gpuCount > 0)
            {

                // More than one valid GPU was found, prompt user to choose the proper GPU mapped by device ID
                if (gpuCount > 1)
                {

                    // Retrieve GPU ID from config, or prompts user to choose, if config is not found
                    string configGpuId = ConfigurationHandler.ReadSetting("GPU ID", gpuList).ToLower();

                    // Validate that the GPU ID is still active on this system
                    foreach (GPU gpu in gpuList.Where(x => x.isValidated))
                    {
                        if (gpu.deviceId.ToLower() == configGpuId)
                        {
                            return gpu;
                        }
                    }

                    // GPU ID is no longer active on this system, prompt user to choose new GPU
                    configGpuId = ConfigurationHandler.SetupSetting("GPU ID", gpuList);

                    foreach (GPU gpu in gpuList.Where(x => x.isValidated))
                    {
                        if (gpu.deviceId.ToLower() == configGpuId)
                        {
                            return gpu;
                        }
                    }
                }
                else
                {
                    // Only one GPU was found on the system
                    GPU firstGpu = gpuList.Where(x => x.isValidated).First();
                    return firstGpu;
                }
            }

            // No valid GPU was found
            MainConsole.Write("ERROR!");
            MainConsole.WriteLine();

            if (!gpuList.Any(x => x.vendorId == "10de"))
            {
                MainConsole.WriteLine("No NVIDIA GPU was detected on this system.");
                MainConsole.WriteLine();
            }
            else
            {
                MainConsole.WriteLine("GPU metadata lookup failed!");
                MainConsole.WriteLine();
                MainConsole.WriteLine("Found GPUs:");

                foreach (GPU gpu in gpuList)
                {
                    MainConsole.WriteLine($"GPU Name: '{gpu.name}' | VendorId: {gpu.vendorId} | DeviceId: {gpu.deviceId} | IsNotebook: {gpu.isNotebook}");
                }

                MainConsole.WriteLine();
            }

            // Return no GPU
            return null;
        }

        /// <summary>
        /// Uses PCI Lookup API to get a GPU label
        /// </summary>
        /// <returns>Found GPU label</returns>
        public static string LookupGpuLabel(string vendorID, string deviceID, string rawGpuLabel)
        {
            try
            {
                Regex apiRegex = new(@"([A-Za-z0-9]+( [A-Za-z0-9]+)+)");

                string url = $"{PCILookupAPI}?action=search&vendor={vendorID}&device={deviceID}";
                string rawData = MainConsole.SendGetRequest(url);
                PCILookupClassRoot apiResponse = JsonConvert.DeserializeObject<PCILookupClassRoot>(rawData);

                if (apiResponse != null && apiResponse.Count > 0)
                {
                    string rawName = apiResponse[0].desc;

                    if (apiRegex.IsMatch(rawName))
                    {
                        string foundLabel = apiRegex.Match(rawName).Value.Trim();

                        return foundLabel;
                    }
                }
            }
            catch { }

            return rawGpuLabel;
        }
    }
}

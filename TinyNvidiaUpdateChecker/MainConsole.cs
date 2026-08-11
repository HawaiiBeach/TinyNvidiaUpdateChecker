using Ganss.Xss;
using HtmlAgilityPack;
using HttpClientProgress;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TinyNvidiaUpdateChecker.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker
{

    class MainConsole
    {
        /// <summary>
        /// GPU metadata repo
        /// </summary>
        public readonly static string gpuMetadataRepo = "https://github.com/ZenitH-AT/nvidia-data/raw/main";

        /// <summary>
        /// GPU metadata repo by NVCleanstall, credit to them and their work
        /// </summary>
        public readonly static string experimentalGpuMetadataRepo = "https://gpu.me/v1/index2.json";

        /// <summary>
        /// GPU metadata repo (release notes) by Techpowerup, credit to them and their work
        /// </summary>
        public readonly static string experimentalGpuMetadataRepoReleaseNotes = "https://www.techpowerup.com/download/nvidia-geforce-graphics-drivers/changes";

        /// <summary>
        /// URL for client update
        /// </summary>
        public readonly static string updateUrl = "https://api.github.com/repos/HawaiiBeach/TinyNvidiaUpdateChecker/releases/latest";

        /// <summary>
        /// URL for NVIDIA Ajax API
        /// </summary>
        public readonly static string nvidiaAjaxURL = "https://gfwsl.geforce.com/services_toolkit/services/com/nvidia/services/AjaxDriverService.php?func=DriverManualLookup";

        /// <summary>
        /// Current client version
        /// </summary>
        public static string offlineVer = Application.ProductVersion;

        /// <summary>
        /// Remote client version
        /// </summary>
        public static string onlineVer;

        /// <summary>
        /// Current GPU driver version
        /// </summary>
        public static string OfflineGPUVersion;

        /// <summary>
        /// Remote GPU driver version
        /// </summary>
        public static string OnlineGPUVersion;

        /// <summary>
        /// Show UI or go quiet mode
        /// </summary>
        public static bool showUI = true;

        /// <summary>
        /// Disable "Press any key to exit..." prompt
        /// </summary>
        public static bool noPrompt = false;

        /// <summary>
        /// Dry run
        /// </summary>
        public static bool dryRun = false;    

        /// <summary>
        /// Enable extended information
        /// </summary>
        public static bool debug = false;

        /// <summary>
        /// Force a prompt to download GPU drivers
        /// </summary>
        private static bool forceDL = false;

        /// <summary>
        /// Will automaticly download and install drivers
        /// </summary>
        public static bool confirmDL = false;

        /// <summary>
        /// If this value is set then it will override the default configuration file location
        /// </summary>
        public static string overrideConfigFileLocation = null;

        /// <summary>
        /// Override chassis type. Used for example notebook systems with desktop e-GPUs
        /// </summary>
        public static int overrideChassisType = 0;

        /// <summary>
        /// Has the intro been displayed? Because we do not want to display the intro multiple times.
        /// </summary>
        private static bool hasRunIntro = false;

        public static HttpClient httpClient = new();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        static bool debuggerAttached = Debugger.IsAttached;

        static bool consoleAttached = false;

        [STAThread]
        private static void Main(string[] args)
        {
            string message = $"TinyNvidiaUpdateChecker v{offlineVer}";
            
            CheckArgs(args);

            if (showUI && !debuggerAttached) {
                if (GetConsoleWindow() == IntPtr.Zero) {
                    bool success = AttachConsole(ATTACH_PARENT_PROCESS);
                    consoleAttached = true;

                    if (success) {
                        WriteLine();
                        noPrompt = true; // no prompt needed, we are in existing console
                    } else {
                        AllocConsole();
                    }
                }

                Console.Title = message;

                if (!debug) {
                    GenericHandler.DisableQuickEdit();
                }
            } else if (!showUI && !debuggerAttached) {
                FreeConsole();
            }

            RunIntro();

            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36");

            ConfigurationHandler.ConfigInit(overrideConfigFileLocation);

            CheckDependencies();

            if (ConfigurationHandler.ReadSettingBool("Check for Updates")) {
                UpdateHandler.SearchForUpdate(args);
            }

            Write("Retrieving GPU information . . . ");
            DriverMetadata metadata;
            GPU gpu;
            int osId;
            string driverType = ConfigurationHandler.ReadSetting("Driver type");
            string useExperimental = ConfigurationHandler.ReadSetting("Use Experimental Metadata", null, false);

            if (useExperimental == "true") {
                (gpu, osId, bool success) = OldMetadataHandler.GetDriverMetadata(false, true);
                (metadata, string error) = NewMetadataHandler.GetDriverMetadata(gpu.deviceId, driverType);

                if (metadata == null) {
                    Write("ERROR!");
                    WriteLine();
                    WriteLine("GPU metadata lookup using NewMetadataHandler failed. TNUC can not continue.");
                    WriteLine($"Error reason: {error}");
                    callExit(1);
                }
            } else {
                OldMetadataHandler.PrepareCache();
                (gpu, osId, bool success) = OldMetadataHandler.GetDriverMetadata();

                // If Old was able to lookup GPU data
                if (success)
                {
                    JObject downloadInfo = GetDriverDownloadInfo(gpu.pfId, osId, gpu.isDch, driverType);
                    metadata = new();

                    string tempUrl = downloadInfo["DownloadURL"].ToString();
                    // Some GPUs return URL with HTTP and not HTTPS
                    tempUrl = tempUrl.StartsWith("https://") ? tempUrl[10..] : tempUrl[9..];
                    metadata.downloadUrl = $"https://international{tempUrl}";
                    metadata.version = downloadInfo["Version"].ToString();
                    metadata.releaseDate = DateTime.Parse(downloadInfo["ReleaseDateTime"].ToString());

                    // Get driver type/platform
                    metadata.platform = (driverType == "grd") ? (!gpu.isNotebook ? "Desktop" : "Notebook") : "Studio";

                    // Santitize release notes, and get PDF
                    string tempNotes = Uri.UnescapeDataString(downloadInfo["ReleaseNotes"].ToString());

                    // Cleanup release description
                    var htmlDocument = new HtmlAgilityPack.HtmlDocument();
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

                    // Save the cleaned release description
                    tempNotes = htmlDocument.DocumentNode.OuterHtml;

                    // Sanitize it
                    HtmlSanitizer sanitizer = new HtmlSanitizer();
                    string sanitizedHtml = sanitizer.Sanitize(tempNotes);

                    // Finally set new release description
                    metadata.releaseNotes = sanitizedHtml;

                    // Get file size in bytes
                    using (var request = new HttpRequestMessage(HttpMethod.Head, metadata.downloadUrl))
                    {
                        using var response = httpClient.Send(request);
                        response.EnsureSuccessStatusCode();
                        metadata.fileSize = response.Content.Headers.ContentLength.Value;
                    }

                    // Get PDF release notes
                    var otherNotes = Uri.UnescapeDataString(downloadInfo["OtherNotes"].ToString());

                    htmlDocument.LoadHtml(otherNotes);
                    IEnumerable<HtmlNode> node = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));

                    foreach (var child in node)
                    {
                        if (child.Attributes["href"].Value.Contains("release-notes.pdf"))
                        {
                            metadata.pdfUrl = child.Attributes["href"].Value.Trim();
                            break;
                        }
                    }
                } else {
                    Write("Now loading NewMetadataHandler. . . ");
                    // OldMetadataHandler failed lookup, revert to NewMetadataHandler
                    (gpu, osId, bool success2) = OldMetadataHandler.GetDriverMetadata(false, true);

                    if (gpu == null)
                    {
                        WriteLine("TNUC can not continue.");
                        callExit(1);
                    }

                    (metadata, string error) = NewMetadataHandler.GetDriverMetadata(gpu.deviceId, driverType);

                    if (metadata == null)
                    {
                        Write("ERROR!");
                        WriteLine();
                        WriteLine("GPU metadata lookup using NewMetadataHandler failed. TNUC can not continue.");
                        WriteLine($"Error reason: {error}");
                        callExit(1);
                    }
                }
            }

            OfflineGPUVersion = gpu.version;
            OnlineGPUVersion = metadata.version;

            Write("OK!");
            WriteLine();

            if (debug) {
                WriteLine($"pfId:        {gpu.pfId}");
                WriteLine($"osId:        {osId}");
                WriteLine($"isDchDriver: {gpu.isDch}");
                WriteLine($"downloadURL: {metadata.downloadUrl}");
                WriteLine($"pdfURL:      {metadata.pdfUrl}");
                WriteLine($"releaseDate: {metadata.releaseDate.ToShortDateString()}");
                WriteLine($"downloadFileSize:  {Math.Round((metadata.fileSize / 1024f) / 1024f)} MiB");
                WriteLine($"OfflineGPUVersion: {OfflineGPUVersion}");
                WriteLine($"OnlineGPUVersion:  {OnlineGPUVersion}");
            }

            var updateAvailable = false;

            Version.TryParse(OfflineGPUVersion, out Version vOffline);
            Version.TryParse(OnlineGPUVersion, out Version vOnline);
            int comparison = vOffline.CompareTo(vOnline);

            if (comparison == 0) {
                WriteLine("There is no new GPU driver available, you are up to date.");
            } else if (comparison > 0) {
                WriteLine("Your current GPU driver is newer than what NVIDIA reports!");
            } else {
                WriteLine("There is a new GPU driver available to download!");
                updateAvailable = true;
            }

            if ((updateAvailable || forceDL) && !dryRun) {
                if (confirmDL) {
                    DownloadDriverQuiet(true, metadata);
                } else {
                    PromptAvailableUpdate(metadata);
                }
            }

            callExit(0);
        }

        /// <summary>
        /// Handles the command line arguments </summary>
        /// <param name="args"> Command line arguments in. Turned out that Environment.GetCommandLineArgs() wasn't any good.</param>
        private static void CheckArgs(string[] args)
        {

            /// The command line argument handler does it's work here,
            /// for a list of available arguments, use the '--help' argument.

            foreach (var arg in args)
            {

                // no window
                if (arg.ToLower() == "--quiet") {
                    showUI = false;
                }

                else if (arg.ToLower() == "--noprompt") {
                    noPrompt = true;
                }

                else if (arg.ToLower() == "--dry-run") {
                    dryRun = true;
                }

                // erase config
                else if (arg.ToLower() == "--erase-config") {
                    if (File.Exists(ConfigurationHandler.configFilePath)) {
                        try {
                            File.Delete(ConfigurationHandler.configFilePath);
                        } catch (Exception ex) {
                            RunIntro();
                            WriteLine(ex.ToString());
                            WriteLine();
                        }
                    }
                }

                // enable debugging
                else if (arg.ToLower() == "--debug") {
                    debug = true;
                }

                // force driver download
                else if (arg.ToLower() == "--force-dl") {
                    forceDL = true;
                }

                // show version number
                else if (arg.ToLower() == "--version") {
                    RunIntro();
                    WriteLine($"Current version is {offlineVer}");
                    WriteLine();
                }

                // automaticly download driver
                else if (arg.ToLower() == "--confirm-dl") {
                    confirmDL = true;
                }

                // override the config path to working directory
                else if (arg.ToLower() == "--config-here") {
                    overrideConfigFileLocation = Path.Combine(Directory.GetCurrentDirectory(), "app.config");
                }

                // overide config file path
                else if (arg.StartsWith("--config-override=")) {
                    var locationArg = arg.Substring(18);
                    overrideConfigFileLocation = locationArg;
                }

                // delete old file
                else if (arg.ToLower() == "--cleanup-update") {
                    File.Delete(Path.GetFullPath(Environment.ProcessPath) + ".old");
                }

                // help menu
                else if (arg.ToLower() == "--help") {
                    RunIntro();
                    WriteLine($"Usage: {Path.GetFileName(Environment.ProcessPath)} [ARGS]");
                    WriteLine();
                    WriteLine("--quiet                      Runs the application quietly in the background, and will only notify the user if an update is available.");
                    WriteLine("--noprompt                   Runs the application without prompting to exit.");
                    WriteLine("--dry-run                    Perform a dry run.");
                    WriteLine("--erase-config               Erase configuration file.");
                    WriteLine("--debug                      Turn debugging on, will output more information that can be used for debugging.");
                    WriteLine("--force-dl                   Force prompt to download drivers, even if the user is up-to-date - should only be used for debugging.");
                    WriteLine("--version                    View version.");
                    WriteLine("--confirm-dl                 Automatically download and install the driver quietly without any user interaction at all. should be used with '--quiet' for the optimal solution.");
                    WriteLine("--config-here                Use the working directory as path to the configuration file.");
                    WriteLine("--config-override=<path>     Override configuration file location with absolute file path.");
                    WriteLine("--help                       Shows help.");
                    WriteLine("--override-desktop           Override automatic desktop/notebook identification.");
                    WriteLine("--override-notebook          Override automatic desktop/notebook identification.");
                    Environment.Exit(0);
                }

                else if (arg.ToLower() == "--override-desktop") {
                    overrideChassisType = 3;
                }

                else if (arg.ToLower() == "--override-notebook") {
                    overrideChassisType = 9;
                }

                // unknown command, right?
                else
                {
                    RunIntro();
                    WriteLine($"Unknown command '{arg}', type --help for help.");
                    WriteLine();
                }
            }

            // show the args if debug mode
            if (debug) {
                foreach (var arg in args) {
                    RunIntro();
                    WriteLine($"Arg: {arg}");
                }
                WriteLine();
            }
        }

        private static JObject GetDriverDownloadInfo(int pfId, int osId, bool isDchDriver, string driverType) {
            isDchDriver = false;
            try {
                // Driver type (upCRD)
                // - 0 is Game Ready Driver (GRD)
                // - 1 is Studio Driver (SD)
                int driverTypeInt = driverType == "grd" ? 0 : 1;

                // DCH driver
                int dchDriverInt = isDchDriver ? 1 : 0;

                // Constructs the driver URL
                // Note: the last character is the DCH switch, and used for auto-upgrade
                string ajaxDriverURL = nvidiaAjaxURL;
                ajaxDriverURL += $"&pfid={pfId}&osID={osId}&upCRD={driverTypeInt}&dch={dchDriverInt}";

                // Sends a GET request, and parses the response into JObject
                JObject nvResponse = JObject.Parse(SendGetRequest(ajaxDriverURL));

                // GPU driver was found
                if ((int)nvResponse["Success"] == 1) {

                    // If the operating system has support for DCH drivers, and DCH drivers are currently not installed, then serach for DCH drivers, too.
                    // Non-DCH drivers are discontinued. Not searching for DCH drivers will result in users having outdated graphics drivers, and we don't want that.
                    if (Environment.OSVersion.Version.Build > 10240 && !isDchDriver) {
                        ajaxDriverURL = ajaxDriverURL[..^1] + "1";
                        JObject nvResponseDCH = JObject.Parse(SendGetRequest(ajaxDriverURL));

                        if ((int)nvResponseDCH["Success"] == 1) {
                            return (JObject)nvResponseDCH["IDS"][0]["downloadInfo"];
                        }
                    }

                    return (JObject)nvResponse["IDS"][0]["downloadInfo"];

                // No GPU driver was found, and we are not identified as DCH
                // Auto-upgrade to DCH and search for drivers again
                } else if ((int)nvResponse["Success"] == 0 && Environment.OSVersion.Version.Build > 10240 && !isDchDriver) {
                    ajaxDriverURL = ajaxDriverURL[..^1] + "1";
                    JObject nvResponseDCH = JObject.Parse(SendGetRequest(ajaxDriverURL));

                    // Auto-upgrade DCH was successful
                    if ((int)nvResponseDCH["Success"] == 1) {
                        return (JObject)nvResponseDCH["IDS"][0]["downloadInfo"];
                    }

                    // Auto-upgrade DCH failed, meaning no DCH or non-DCH driver found.
                    // If the current driverType is SD, TNUC will prompt to chance to GRD
                    // TODO: don't require restart for changes to apply
                    throw new ArgumentOutOfRangeException();
                } else {
                    throw new ArgumentOutOfRangeException();
                }
            } catch (ArgumentOutOfRangeException) {
                Write("ERROR!");
                WriteLine();
                WriteLine("No NVIDIA driver was found for your system configuration.");
                WriteLine();
                WriteLine("Debugging information:");
                WriteLine($"pfId:        {pfId}");
                WriteLine($"osId:        {osId}");
                WriteLine($"isDchDriver: {isDchDriver}");
                WriteLine($"driverType:  {driverType}");

                // Ask user to switch to GRD driver
                if (driverType == "sd") {
                    WriteLine();
                    WriteLine("NOTICE: you have selected Studio Drivers (SD)");

                    TaskDialogButton[] buttons = [
                        new("Change to Game Ready Driver (GRD)") { Tag = "change" },
                        new("No") { Tag = "no" }
                    ];

                    string text = @"No driver was found for your system and you have choosen Studio Drivers." +
                        Environment.NewLine + Environment.NewLine +
                        "TNUC does currently not support searching for GRD and SD drivers at the same time." +
                        Environment.NewLine + Environment.NewLine +
                        "Do you wish to change driver type to Game Ready Drivers (GRD)?";

                    string result = ConfigurationHandler.ShowButtonDialog("Change driver type?", text, TaskDialogIcon.Warning, buttons);

                    if (result == "change") {
                        ConfigurationHandler.SetSetting("Driver type", "grd");
                        WriteLine("The driver type has now been changed to Game Ready Driver (GRD). Restart for changes to apply");
                    }
                }
            } catch (Exception ex) {
                Write("ERROR!");
                WriteLine();
                WriteLine("Unable to interact with NVIDIA API.");
                WriteLine();
                WriteLine(ex.ToString());
            }

            callExit(1);
            return null;
        }

        public static string SendGetRequest(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = httpClient.Send(request);
            response.EnsureSuccessStatusCode();

            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Check if dependencies are all OK
        /// </summary>
        private static void CheckDependencies()
        {

            // Check internet connection
            Write("Verifying internet connection . . . ");

            if (NetworkInterface.GetIsNetworkAvailable()) {
                Write("OK!");
                WriteLine();
            } else {
                Write("ERROR!");
                WriteLine();
                WriteLine("You are not connected to the internet!");
                callExit(2);
            }

            if (ConfigurationHandler.ReadSettingBool("Minimal install")) {
                if (LibraryHandler.EvaluateLibrary() == null) {
                    WriteLine("No compatible extract library was detected on the system. The minimal install feature has been disabled.");
                    ConfigurationHandler.SetSetting("Minimal install", "false");
                }
            }

            WriteLine();
        }

        /// <summary>
        /// Prompt for available GPU update with UI
        /// </summary>
        private static void PromptAvailableUpdate(DriverMetadata metadata)
        {
            DriverDialog.ShowGUI(metadata);

            if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLEXTRACT) {
                // download and save (and extract)

                string driverFileName = metadata.downloadUrl.Split('/').Last(); // retrives file name from url
                string savePath = "";

                try {
                    string title = "Choose download location";

                    if (ConfigurationHandler.ReadSettingBool("Minimal install")) {
                        title += " (you should select an empty folder)";
                    }

                    using var dialog = new FolderBrowserDialog {
                        Description = title,
                        UseDescriptionForTitle = true,
                        ShowNewFolderButton = true
                    };

                    if (dialog.ShowDialog() == DialogResult.OK) {
                        savePath = dialog.SelectedPath + @"\";
                    } else {
                        PromptAvailableUpdate(metadata);
                        return;
                    }

                    string finalPath = savePath + driverFileName;

                    if (File.Exists(finalPath) && !DoesDriverFileSizeMatch(finalPath, metadata.fileSize)) {
                        File.Delete(finalPath);
                    }

                    // don't download driver if it already exists
                    WriteLine();
                    Write("Downloading the driver . . . ");
                    if (showUI && !File.Exists(finalPath)) {
                        HandleDownload(metadata.downloadUrl, finalPath).GetAwaiter().GetResult();
                    }

                    // show progress bar gui if quiet
                    else if (!showUI && !File.Exists(finalPath)) {
                        using var dlForm = new DownloaderForm(metadata.downloadUrl, finalPath);
                        dlForm.ShowDialog();
                        if (dlForm.Error != null) throw dlForm.Error;
                    }

                } catch (Exception ex) {
                    WriteLine();
                    Write("ERROR!");
                    WriteLine();
                    WriteLine("Driver download failed.");
                    WriteLine();
                    WriteLine(ex.ToString());
                    WriteLine();
                    callExit(1);
                }

                Write("OK!");
                WriteLine();

                if (debug) {
                    WriteLine($"savePath: {savePath}");
                }

                if (ConfigurationHandler.ReadSettingBool("Minimal install")) {
                    MakeInstaller(false, savePath, driverFileName);
                }
            } else if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLINSTALL) {
                DownloadDriverQuiet(confirmDL, metadata);
                
            } else if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLINSTALLCUSTOM) {
                string title = "Choose download location";

                if (ConfigurationHandler.ReadSettingBool("Minimal install"))
                {
                    title += " (you should select an empty folder)";
                }

                using var dialog = new FolderBrowserDialog
                {
                    Description = title,
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DownloadDriverQuiet(confirmDL, metadata, dialog.SelectedPath + @"\", true);
                }
                else
                {
                    PromptAvailableUpdate(metadata);
                    return;
                }
            }
        }

        /// <summary>
        /// Downloads and installs the driver without user interaction
        /// </summary>
        private static void DownloadDriverQuiet(bool minimized, DriverMetadata metadata, string overrideDownloadLocation = null, bool keepDriver = false)
        {
            string driverFileName = metadata.downloadUrl.Split('/').Last(); // retrives file name from url
            string savePath = overrideDownloadLocation ?? Path.GetTempPath();

            string FULL_PATH_DIRECTORY = overrideDownloadLocation ?? savePath + OnlineGPUVersion + @"\";
            string FULL_PATH_DRIVER = FULL_PATH_DIRECTORY + driverFileName;

            savePath = FULL_PATH_DIRECTORY;

            Directory.CreateDirectory(FULL_PATH_DIRECTORY);

            if (File.Exists(FULL_PATH_DRIVER) && !DoesDriverFileSizeMatch(FULL_PATH_DRIVER, metadata.fileSize)) {
                File.Delete(savePath + driverFileName);
            }

            if (!File.Exists(FULL_PATH_DRIVER)) {
                Write("Downloading the driver . . . ");

                if (showUI || confirmDL) {
                    try {
                        HandleDownload(metadata.downloadUrl, FULL_PATH_DRIVER).GetAwaiter().GetResult();

                        Write("OK!");
                        WriteLine();
                    } catch (Exception ex) {
                        Write("ERROR!");
                        WriteLine();
                        WriteLine(ex.ToString());
                        WriteLine();
                        callExit(1);
                    }
                } else {
                    using var dlForm = new DownloaderForm(metadata.downloadUrl, FULL_PATH_DRIVER);
                    dlForm.ShowDialog();
                    if (dlForm.Error != null) throw dlForm.Error;
                }
            }

            bool minimalInstaller = ConfigurationHandler.ReadSettingBool("Minimal install");

            if (minimalInstaller) {
                MakeInstaller(minimized, FULL_PATH_DIRECTORY, driverFileName);
            }

            try {
                WriteLine();
                Write("Executing driver installer . . . ");

                string fileName = minimalInstaller ? FULL_PATH_DIRECTORY + "setup.exe" : FULL_PATH_DRIVER;

                ProcessStartInfo startInfo = new(fileName) {
                    UseShellExecute = true
                };

                if (minimized) {
                    startInfo.Arguments = "/s /noreboot";
                }

                Process.Start(startInfo).WaitForExit();
                Write("OK!");
            } catch (Exception ex) {
                WriteLine("An error occurred preventing the driver installer to execute!");
                WriteLine();
                WriteLine(ex.ToString());
                callExit(1);
            }

            WriteLine();

            if (!keepDriver)
            {
                try
                {
                    Directory.Delete(FULL_PATH_DIRECTORY, true);
                    WriteLine($"Cleaned up: {FULL_PATH_DIRECTORY}");
                }
                catch
                {
                    WriteLine($"Could not cleanup: {FULL_PATH_DIRECTORY}");
                }
            }
        }

        /// <summary>
        /// Shared method for the accual downloading of a file with the command line progress bar.
        /// </summary>
        /// <param name="url">URL path for download</param>
        /// <param name="path">Absolute file path</param>
        /// <returns></returns>
        async public static Task HandleDownload(string url, string path, EventHandler<float> progressHandle = null)
        {
            // if a partial file download exists, delete it now
            if (File.Exists(path)) {
                File.Delete(path);
            }

            path += ".part"; // add 'partial' to file name making it easier to identify as an incomplete download

            // if a partial file download exists, delete it now
            if (File.Exists(path)) {
                File.Delete(path);
            }

            Progress<float> progress = new();
            Handlers.ProgressBar progressBar = null;

            if (progressHandle == null) {
                progressBar = new();

                progress.ProgressChanged += delegate (object sender, float progress) {
                    progressBar.Report(progress / 100);
                };
            } else {
                progress.ProgressChanged += progressHandle;
            }

            try {
                using (FileStream file = new(path, FileMode.Create, FileAccess.Write, FileShare.None))  {
                    await httpClient.DownloadDataAsync(url, file, progress);
                }

                File.Move(path, path[..^5]); // rename back
                if (progressHandle == null ) { progressBar.Dispose(); }
            } catch {
                File.Delete(path);
                if (progressHandle == null) { progressBar.Dispose(); }
                throw;
            }
        }

        /// <summary>
        /// Remove telementry and only extract basic drivers
        /// </summary>
        private static void MakeInstaller(bool silent, string savePath, string fileName)
        {
            WriteLine();
            Write("Extracting drivers . . . ");

            LibraryFile libraryFile = LibraryHandler.EvaluateLibrary();
            using var process = new Process();
            LibraryHandler.Library library = libraryFile.LibraryName();

            // Extract full driver to then analyze
            if (library == LibraryHandler.Library.WINRAR) {
                process.StartInfo = new ProcessStartInfo {
                    FileName = libraryFile.GetInstallationDirectory() + "winrar.exe",
                    WorkingDirectory = savePath,
                    Arguments = @$"x {fileName} -y temp\",
                    UseShellExecute = false
                };

                if (silent) process.StartInfo.Arguments += " -ibck";
            } else if (library == LibraryHandler.Library.SEVENZIP) {
                process.StartInfo = new ProcessStartInfo {
                    WorkingDirectory = savePath,
                    Arguments = $"x {fileName} -otemp -y",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (silent) {
                    process.StartInfo.FileName = libraryFile.GetInstallationDirectory() + "7z.exe";
                } else {
                    process.StartInfo.FileName = libraryFile.GetInstallationDirectory() + "7zG.exe";
                }
            } else if (library == LibraryHandler.Library.NANAZIP) {
                process.StartInfo = new ProcessStartInfo {
                    WorkingDirectory = savePath,
                    Arguments = $"x {fileName} -otemp -y",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (silent) {
                    process.StartInfo.FileName = "NanaZipC.exe";
                } else {
                    process.StartInfo.FileName = "NanaZipG.exe";
                }
            }

            try {
                process.Start();
                process.WaitForExit();
            } catch (Exception ex) {
                Write("ERROR!");
                WriteLine();
                WriteLine(ex.ToString());
                callExit(1);
            }

            // If extraction was unsuccessful
            if (process.ExitCode != 0)
            {
                Write("ERROR!");
                WriteLine();
                WriteLine("Driver extraction was canceled. Please rerun TNUC, and disable the Minimal Install feature if the issue doesn't go away.");
                WriteLine();
                WriteLine("Minimal Install info:");
                WriteLine($"Library: {library}");
                WriteLine($"Installation Directory: { libraryFile.GetInstallationDirectory()}");
                WriteLine($"Exit code: {process.ExitCode}");
                callExit(1);
            }

            string extractedPath = $"{savePath}temp";

            // If extraction was successful (code 0), but extract directory was not found
            if (!Directory.Exists(extractedPath)) {
                Write("ERROR!");
                WriteLine();
                WriteLine("The driver archive extracted without error but no files were found. Please rerun TNUC, and disable the Minimal Install feature if the issue doesn't go away.");
                WriteLine();
                WriteLine("Minimal Install info:");
                WriteLine($"Library: {library}");
                WriteLine($"Installation Directory: {libraryFile.GetInstallationDirectory()}");
                WriteLine($"Expected extract path: {extractedPath}");
                callExit(1);
            }

            // Analyze with ComponentHandler
            List<Component> driverComponents = ComponentHandler.ParseComponentData(extractedPath);

            // If config entry exists, show "Use last used components" button
            string configComponentsString = ConfigurationHandler.ReadSetting("Minimal install components", null, false);

            ComponentChooserForm componentForm = new();

            // Open component form
            // If quiet mode + configComponents exists, it will not show dialog, and will use configComponents
            (List<string> chosenComponents, bool saveConfig) =
                componentForm.OpenForm(driverComponents, configComponentsString);


            // Save latest used components to config file if user selected "Save selection"
            if (saveConfig) {
                ConfigurationHandler.SetSetting("Minimal install components", string.Join(", ", chosenComponents));
            }

            string[] extractFiles = [.. chosenComponents, "NVI2", "EULA.txt", "license.txt", "ListDevices.txt", "setup.cfg", "setup.exe"];

            foreach (string file in extractFiles) {
                string filePath = Path.Combine(savePath, "temp", file);

                if (File.Exists(filePath))
                {
                    try
                    {
                        string destinationFilePath = Path.Combine(savePath, file);
                        File.Move(filePath, destinationFilePath);
                    }
                    catch { }
                }
                else if (Directory.Exists(filePath))
                {
                    try
                    {
                        string destinationDirectoryPath = Path.Combine(savePath, Path.GetFileName(filePath));
                        Directory.Move(filePath, destinationDirectoryPath);
                    }
                    catch { }
                }
            }

            Directory.Delete(Path.Combine(savePath, "temp"), true);

            // Remove new EULA files from the installer config, or else the installer throws error codes
            // author https://github.com/cywq
            var xmlDocument = new XmlDocument();
            string setupFile = savePath + "setup.cfg";
            string[] linesToDelete = { "${{EulaHtmlFile}}", "${{FunctionalConsentFile}}", "${{PrivacyPolicyFile}}" };

            xmlDocument.Load(setupFile);

            foreach (var line in linesToDelete) {
                var node = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode($"/setup/manifest/file[@name=\"{line}\"]");

                if (node != null) {
                    node.ParentNode.RemoveChild(node);
                }
            }

            xmlDocument.Save(setupFile);

            // Disable telemetry and installer ads
            var presentationsXml = new XmlDocument();
            string presentationsFile = Path.Combine(savePath, "NVI2", "presentations.cfg");
            string[] urlsToEmpty = { "ProgressPresentationUrl", "ProgressPresentationSelectedPackageUrl" };

            if (File.Exists(presentationsFile)) {
                presentationsXml.Load(presentationsFile);

                foreach (var urlName in urlsToEmpty) {
                    var urlNode = (XmlElement)presentationsXml.DocumentElement.SelectSingleNode($"/presentations/properties/string[@name=\"{urlName}\"]");

                    if (urlNode != null) {
                        urlNode.SetAttribute("value", "");
                    }
                }

                presentationsXml.Save(presentationsFile);
            }

            Write("OK!");
            WriteLine();
        }

        /// <summary>
        /// Intro with legal message, moved to reduce lines that ultimately does the same thing.
        /// </summary>
        private static void RunIntro()
        {
            if (!hasRunIntro) {
                hasRunIntro = true;
                //WriteLine($"TinyNvidiaUpdateChecker v{offlineVer} dev build");
                WriteLine($"TinyNvidiaUpdateChecker v{offlineVer}");
                WriteLine();
            }
        }

        /// <summary>
        /// Check for passed argument and prompt for exit if applicable
        /// </summary>
        /// 
        public static void callExit(int exitNum)
        {
            if (!noPrompt)
            {
                WriteLine();
                WriteLine("Press any key to exit...");
            }

            if (showUI & !noPrompt) Console.ReadKey(true);
            FreeConsole();
            Environment.Exit(exitNum);
        }
        
        private static bool DoesDriverFileSizeMatch(string absoluteFilePath, long fileSize) {
            return new FileInfo(absoluteFilePath).Length == fileSize;
        }

        public static void Write(string value = "")
        {
            if (!showUI) return;
            if (!consoleAttached) AttachConsole();
            if (debuggerAttached) AllocConsole();
            Console.Write(value);
        }

        public static void WriteLine(string value = "")
        {
            if (!showUI) return;
            if (!consoleAttached) AttachConsole();
            if (debuggerAttached) AllocConsole();
            Console.WriteLine(value);
        }

        private static void AttachConsole()
        {
            if (GetConsoleWindow() == IntPtr.Zero) {
                bool success = AttachConsole(ATTACH_PARENT_PROCESS);
                consoleAttached = true;

                if (success) {
                    WriteLine();
                } else {
                    AllocConsole();
                }
            }
        }
    }
}

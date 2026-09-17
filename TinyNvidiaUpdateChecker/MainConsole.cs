using HttpClientProgress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
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
        /// Perform automatic download and install
        /// </summary>
        public static bool confirmDL = false;

        /// <summary>
        /// Use a local driver on system instead of downloading
        /// </summary>
        public static bool useLocalDriver = false;

        /// <summary>
        /// Local driver path
        /// </summary>
        public static string localDriverPath = null;

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
            ApplicationConfiguration.Initialize();
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

            // Spoof HTML user agent to avoid 403 forbidden errors when downloading drivers
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36");

            ConfigurationHandler.ConfigInit(overrideConfigFileLocation);

            CheckDependencies();

            if (ConfigurationHandler.ReadSettingBool("Check for Updates")) {
                UpdateHandler.SearchForUpdate(args);
            }

            // If the user has specified to use a local driver, use custom flow
            if (useLocalDriver) {
                localDriverInstall();
            }

            Write("Retrieving GPU information . . . ");

            GPU gpu = GPUHandler.GetGPU();
            if (gpu == null)
            {
                WriteLine("Unable to retrieve an NVIDIA GPU for driver lookup.");
                callExit(1);
                return;
            }
            string driverType = ConfigurationHandler.ReadSetting("Driver type");
            bool useExperimental = ConfigurationHandler.ReadSetting("Use Experimental Metadata", null, false) == "true";

            (List<NvidiaDriver> nvidiaDrivers, string releaseNotes) = GetGpuMetadata(gpu, driverType, useExperimental, false);

            // Get the latest driver (recommended)
            NvidiaDriver latestDriver = nvidiaDrivers.Find(x => x.recommended);
            latestDriver.title = $"[Latest] {latestDriver.title}";

            OfflineGPUVersion = gpu.version;
            OnlineGPUVersion = latestDriver.version;

            Write("OK!");
            WriteLine();

            if (debug) {
                WriteLine($"downloadURL: {latestDriver.downloadUrl}");
                if (latestDriver.releaseDate != DateTime.MinValue) WriteLine($"releaseDate: {latestDriver.releaseDate.ToShortDateString()}");
                if (latestDriver.fileSizeEst != "unknown") WriteLine($"downloadFileSize:  {latestDriver.fileSizeEst}");
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
                    DownloadDriverQuiet(latestDriver, true);
                } else {
                    PromptAvailableUpdate(nvidiaDrivers, releaseNotes);
                }
            }

            callExit(0);
        }

        private static (List<NvidiaDriver> nvidiaDrivers, string releaseNotes) GetGpuMetadata(GPU gpu, string driverType, bool useExperimental, bool secondAttempt)
        {
            List<NvidiaDriver> nvidiaDrivers;
            string error, releaseNotes;

            if (useExperimental)
            {
                (nvidiaDrivers, error, releaseNotes) = NewMetadataHandler.GetDriverMetadata(gpu.deviceId, driverType);
            }
            else
            {
                OldMetadataHandler.PrepareCache();
                (nvidiaDrivers, error, releaseNotes) = OldMetadataHandler.GetDriverMetadata(gpu, driverType);
            }

            if (nvidiaDrivers != null)
            {
                return (nvidiaDrivers, releaseNotes);
            }
            else if (!secondAttempt)
            {
                // Try other metadata repo if failing
                string nowLoading = (useExperimental == false ? "New" : "Old");
                Write($"Now loading {nowLoading}MetadataHandler . . . ");
                return GetGpuMetadata(gpu, driverType, !useExperimental, true); // set secondAttempt to allow one more run
            }
            else
            {
                WriteLine("GPU metadata lookup failed both methods. TNUC can not continue.");
                WriteLine($"Error reason: {error}");
                WriteLine();
                callExit(1);
                return (null, null);
            }
        }

        // Local driver install flow
        private static void localDriverInstall()
        {
            if (!PowerHandler.ConfirmHeavyOperation("installing a driver"))
            {
                callExit(1);
            }

            bool fileExists = localDriverPath != null && File.Exists(localDriverPath);
            string selectedFilePath = null;

            if (fileExists)
            {
                selectedFilePath = localDriverPath;
                WriteLine($"Using local driver: {localDriverPath}");
            }
            else
            {
                // Open file dialog to select driver
                using var dialog = new OpenFileDialog
                {
                    Title = "Select NVIDIA driver",
                    Filter = "NVIDIA driver (*.exe)|*.exe",
                    CheckFileExists = true,
                    Multiselect = false
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = dialog.FileName;
                    WriteLine($"Selected file: {selectedFilePath}");
                }
                else
                {
                    WriteLine("No file selected. Exiting.");
                    callExit(1);
                }
            }

            string driverFileName = Path.GetFileName(selectedFilePath);
            string FULL_PATH_DIRECTORY = Path.GetDirectoryName(Path.GetFullPath(selectedFilePath)) + Path.DirectorySeparatorChar;
            string FULL_PATH_DRIVER = FULL_PATH_DIRECTORY + driverFileName;

            bool minimalInstaller = ConfigurationHandler.ReadSettingBool("Minimal install");
            List<string> tempFiles = [driverFileName];

            if (minimalInstaller) {
                bool minimized = false; // forced off for now

                // Perform minimal install
                string[] minimalInstallTempFiles = MakeInstaller(minimized, FULL_PATH_DIRECTORY, driverFileName);

                // Add minimal temp files
                minimalInstaller = minimalInstallTempFiles != null;
                if (minimalInstaller) tempFiles.AddRange(minimalInstallTempFiles);
            }

            // Show installer
            string fileName = minimalInstaller ? FULL_PATH_DIRECTORY + "setup.exe" : FULL_PATH_DRIVER;

            ReadyInstallForm.handleInstall(fileName, false, tempFiles);

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

                // enable useLocalDriver + optional localDriverPath
                else if (arg.ToLower().Contains("--driver-path")) {
                    useLocalDriver = true;
                    var pathArg = arg.Substring(arg.IndexOf('=') + 1);
                    localDriverPath = pathArg;
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
                    Environment.Exit(0);
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
                    WriteLine("--driver-path=<optional>     Install (and perform minimal install, if enabled) a local driver on the system.");
                    WriteLine("--force-dl                   Force prompt to download drivers, even if the user is up-to-date - should only be used for debugging.");
                    WriteLine("--version                    View version.");
                    WriteLine("--confirm-dl                 Automatically download and install the driver quietly without any user interaction at all. should be used with '--quiet' for the optimal solution.");
                    WriteLine("--config-here                Use the working directory as path to the configuration file.");
                    WriteLine("--config-override=<path>     Override configuration file location with absolute file path.");
                    WriteLine("--override-desktop           Override automatic desktop/notebook identification.");
                    WriteLine("--override-notebook          Override automatic desktop/notebook identification.");
                    WriteLine("--help                       Show all commands.");
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

        public static string SendGetRequest(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = SendMetadataRequest(request);
            response.EnsureSuccessStatusCode();

            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static HttpResponseMessage SendMetadataRequest(HttpRequestMessage request)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            return httpClient.Send(request, HttpCompletionOption.ResponseContentRead, timeout.Token);
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
        private static void PromptAvailableUpdate(List<NvidiaDriver> nvidiaDrivers, string releaseNotes)
        {
            (DriverAvailableDialog.SelectedBtn selectedBtn, NvidiaDriver selectedVersion) = DriverAvailableDialog.ShowGUI(nvidiaDrivers, releaseNotes);

            if (selectedBtn == DriverAvailableDialog.SelectedBtn.DLEXTRACT) {
                // download and save (and extract)

                if (!PowerHandler.ConfirmHeavyOperation("downloading a driver"))
                {
                    callExit(1);
                }

                string driverFileName = selectedVersion.downloadUrl.Split('/').Last(); // retrives file name from url
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
                        PromptAvailableUpdate(nvidiaDrivers, releaseNotes);
                        return;
                    }

                    string finalPath = savePath + driverFileName;

                    // Get file size from NVIDIA server
                    (long fileSize,_) = GetDriverMetadataFromNvidia(selectedVersion.downloadUrl);

                    if (File.Exists(finalPath) && !DoesDriverFileSizeMatch(finalPath, fileSize)) {
                        File.Delete(finalPath);
                    }

                    // don't download driver if it already exists
                    WriteLine();
                    Write("Downloading the driver . . . ");
                    if (showUI && !File.Exists(finalPath)) {
                        HandleDownload(selectedVersion.downloadUrl, finalPath).GetAwaiter().GetResult();
                    }

                    // show progress bar gui if quiet
                    else if (!showUI && !File.Exists(finalPath)) {
                        using var dlForm = new DownloaderForm(selectedVersion.downloadUrl, finalPath);
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
                    if (MakeInstaller(false, savePath, driverFileName) == null)
                        ReadyInstallForm.handleInstall(Path.Combine(savePath, driverFileName), false, [driverFileName], true);
                }
            } else if (selectedBtn == DriverAvailableDialog.SelectedBtn.DLINSTALL) {
                DownloadDriverQuiet(selectedVersion, confirmDL);
                
            } else if (selectedBtn == DriverAvailableDialog.SelectedBtn.DLINSTALLCUSTOM) {
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
                    DownloadDriverQuiet(selectedVersion, confirmDL, dialog.SelectedPath + @"\", true);
                }
                else
                {
                    PromptAvailableUpdate(nvidiaDrivers, releaseNotes);
                    return;
                }
            }
        }

        /// <summary>
        /// Downloads and installs the driver without user interaction
        /// </summary>
        private static void DownloadDriverQuiet(NvidiaDriver nvidiaDriver, bool minimized, string overrideDownloadLocation = null, bool keepDriver = false)
        {
            if (!PowerHandler.ConfirmHeavyOperation("installing a driver"))
            {
                callExit(1);
            }

            string driverFileName = nvidiaDriver.downloadUrl.Split('/').Last(); // retrives file name from url
            string savePath = overrideDownloadLocation ?? Path.GetTempPath();

            string FULL_PATH_DIRECTORY = overrideDownloadLocation ?? Path.Combine(savePath, nvidiaDriver.version) + Path.DirectorySeparatorChar;
            string FULL_PATH_DRIVER = FULL_PATH_DIRECTORY + driverFileName;

            savePath = FULL_PATH_DIRECTORY;

            Directory.CreateDirectory(FULL_PATH_DIRECTORY);

            // Get file size from NVIDIA server
            (long fileSize,_) = GetDriverMetadataFromNvidia(nvidiaDriver.downloadUrl);

            if (File.Exists(FULL_PATH_DRIVER) && !DoesDriverFileSizeMatch(FULL_PATH_DRIVER, fileSize)) {
                File.Delete(savePath + driverFileName);
            }

            if (!File.Exists(FULL_PATH_DRIVER)) {
                Write("Downloading the driver . . . ");

                if (showUI || confirmDL) {
                    try {
                        HandleDownload(nvidiaDriver.downloadUrl, FULL_PATH_DRIVER).GetAwaiter().GetResult();

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
                    using var dlForm = new DownloaderForm(nvidiaDriver.downloadUrl, FULL_PATH_DRIVER);
                    dlForm.ShowDialog();
                    if (dlForm.Error != null) throw dlForm.Error;
                }
            }

            bool minimalInstaller = ConfigurationHandler.ReadSettingBool("Minimal install");
            List<string> tempFiles = [driverFileName];

            if (minimalInstaller) {
                // Perform minimal install
                string[] minimalInstallTempFiles = MakeInstaller(minimized, FULL_PATH_DIRECTORY, driverFileName);

                // Add minimal temp files
                minimalInstaller = minimalInstallTempFiles != null;
                if (minimalInstaller) tempFiles.AddRange(minimalInstallTempFiles);
            }

            string fileName = minimalInstaller ? FULL_PATH_DIRECTORY + "setup.exe" : FULL_PATH_DRIVER;

            // Handle driver install
            ReadyInstallForm.handleInstall(fileName, minimized, tempFiles, keepDriver);
        }

        /// <summary>
        /// Shared method for the accual downloading of a file with the command line progress bar.
        /// </summary>
        /// <param name="url">URL path for download</param>
        /// <param name="path">Absolute file path</param>
        /// <returns></returns>
        async public static Task HandleDownload(string url, string path, EventHandler<float> progressHandle = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                using (FileStream file = new(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1, FileOptions.Asynchronous))  {
                    await httpClient.DownloadDataAsync(url, file, progress, cancellationToken).ConfigureAwait(false);
                }

                cancellationToken.ThrowIfCancellationRequested();
                File.Move(path, path[..^5], true); // rename back
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
        private static string[] MakeInstaller(bool silent, string savePath, string fileName)
        {
            try
            {
                return MakeInstallerCore(silent, savePath, fileName);
            }
            catch (Exception ex)
            {
                string message = $"Driver extraction failed: {ex.Message}";
                Console.Error.WriteLine(message);
                // Download confirmation is not consent to install additional components.
                if (!silent && !confirmDL && MessageBox.Show(
                    message + "\n\nUse the original full installer instead? This will not use your minimal component selection.",
                    "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    return null;

                callExit(1);
                return null;
            }
        }

        private static string[] MakeInstallerCore(bool silent, string savePath, string fileName)
        {
            WriteLine();
            Write("Extracting drivers . . . ");

            savePath = Path.GetFullPath(savePath);
            string extractedPath = Path.Combine(savePath, "temp");
            string fullInstallerPath = Path.Combine(savePath, fileName);
            if (!File.Exists(fullInstallerPath) || new FileInfo(fullInstallerPath).Length < 10L * 1024 * 1024)
                throw new FileNotFoundException($"Driver installer file is missing or incomplete: {fullInstallerPath}");

            // Never merge a new extraction with files from a previous attempt.
            if (Directory.Exists(extractedPath) && Directory.GetFileSystemEntries(extractedPath).Length != 0)
                throw new IOException($"Extraction folder is not empty: {extractedPath}. Select an empty folder or remove the old extraction first.");
            Directory.CreateDirectory(extractedPath);

            try
            {
                LibraryFile libraryFile = LibraryHandler.EvaluateLibrary()
                    ?? throw new InvalidOperationException("No supported archiver was found. Install 7-Zip, WinRAR, or NanaZip.");
                using var process = new Process();
                LibraryHandler.Library library = libraryFile.LibraryName();

                // Extract full driver to then analyze
                if (library == LibraryHandler.Library.WINRAR) {
                    process.StartInfo = new ProcessStartInfo {
                        FileName = libraryFile.GetInstallationDirectory() + "winrar.exe",
                        WorkingDirectory = savePath,
                        Arguments = $"x -y \"{fullInstallerPath}\" \"{extractedPath}{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}\"",
                        UseShellExecute = false
                    };

                    if (silent) process.StartInfo.Arguments += " -ibck";
                } else if (library == LibraryHandler.Library.SEVENZIP) {
                    process.StartInfo = new ProcessStartInfo {
                        WorkingDirectory = savePath,
                        Arguments = $"x \"{fullInstallerPath}\" -o\"{extractedPath}\" -y",
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
                        Arguments = $"x \"{fullInstallerPath}\" -o\"{extractedPath}\" -y",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    if (silent) {
                        process.StartInfo.FileName = "NanaZipC.exe";
                    } else {
                        process.StartInfo.FileName = "NanaZipG.exe";
                    }
                }

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                // Drain both pipes while the archiver runs, not after WaitForExit.
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(TimeSpan.FromMinutes(5)))
                {
                    process.Kill(true);
                    process.WaitForExit(TimeSpan.FromSeconds(10));
                    throw new TimeoutException("Driver extraction exceeded the five-minute timeout.");
                }
                if (!Task.WhenAll(outputTask, errorTask).Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException("Archiver output streams did not close after extraction.");
                string output = outputTask.GetAwaiter().GetResult();
                string error = errorTask.GetAwaiter().GetResult();
                if (process.ExitCode != 0 || !Directory.Exists(extractedPath)
                    || Directory.GetFileSystemEntries(extractedPath).Length == 0)
                    throw new IOException($"Archiver: {process.StartInfo.FileName}\nExit code: {process.ExitCode}\nExtraction folder: {extractedPath}\n{error}\n{output}");

                // Analyze with ComponentHandler
                List<Component> driverComponents = ComponentHandler.ParseComponentData(extractedPath);
                if (driverComponents.Count == 0 || !File.Exists(Path.Combine(extractedPath, "setup.exe"))
                    || !File.Exists(Path.Combine(extractedPath, "setup.cfg")))
                    throw new InvalidDataException($"No usable NVIDIA installer was extracted to {extractedPath}.\n{error}\n{output}");

                // If config entry exists, show "Use last used components" button
                string configComponentsString = ConfigurationHandler.ReadSetting("Minimal install components", null, false);

                using ComponentChooserForm componentForm = new();

                // Open component form
                // If quiet mode + configComponents exists, it will not show dialog, and will use configComponents
                (List<string> chosenComponents, bool saveConfig) =
                    componentForm.OpenForm(driverComponents, configComponentsString);

                // Save latest used components to config file if user selected "Save selection"
                if (saveConfig) {
                    ConfigurationHandler.SetSetting("Minimal install components", string.Join(", ", chosenComponents));
                }

                string[] extractFiles = [.. chosenComponents, "NVI2", "EULA.txt", "license.txt", "ListDevices.txt", "setup.cfg", "setup.exe"];

                foreach (string file in extractFiles)
                {
                    string destination = Path.Combine(savePath, file);
                    if (File.Exists(destination) || Directory.Exists(destination))
                        throw new IOException($"Extraction destination already exists: {destination}. Select an empty folder.");
                }

                foreach (string file in extractFiles) {
                    string filePath = Path.Combine(savePath, "temp", file);

                    if (File.Exists(filePath))
                    {
                        string destinationFilePath = Path.Combine(savePath, file);
                        File.Move(filePath, destinationFilePath);
                    }
                    else if (Directory.Exists(filePath))
                    {
                        string destinationDirectoryPath = Path.Combine(savePath, Path.GetFileName(filePath));
                        Directory.Move(filePath, destinationDirectoryPath);
                    }
                }

                if (Directory.Exists(extractedPath))
                    Directory.Delete(extractedPath, true);

                // Remove new EULA files from the installer config, or else the installer throws error codes
                // author https://github.com/cywq
                var xmlDocument = new XmlDocument();
                string setupFile = Path.Combine(savePath, "setup.cfg");
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
                return extractFiles;
            }
            finally
            {
                // The stale-folder guard runs before this scope: never remove pre-existing content.
                try
                {
                    if (Directory.Exists(extractedPath)) Directory.Delete(extractedPath, true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Could not clean extraction folder {extractedPath}: {ex.Message}");
                }
            }
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
            if (showUI && !noPrompt && !confirmDL && !Console.IsInputRedirected)
            {
                WriteLine();
                WriteLine("Press any key to exit...");
            }

            if (showUI && !noPrompt && !confirmDL && !Console.IsInputRedirected) Console.ReadKey(true);
            FreeConsole();
            Environment.Exit(exitNum);
        }
        
        private static bool DoesDriverFileSizeMatch(string absoluteFilePath, long fileSize) {
            return new FileInfo(absoluteFilePath).Length == fileSize;
        }

        public static (long fileSize, DateTime releaseDate) GetDriverMetadataFromNvidia(string downloadUrl)
        {
            // Query release date and file size
            using (var request = new HttpRequestMessage(HttpMethod.Head, downloadUrl))
            {
                using var response = SendMetadataRequest(request);
                response.EnsureSuccessStatusCode();

                // File size
                long fileSize = response.Content.Headers.ContentLength ?? -1;

                // Release date
                DateTimeOffset? releaseDateOffset = response.Content.Headers.LastModified;
                DateTime releaseDate = releaseDateOffset?.LocalDateTime ?? DateTime.MinValue;

                return (fileSize, releaseDate);
            }
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

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class DownloaderForm : Form
    {
        string downloadURL;
        string savePath;
        CancellationTokenSource downloadCancellation;
        bool downloadFinished;

        public Exception Error { get; private set; }

        public DownloaderForm(string downloadURL, string savePath)
        {
            InitializeComponent();
            this.downloadURL = downloadURL;
            this.savePath = savePath;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            downloadCancellation = new CancellationTokenSource();

            var progress = new Progress<float>(value =>
            {
                if (!IsDisposed && !downloadFinished) progressBar1.Value = Math.Clamp((int)value, 0, 100);
            });

            try
            {
                await Task.Run(() => MainConsole.HandleDownload(
                    downloadURL,
                    savePath,
                    (s, value) => ((IProgress<float>)progress).Report(value), downloadCancellation.Token));
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                downloadFinished = true;
                downloadCancellation.Dispose();
                downloadCancellation = null;
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (downloadCancellation != null && !downloadFinished)
            {
                e.Cancel = true;
                downloadCancellation.Cancel();
            }
        }
    }
}
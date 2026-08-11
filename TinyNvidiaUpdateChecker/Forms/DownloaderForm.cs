using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    public partial class DownloaderForm : Form
    {
        string downloadURL;
        string savePath;

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

            var progress = new Progress<float>(value =>
                progressBar1.Value = Math.Min((int)value, 100));

            try
            {
                await Task.Run(() => MainConsole.HandleDownload(
                    downloadURL,
                    savePath,
                    (s, value) => ((IProgress<float>)progress).Report(value)));
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                Close();
            }
        }
    }
}
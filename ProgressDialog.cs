using System.Windows.Forms;

namespace UniFlash
{
    public class ProgressDialog : Form
    {
        private Label statusLabel;
        private ProgressBar progressBar;

        public ProgressDialog(string message = "Please wait...")
        {
            this.Text = "Processing";
            this.Width = 350;
            this.Height = 120;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ControlBox = false;

            statusLabel = new Label { Text = message, Left = 20, Top = 15, Width = 300 };
            progressBar = new ProgressBar { Style = ProgressBarStyle.Marquee, Left = 20, Top = 45, Width = 300, Height = 20, MarqueeAnimationSpeed = 30 };

            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
        }

        public void SetStatus(string message)
        {
            statusLabel.Text = message;
            statusLabel.Refresh();
        }
    }
} 
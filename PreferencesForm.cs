using System;
using System.Windows.Forms;

namespace UniFlash
{
    public partial class PreferencesForm : Form
    {
        private TextBox urlTextBox;
        private Button saveButton;
        private Button cancelButton;
        public event Action<string[]> OnSave;

        public PreferencesForm(string[] initialUrls)
        {
            InitializeComponent();
            urlTextBox.Lines = initialUrls;
        }

        private void InitializeComponent()
        {
            this.Text = "Preferences";
            this.Width = 500;
            this.Height = 350;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label label = new Label { Text = "Boards Manager URLs (one per line):", Left = 10, Top = 20, Width = 400 };
            urlTextBox = new TextBox { Multiline = true, Left = 10, Top = 50, Width = 460, Height = 180, ScrollBars = ScrollBars.Vertical };
            saveButton = new Button { Text = "Save", Left = 290, Top = 250, Width = 80 };
            cancelButton = new Button { Text = "Cancel", Left = 390, Top = 250, Width = 80 };

            saveButton.Click += (s, e) => { OnSave?.Invoke(urlTextBox.Lines); this.DialogResult = DialogResult.OK; this.Close(); };
            cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(label);
            this.Controls.Add(urlTextBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }
    }
} 
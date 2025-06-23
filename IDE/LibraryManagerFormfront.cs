using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;

namespace UniFlash.IDE
{
    public partial class LibraryManagerForm : Form
    {
        private ArduinoLibraryManager libraryManager;
        private List<LibraryInfo> allLibraries = new();
        private TextBox searchBox;
        private DataGridView resultsGrid;
        private TextBox manualEntryBox;
        private Button manualInstallButton;
        private RichTextBox logBox;
        private Button searchButton;
        private LibraryIndexManager indexManager;
        private bool cliAvailable = false;

        public LibraryManagerForm(ArduinoLibraryManager manager)
        {
            libraryManager = manager;
            indexManager = new LibraryIndexManager();
            InitializeUI();
            LoadLibraryIndex();
            CheckCliAvailabilityAsync();
        }

        private async void CheckCliAvailabilityAsync()
        {
            try
            {
                logBox.AppendText("[Checking arduino-cli availability...]\n");
                var result = await libraryManager.RunCliCommand("arduino-cli version");
                cliAvailable = !string.IsNullOrEmpty(result) && !result.Contains("Error") && !result.Contains("not recognized");
                
                if (cliAvailable)
                {
                    logBox.AppendText("[arduino-cli is available]\n");
                    // Enable install buttons
                    manualInstallButton.Enabled = true;
                    UpdateGridInstallButtons(true);
                }
                else
                {
                    logBox.AppendText("[arduino-cli is not available. Please ensure it is installed and in your PATH]\n");
                    // Disable install buttons
                    manualInstallButton.Enabled = false;
                    UpdateGridInstallButtons(false);
                }
            }
            catch (Exception ex)
            {
                logBox.AppendText($"[Error checking arduino-cli: {ex.Message}]\n");
                cliAvailable = false;
                manualInstallButton.Enabled = false;
                UpdateGridInstallButtons(false);
            }
        }

        private void UpdateGridInstallButtons(bool enabled)
        {
            foreach (DataGridViewRow row in resultsGrid.Rows)
            {
                var cell = row.Cells["Install"] as DataGridViewButtonCell;
                if (cell != null)
                {
                    cell.Value = enabled ? "Install" : "(Requires CLI)";
                    cell.Style.ForeColor = enabled ? Color.Black : Color.Gray;
                    cell.Style.SelectionForeColor = enabled ? Color.Black : Color.Gray;
                }
            }
        }

        private void InitializeUI()
        {
            this.Text = "Library Manager";
            this.Width = 800;
            this.Height = 600;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            searchBox = new TextBox { Width = 400, Left = 10, Top = 10, PlaceholderText = "Search libraries..." };
            searchButton = new Button { Text = "Search", Left = 420, Top = 8, Width = 80 };
            searchButton.Click += async (s, e) => await SearchLibraries();
            searchBox.KeyPress += async (s, e) => {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    await SearchLibraries();
                }
            };

            resultsGrid = new DataGridView
            {
                Left = 10,
                Top = 40,
                Width = 760,
                Height = 350,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false
            };
            resultsGrid.Columns.Add("Name", "Name");
            resultsGrid.Columns.Add("Author", "Author");
            resultsGrid.Columns.Add("Description", "Description");
            var installCol = new DataGridViewButtonColumn { Text = "Install", UseColumnTextForButtonValue = true, Name = "Install" };
            resultsGrid.Columns.Add(installCol);
            resultsGrid.CellClick += ResultsGrid_CellClick;

            manualEntryBox = new TextBox { Width = 400, Left = 10, Top = 400, PlaceholderText = "Manual library name..." };
            manualInstallButton = new Button { Text = "Install", Left = 420, Top = 398, Width = 80, Enabled = false };
            manualInstallButton.Click += async (s, e) => await InstallManualLibrary();

            logBox = new RichTextBox { Left = 10, Top = 440, Width = 760, Height = 110, ReadOnly = true };

            this.Controls.Add(searchBox);
            this.Controls.Add(searchButton);
            this.Controls.Add(resultsGrid);
            this.Controls.Add(manualEntryBox);
            this.Controls.Add(manualInstallButton);
            this.Controls.Add(logBox);
        }

        private async void LoadLibraryIndex()
        {
            logBox.AppendText("[Loading Arduino library index...]\n");
            bool loaded = await indexManager.LoadAsync();
            if (!loaded)
            {
                logBox.AppendText("[Failed to load library index. No internet and no local copy found.]\n");
                searchBox.Enabled = false;
                searchButton.Enabled = false;
                resultsGrid.Enabled = false;
                manualEntryBox.Enabled = false;
                manualInstallButton.Enabled = false;
                return;
            }
            logBox.AppendText("[Library index loaded. Type to search.]\n");
            await SearchLibraries();
        }

        private async Task SearchLibraries()
        {
            string query = searchBox.Text.Trim();
            var libs = indexManager.Search(query);
            DisplayLibraries(libs);
        }

        private void DisplayLibraries(List<LibraryIndexManager.ArduinoLibrary> libs)
        {
            resultsGrid.Rows.Clear();
            foreach (var lib in libs)
            {
                int rowIdx = resultsGrid.Rows.Add(lib.Name, lib.Author, lib.Sentence);
                var cell = resultsGrid.Rows[rowIdx].Cells["Install"] as DataGridViewButtonCell;
                if (cell != null)
                {
                    cell.Value = cliAvailable ? "Install" : "(Requires CLI)";
                    cell.Style.ForeColor = cliAvailable ? Color.Black : Color.Gray;
                    cell.Style.SelectionForeColor = cliAvailable ? Color.Black : Color.Gray;
                }
            }
        }

        private async void ResultsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == resultsGrid.Columns["Install"].Index)
            {
                if (!cliAvailable)
                {
                    MessageBox.Show("Library install requires Arduino CLI to be installed and available in PATH.", "CLI Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var libraryName = resultsGrid.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                if (!string.IsNullOrEmpty(libraryName))
                {
                    logBox.AppendText($"Installing library: {libraryName}\n");
                    bool success = await libraryManager.InstallLibrary(libraryName);
                    if (success)
                    {
                        logBox.AppendText($"Successfully installed {libraryName}\n");
                        MessageBox.Show($"Library {libraryName} installed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        logBox.AppendText($"Failed to install {libraryName}\n");
                        MessageBox.Show($"Failed to install library {libraryName}. Check the log for details.", "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async Task InstallManualLibrary()
        {
            if (!cliAvailable)
            {
                MessageBox.Show("Library install requires Arduino CLI to be installed and available in PATH.", "CLI Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string libraryName = manualEntryBox.Text.Trim();
            if (string.IsNullOrEmpty(libraryName))
            {
                MessageBox.Show("Please enter a library name.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            logBox.AppendText($"Installing library: {libraryName}\n");
            bool success = await libraryManager.InstallLibrary(libraryName);
            if (success)
            {
                logBox.AppendText($"Successfully installed {libraryName}\n");
                MessageBox.Show($"Library {libraryName} installed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                manualEntryBox.Clear();
            }
            else
            {
                logBox.AppendText($"Failed to install {libraryName}\n");
                MessageBox.Show($"Failed to install library {libraryName}. Check the log for details.", "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 
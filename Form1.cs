using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using ScottPlot.WinForms;
using System.Linq;
using UniFlash.Graphs;
using UniFlash.ViewModels;
using System.Threading.Tasks;

namespace UniFlash;

public partial class Form1 : Form
{
    public ComboBox comPortComboBox;
    public System.Windows.Forms.Label comPortLabel;
    public System.Windows.Forms.Label connectLabel;
    public Button connectButton;
    public System.Windows.Forms.Label separatorLabel;
    public System.Windows.Forms.Label baudLabel;
    public ComboBox baudComboBox;
    public SerialPort serialPort;
    public TextBox dataTextBox;
    public FormsPlot formsPlot1;
    public FormsPlot formsPlot2;
    public ComboBox paramComboBox1;
    public ComboBox paramComboBox2;
    public CheckBox splitScreenCheckBox;
    public System.Windows.Forms.Label paramLabel1;
    public System.Windows.Forms.Label paramLabel2;
    public Panel chartPanel;
    public Button maximizeButton1;
    public Button maximizeButton2;
    private List<Form>? maximizedForms;
    private GraphManager graphManager;
    private List<string> pausedLines = new List<string>();
    private List<double> pausedVoltages = new List<double>();
    private List<double> pausedTdsValues = new List<double>();
    private List<double> pausedTempValues = new List<double>();
    private List<double> pausedTimelapses = new List<double>();
    private DateTime? pauseStartTime = null;
    private double lastTimeValue = 0;
    private bool isFirstPause = true;
    public System.Windows.Forms.Label pauseLoggingLabel;
    public Button pauseLoggingButton;
    public System.Windows.Forms.Label logSeparatorLabel;
    public System.Windows.Forms.Label stopLoggingLabel;
    public Button stopLoggingButton;
    public CheckBox autoScrollCheckBox;

    // BaudRate class for ComboBox
    public class BaudRate
    {
        public int Value { get; set; }
        public string Display { get; set; }
        public override string ToString() => Display;
    }

    // Data storage for plotting
    private List<double> voltages = new List<double>();
    private List<double> tdsValues = new List<double>();
    private List<double> tempValues = new List<double>();
    private List<double> timelapses = new List<double>();
    private DateTime? startTime = null;

    // Add maximize/restore toggle logic
    private Form maximizedGraphForm = null;

    // Add ViewModel instance:
    private UniFlash.ViewModels.MainViewModel viewModel = new UniFlash.ViewModels.MainViewModel();

    public Form1()
    {
        InitializeComponent();
        InitializeCustomComponents();
        LoadComPorts();
        // Auto-detect COM ports whenever the form gains focus
        this.Activated += (s, e) => LoadComPorts();
        // Instantiate GraphManager after controls are created
        graphManager = new GraphManager(formsPlot1, formsPlot2, paramComboBox1, paramComboBox2, splitScreenCheckBox);
        // Bind UI controls to ViewModel properties (example for ComboBox, Button, etc.)
        // (You may need to add more bindings as needed)
        comPortComboBox.SelectedIndexChanged += (s, e) => viewModel.SelectedComPort = comPortComboBox.SelectedItem?.ToString();
        baudComboBox.SelectedIndexChanged += (s, e) => viewModel.SelectedBaudRate = baudComboBox.SelectedItem is BaudRate br ? br.Value : 9600;
    }

    private void InitializeCustomComponents()
    {
        // Set form properties for dark background
        this.Text = "UNI Flash Device";
        this.WindowState = FormWindowState.Maximized;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(18, 18, 18);
        this.Font = new Font("Segoe UI", 12, FontStyle.Regular);

        // Create a TableLayoutPanel for main layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(18, 18, 18),
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(40, 30, 40, 30),
            AutoSize = true,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // Terminal row
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 400)); // Chart row
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        this.Controls.Add(mainLayout);

        // --- Top Toolbar Row ---
        var toolbarPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            BackColor = Color.FromArgb(24, 24, 24),
            Padding = new Padding(20, 15, 20, 15),
            Margin = new Padding(0, 0, 0, 25),
            Anchor = AnchorStyles.Top,
            WrapContents = false
        };
        toolbarPanel.Paint += (s, e) => {
            using (var pen = new Pen(Color.FromArgb(40, 40, 40), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, toolbarPanel.Width - 1, toolbarPanel.Height - 1);
            }
        };

        comPortLabel = new Label { Text = "COM Port:", ForeColor = Color.FromArgb(255, 192, 203), Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 8, 12, 0) };
        comPortComboBox = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, ForeColor = Color.FromArgb(255, 192, 203), BackColor = Color.FromArgb(30, 30, 30), Font = new Font("Segoe UI", 13), Margin = new Padding(0, 0, 25, 0) };
        baudLabel = new Label { Text = "Baud:", ForeColor = Color.FromArgb(255, 192, 203), Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 8, 12, 0) };
        baudComboBox = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, ForeColor = Color.FromArgb(255, 192, 203), BackColor = Color.FromArgb(30, 30, 30), Font = new Font("Segoe UI", 13), Margin = new Padding(0, 0, 25, 0) };
        separatorLabel = new Label { Text = "|", ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("Segoe UI", 13), AutoSize = true, Margin = new Padding(0, 8, 25, 0) };
        connectButton = new Button { Text = "Connect", Width = 120, Height = 38, ForeColor = Color.HotPink, BackColor = Color.FromArgb(40, 40, 40), Font = new Font("Segoe UI", 13), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 0, 0) };
        connectButton.FlatAppearance.BorderColor = Color.HotPink;
        connectButton.FlatAppearance.BorderSize = 1;
        pauseLoggingLabel = new Label { Text = "Pause Logging", ForeColor = Color.White, Font = new Font("Segoe UI", 12), AutoSize = true, Margin = new Padding(0, 8, 12, 0) };
        pauseLoggingButton = new Button { Text = "⏸", Width = 38, Height = 38, ForeColor = Color.HotPink, BackColor = Color.FromArgb(40, 40, 40), Font = new Font("Segoe UI", 16, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 16, 0) };
        pauseLoggingButton.FlatAppearance.BorderColor = Color.HotPink;
        pauseLoggingButton.FlatAppearance.BorderSize = 1;
        logSeparatorLabel = new Label { Text = "|", ForeColor = Color.White, Font = new Font("Segoe UI", 13), AutoSize = true, Margin = new Padding(0, 8, 16, 0) };
        stopLoggingLabel = new Label { Text = "Stop Logging", ForeColor = Color.White, Font = new Font("Segoe UI", 12), AutoSize = true, Margin = new Padding(0, 8, 12, 0) };
        stopLoggingButton = new Button { Text = "■", Width = 38, Height = 38, ForeColor = Color.HotPink, BackColor = Color.FromArgb(40, 40, 40), Font = new Font("Segoe UI", 16, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 16, 0) };
        stopLoggingButton.FlatAppearance.BorderColor = Color.HotPink;
        stopLoggingButton.FlatAppearance.BorderSize = 1;
        var downloadExcelButton = new Button { Text = "Download Excel", Width = 180, Height = 38, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(40, 40, 40), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 0, 0) };
        downloadExcelButton.FlatAppearance.BorderColor = Color.HotPink;
        downloadExcelButton.FlatAppearance.BorderSize = 1;
        downloadExcelButton.Click += (s, e) => graphManager.ExportToExcelWithDialog(this);

        var ideButton = new Button { Text = "IDE", Width = 100, Height = 38, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(40, 40, 40), FlatStyle = FlatStyle.Flat, Margin = new Padding(10, 0, 0, 0) };
        ideButton.FlatAppearance.BorderColor = Color.HotPink;
        ideButton.FlatAppearance.BorderSize = 1;
        ideButton.Click += async (s, e) => {
            try
            {
                // Disable the button temporarily to show it's working
                ideButton.Enabled = false;
                ideButton.Text = "Opening...";
                
                // Create and show the IDE window
                var ideWindow = new UniFlash.IDE.IDEWindow();
                ideWindow.Show();
                
                // Re-enable the button
                ideButton.Enabled = true;
                ideButton.Text = "IDE";
            }
            catch (Exception ex)
            {
                // Re-enable the button in case of error
                ideButton.Enabled = true;
                ideButton.Text = "IDE";
                
                MessageBox.Show($"Error opening IDE: {ex.Message}\n\nPlease check if arduino-cli is installed and accessible.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        toolbarPanel.Controls.AddRange(new Control[] {
            comPortLabel, comPortComboBox, baudLabel, baudComboBox, separatorLabel, connectButton,
            pauseLoggingLabel, pauseLoggingButton, logSeparatorLabel, stopLoggingLabel, stopLoggingButton, 
            downloadExcelButton, ideButton
        });
        mainLayout.Controls.Add(toolbarPanel, 0, 0);

        // --- Terminal Row ---
        var terminalPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 200,
            Margin = new Padding(0, 0, 0, 25),
            BackColor = Color.FromArgb(24, 24, 24),
            Padding = new Padding(20)
        };
        terminalPanel.Paint += (s, e) => {
            using (var pen = new Pen(Color.FromArgb(40, 40, 40), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, terminalPanel.Width - 1, terminalPanel.Height - 1);
            }
        };

        dataTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 12),
            ForeColor = Color.FromArgb(220, 220, 220),
            BackColor = Color.FromArgb(30, 30, 30),
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(15)
        };

        autoScrollCheckBox = new CheckBox
        {
            Text = "Auto-scroll",
            Checked = true,
            ForeColor = Color.FromArgb(180, 180, 180),
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(20, 20),
            BackColor = Color.Transparent
        };

        terminalPanel.Controls.Add(dataTextBox);
        terminalPanel.Controls.Add(autoScrollCheckBox);
        mainLayout.Controls.Add(terminalPanel);

        // --- Graph Controls Row ---
        var graphControlPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            BackColor = Color.FromArgb(24, 24, 24),
            Padding = new Padding(20, 15, 20, 15),
            Margin = new Padding(0, 0, 0, 25)
        };
        graphControlPanel.Paint += (s, e) => {
            using (var pen = new Pen(Color.FromArgb(40, 40, 40), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, graphControlPanel.Width - 1, graphControlPanel.Height - 1);
            }
        };

        paramLabel1 = new Label { Text = "Graph 1:", ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("Segoe UI", 12), AutoSize = true, Margin = new Padding(0, 8, 12, 0) };
        paramComboBox1 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(255, 192, 203), BackColor = Color.FromArgb(30, 30, 30), Width = 120, Margin = new Padding(0, 0, 25, 0) };
        paramComboBox1.Items.AddRange(new string[] { "Voltage", "TDS", "Temperature" });
        paramComboBox1.SelectedIndex = 0;
        splitScreenCheckBox = new CheckBox { Text = "Split Screen", Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(180, 180, 180), BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 8, 25, 0) };
        paramLabel2 = new Label { Text = "Graph 2:", ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("Segoe UI", 12), AutoSize = true, Margin = new Padding(0, 8, 12, 0), Visible = false };
        paramComboBox2 = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(255, 192, 203), BackColor = Color.FromArgb(30, 30, 30), Width = 120, Margin = new Padding(0, 0, 25, 0), Visible = false };
        paramComboBox2.Items.AddRange(new string[] { "TDS", "Temperature" });
        paramComboBox2.SelectedIndex = 0;
        graphControlPanel.Controls.AddRange(new Control[] { paramLabel1, paramComboBox1, splitScreenCheckBox, paramLabel2, paramComboBox2 });
        mainLayout.Controls.Add(graphControlPanel);

        // --- Chart Panel Row ---
        chartPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 400,
            BackColor = Color.FromArgb(24, 24, 24),
            Margin = new Padding(0, 0, 0, 25),
            Padding = new Padding(20)
        };
        chartPanel.Paint += (s, e) => {
            using (var pen = new Pen(Color.FromArgb(40, 40, 40), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, chartPanel.Width - 1, chartPanel.Height - 1);
            }
        };
        mainLayout.Controls.Add(chartPanel);

        // --- ScottPlot Controls ---
        formsPlot1 = new FormsPlot
        {
            Location = new Point(0, 10),
            Size = new Size(chartPanel.Width, 150),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        chartPanel.Controls.Add(formsPlot1);
        maximizeButton1 = new Button
        {
            Text = "🗖",
            Location = new Point(chartPanel.Width - 40, formsPlot1.Top + 5),
            Size = new Size(32, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        maximizeButton1.Click += (s, e) => ToggleMaximizePlot(1);
        chartPanel.Controls.Add(maximizeButton1);
        formsPlot2 = new FormsPlot
        {
            Location = new Point(0, 180),
            Size = new Size(chartPanel.Width, 150),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        chartPanel.Controls.Add(formsPlot2);
        maximizeButton2 = new Button
        {
            Text = "🗖",
            Location = new Point(chartPanel.Width - 40, formsPlot2.Top + 5),
            Size = new Size(32, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Visible = false
        };
        maximizeButton2.Click += (s, e) => ToggleMaximizePlot(2);
        chartPanel.Controls.Add(maximizeButton2);

        // SerialPort setup
        serialPort = new SerialPort();
        serialPort.DataReceived += SerialPort_DataReceived;

        // Populate baudComboBox with BaudRate objects
        var baudRates = new BaudRate[] {
            new BaudRate { Value = 300, Display = "300 baud" },
            new BaudRate { Value = 600, Display = "600 baud" },
            new BaudRate { Value = 750, Display = "750 baud" },
            new BaudRate { Value = 1200, Display = "1200 baud" },
            new BaudRate { Value = 2400, Display = "2400 baud" },
            new BaudRate { Value = 4800, Display = "4800 baud" },
            new BaudRate { Value = 9600, Display = "9600 baud" },
            new BaudRate { Value = 19200, Display = "19200 baud" },
            new BaudRate { Value = 31250, Display = "31250 baud" },
            new BaudRate { Value = 38400, Display = "38400 baud" },
            new BaudRate { Value = 57600, Display = "57600 baud" },
            new BaudRate { Value = 74880, Display = "74880 baud" },
            new BaudRate { Value = 115200, Display = "115200 baud" },
            new BaudRate { Value = 230400, Display = "230400 baud" },
            new BaudRate { Value = 250000, Display = "250000 baud" },
            new BaudRate { Value = 460800, Display = "460800 baud" },
            new BaudRate { Value = 500000, Display = "500000 baud" }
        };
        baudComboBox.Items.Clear();
        baudComboBox.Items.AddRange(baudRates);
        baudComboBox.SelectedIndex = 6; // Default to 9600 baud

        // Connect button event
        connectButton.Click += (s, e) => ConnectOrDisconnect();
        // Baud rate change event
        baudComboBox.SelectedIndexChanged += (s, e) =>
        {
            if (viewModel.IsConnected)
            {
                ConnectOrDisconnect(); // Disconnect
                ConnectOrDisconnect(); // Reconnect with new baud
            }
        };

        // Split screen logic (update for ScottPlot)
        splitScreenCheckBox.CheckedChanged += (s, e) =>
        {
            formsPlot2.Visible = splitScreenCheckBox.Checked;
            maximizeButton2.Visible = splitScreenCheckBox.Checked;
            paramLabel2.Visible = splitScreenCheckBox.Checked;
            paramComboBox2.Visible = splitScreenCheckBox.Checked;
        };

        // Clear data when (re)connecting
        connectButton.Click += (s, e) =>
        {
            if (!viewModel.IsConnected) // about to connect
            {
                graphManager.ClearDataAndPlots();
            }
        };
        paramComboBox1.SelectedIndexChanged += (s, e) => graphManager.UpdateScottPlot();
        paramComboBox2.SelectedIndexChanged += (s, e) => graphManager.UpdateScottPlot();

        // Pause/Continue Logging logic
        pauseLoggingButton.Click += (s, e) =>
        {
            if (!viewModel.IsPaused)
            {
                viewModel.IsPaused = true;
                pauseLoggingButton.Text = "▶"; // Play icon
                pauseLoggingLabel.Text = "Continue  Logging";
                pauseStartTime = DateTime.Now;
                
                // Store the last time value before pause
                if (timelapses.Count > 0)
                {
                    lastTimeValue = timelapses[timelapses.Count - 1];
                }
                isFirstPause = true;
            }
            else
            {
                viewModel.IsPaused = false;
                pauseLoggingButton.Text = "⏸"; // Pause icon
                pauseLoggingLabel.Text = "Pause  Logging";
                
                // On resume, append all buffered lines to terminal
                if (pausedLines.Count > 0)
                {
                    foreach (var line in pausedLines)
                        AppendData(line + "\r\n");
                    pausedLines.Clear();
                }

                // Restore paused graph data
                if (pausedVoltages.Count > 0)
                {
                    voltages.AddRange(pausedVoltages);
                    tdsValues.AddRange(pausedTdsValues);
                    tempValues.AddRange(pausedTempValues);
                    timelapses.AddRange(pausedTimelapses);
                    
                    pausedVoltages.Clear();
                    pausedTdsValues.Clear();
                    pausedTempValues.Clear();
                    pausedTimelapses.Clear();
                }

                pauseStartTime = null;
                
                // Update the graph with all collected data
                this.BeginInvoke(new Action(graphManager.UpdateScottPlot));
            }
        };

        // Stop Logging logic
        stopLoggingButton.Click += (s, e) =>
        {
            viewModel.IsStopped = true;
            try
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
            }
            catch { }
            pauseLoggingButton.Enabled = false;
            stopLoggingButton.Enabled = false;
        };

        // Add buttons to open each graph in a new window
        var openGraph1Button = new Button
        {
            Text = "Open Graph 1 in New Window",
            Width = 220,
            Height = 38,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(40, 40, 40),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(10, 10, 10, 10)
        };
        openGraph1Button.FlatAppearance.BorderColor = Color.HotPink;
        openGraph1Button.FlatAppearance.BorderSize = 1;
        openGraph1Button.Click += (s, e) => ToggleMaximizePlot(1);

        var openGraph2Button = new Button
        {
            Text = "Open Graph 2 in New Window",
            Width = 220,
            Height = 38,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(40, 40, 40),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(10, 10, 10, 10)
        };
        openGraph2Button.FlatAppearance.BorderColor = Color.HotPink;
        openGraph2Button.FlatAppearance.BorderSize = 1;
        openGraph2Button.Click += (s, e) => ToggleMaximizePlot(2);

        // Add these buttons below the chartPanel
        mainLayout.Controls.Add(openGraph1Button);
        mainLayout.Controls.Add(openGraph2Button);
    }

    private void LoadComPorts()
    {
        comPortComboBox.Items.Clear();
        string[] ports = SerialPort.GetPortNames();
        comPortComboBox.Items.AddRange(ports);
        if (comPortComboBox.Items.Count > 0)
        {
            comPortComboBox.SelectedIndex = 0;
        }
        else
        {
            MessageBox.Show("No COM ports detected. Please connect your device.", "No Device Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ConnectOrDisconnect()
    {
        if (!viewModel.IsConnected)
        {
            try
            {
                serialPort.PortName = comPortComboBox.SelectedItem?.ToString();
                serialPort.BaudRate = baudComboBox.SelectedItem is BaudRate br ? br.Value : 9600;
                serialPort.Open();
                viewModel.IsConnected = true;
                connectButton.Text = "Disconnect";
                AppendData($"[Connected: {serialPort.PortName} @ {serialPort.BaudRate} baud]\r\n");
                viewModel.IsStopped = false; // Reset stop flag on connect
                pauseLoggingButton.Enabled = true;
                stopLoggingButton.Enabled = true;
            }
            catch (Exception ex)
            {
                AppendData($"[Error: {ex.Message}]\r\n");
            }
        }
        else
        {
            try
            {
                serialPort.Close();
                viewModel.IsConnected = false;
                connectButton.Text = "Connect";
                AppendData("[Disconnected]\r\n");
            }
            catch (Exception ex)
            {
                AppendData($"[Error: {ex.Message}]\r\n");
            }
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (viewModel.IsStopped) return;
        try
        {
            string data = serialPort.ReadLine();
            ParseAndPlotSensorData(data); // Always collect data
            if (viewModel.IsPaused)
            {
                pausedLines.Add(data);
            }
            else
            {
                AppendData(data + "\r\n");
            }
        }
        catch (Exception ex)
        {
            if (!viewModel.IsPaused)
                AppendData($"[Read Error: {ex.Message}]\r\n");
        }
    }

    private void ParseAndPlotSensorData(string data)
    {
        // Use GraphManager for parsing and plotting
        if (viewModel.IsPaused)
        {
            // Store the data in paused collections
            string voltage = null, tds = null, temp = null;
            try
            {
                var parts = data.Split(' ');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Contains("$Voltage$"))
                    {
                        if (i + 2 < parts.Length)
                            voltage = parts[i + 2].Replace("V", "").Trim();
                    }
                    else if (parts[i].Contains("$TDS$"))
                    {
                        if (i + 2 < parts.Length)
                            tds = parts[i + 2].Trim();
                    }
                    else if (parts[i].Contains("$Temp$"))
                    {
                        if (i + 2 < parts.Length)
                            temp = parts[i + 2].Trim();
                    }
                }
            }
            catch { }

            double v = double.NaN, tdsVal = double.NaN, tempVal = double.NaN;
            double.TryParse(voltage, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v);
            double.TryParse(tds, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tdsVal);
            double.TryParse(temp, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempVal);

            if (pauseStartTime.HasValue)
            {
                double timeSincePause = (DateTime.Now - pauseStartTime.Value).TotalSeconds;
                double currentTime;
                
                if (isFirstPause)
                {
                    currentTime = lastTimeValue;
                    isFirstPause = false;
                }
                else
                {
                    currentTime = lastTimeValue + timeSincePause;
                }

                pausedTimelapses.Add(currentTime);
                pausedVoltages.Add(v);
                pausedTdsValues.Add(tdsVal);
                pausedTempValues.Add(tempVal);
            }
        }
        else
        {
            graphManager.ParseAndPlotSensorData(data, () => {
                this.BeginInvoke(new Action(() => {
                    graphManager.UpdateScottPlot();
                    // Update maximized plot if it exists
                    if (maximizedGraphForm != null && !maximizedGraphForm.IsDisposed)
                    {
                        var maxPlot = maximizedGraphForm.Controls.OfType<ScottPlot.WinForms.FormsPlot>().FirstOrDefault();
                        if (maxPlot != null)
                        {
                            graphManager.UpdateMaximizedPlot(maxPlot, maximizedGraphForm.Tag is int plotNum ? plotNum : 1);
                        }
                    }
                }));
            });
        }
    }

    private void AppendData(string text)
    {
        if (dataTextBox.InvokeRequired)
        {
            dataTextBox.Invoke(new Action<string>(AppendData), text);
        }
        else
        {
            // Get the current scroll position
            int currentScrollPosition = dataTextBox.GetLineFromCharIndex(dataTextBox.SelectionStart);
            int totalLines = dataTextBox.GetLineFromCharIndex(dataTextBox.TextLength);
            bool wasAtBottom = (currentScrollPosition + dataTextBox.ClientSize.Height / dataTextBox.Font.Height) >= totalLines;
            
            // Store the current scroll position
            int currentPosition = dataTextBox.SelectionStart;
            
            dataTextBox.AppendText(text);
            
            // Only auto-scroll if we were at the bottom
            if (wasAtBottom)
            {
                dataTextBox.SelectionStart = dataTextBox.Text.Length;
                dataTextBox.ScrollToCaret();
            }
            else
            {
                // Restore the previous scroll position
                dataTextBox.SelectionStart = currentPosition;
                dataTextBox.ScrollToCaret();
            }
        }
    }

    // Add maximize/restore toggle logic
    private void ToggleMaximizePlot(int plotNumber)
    {
        if (maximizedGraphForm != null)
        {
            maximizedGraphForm.Close();
            maximizedGraphForm = null;
            return;
        }
        maximizedGraphForm = new Form
        {
            Text = plotNumber == 1 ? "Maximized Graph 1" : "Maximized Graph 2",
            WindowState = FormWindowState.Maximized,
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.CenterScreen,
            BackColor = Color.FromArgb(30, 30, 30)
        };
        maximizedGraphForm.Tag = plotNumber; // Store which plot this is

        var maxPlot = new ScottPlot.WinForms.FormsPlot
        {
            Dock = DockStyle.Fill
        };
        maximizedGraphForm.Controls.Add(maxPlot);

        // Add a clearly visible close button
        var closeButton = new Button
        {
            Text = "🗙",
            Size = new Size(48, 48),
            Location = new Point(maximizedGraphForm.ClientSize.Width - 58, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(60, 0, 0),
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            TabStop = false
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(120, 0, 0);
        closeButton.Click += (s, e) => { maximizedGraphForm.Close(); maximizedGraphForm = null; };
        maximizedGraphForm.Controls.Add(closeButton);
        closeButton.BringToFront();

        // Plot the correct data
        graphManager.UpdateMaximizedPlot(maxPlot, plotNumber);

        maximizedGraphForm.FormClosing += (s, e) => { maximizedGraphForm = null; };
        maximizedGraphForm.Shown += (s, e) =>
        {
            closeButton.Location = new Point(maximizedGraphForm.ClientSize.Width - closeButton.Width - 10, 10);
            closeButton.BringToFront();
        };
        maximizedGraphForm.Resize += (s, e) =>
        {
            closeButton.Location = new Point(maximizedGraphForm.ClientSize.Width - closeButton.Width - 10, 10);
            closeButton.BringToFront();
        };
        maximizedGraphForm.Show();
    }

    private void ShowPreferences()
    {
        var configManager = new ArduinoCliConfigManager();
        var urls = configManager.GetBoardsManagerUrls();
        var prefForm = new PreferencesForm(urls);
        prefForm.OnSave += async (newUrls) =>
        {
            configManager.SetBoardsManagerUrls(newUrls);
            configManager.Save();
            var result = await ArduinoCliService.RunCommandAsync("arduino-cli core update-index");
            MessageBox.Show("Boards manager URLs updated and index refreshed!\n" + result, "Success");
        };
        prefForm.ShowDialog();
    }
}

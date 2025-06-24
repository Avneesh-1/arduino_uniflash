using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace UniFlash.IDE
{
    public partial class IDEWindow : Form
    {
        private TabControl tabControl;
        private RichTextBox outputBox;
        private SerialPort serialPort;
        private bool isUploading = false;
        private ComboBox portComboBox;
        private Button uploadButton;
        private Button refreshButton;
        private TableLayoutPanel mainLayout;
        private MenuStrip menuStrip;
        private TabControl bottomTabControl;
        private RichTextBox serialMonitorBox;
        private Button serialConnectButton;
        private SerialPort monitorSerialPort;
        private bool isSerialConnected = false;
        private ComboBox baudComboBox;
        private ArduinoLibraryManager libraryManager;
        private ArduinoUploader uploader;
        private ComboBox deviceComboBox;
        private Label baudLabel;
        private ComboBox programmerComboBox;
        private Label programmerLabel;
        private string selectedProgrammer = "SerialUPDI (115200 baud)";

        private static readonly Dictionary<string, string> HeaderToLibraryMap = new()
        {
            {"#include <WiFi.h>", "WiFi"},
            {"#include <WebServer.h>", "WebServer"},
            {"#include <ESPAsyncWebServer.h>", "ESPAsyncWebServer"},
            {"#include <PubSubClient.h>", "PubSubClient"},
            {"#include <ArduinoJson.h>", "ArduinoJson"},
            {"#include <SPIFFS.h>", "SPIFFS"},
            {"#include <Preferences.h>", "Preferences"},
            {"#include <Wire.h>", "Wire"},
            {"#include <SPI.h>", "SPI"},
            {"#include <Servo.h>", "Servo"},
            {"#include <Stepper.h>", "Stepper"},
            {"#include <LiquidCrystal.h>", "LiquidCrystal"},
            {"#include <Adafruit_GFX.h>", "Adafruit_GFX"},
            {"#include <Adafruit_SSD1306.h>", "Adafruit_SSD1306"},
            {"#include <DHT.h>", "DHT sensor library"},
            {"#include <OneWire.h>", "OneWire"},
            {"#include <DallasTemperature.h>", "DallasTemperature"},
            {"#include <NewPing.h>", "NewPing"},
            {"#include <IRremote.h>", "IRremote"},
            {"#include <RF24.h>", "RF24"},
            {"#include <Ethernet.h>", "Ethernet"},
            {"#include <SD.h>", "SD"},
            {"#include <RTClib.h>", "RTClib"},
            {"#include <Adafruit_Sensor.h>", "Adafruit Unified Sensor"},
            {"#include <Adafruit_BMP280.h>", "Adafruit BMP280 Library"},
            {"#include <Adafruit_MPU6050.h>", "Adafruit MPU6050"},
            {"#include <Adafruit_NeoPixel.h>", "Adafruit NeoPixel"},
            {"#include <FastLED.h>", "FastLED"},
            {"#include <U8g2lib.h>", "U8g2"},
            {"#include <TFT_eSPI.h>", "TFT_eSPI"},
            {"#include <lvgl.h>", "lvgl"},
            {"#include <Arduino_JSON.h>", "Arduino_JSON"},
            {"#include <HTTPClient.h>", "HTTPClient"},
            {"#include <WebSocketsClient.h>", "WebSocketsClient"},
            {"#include <ArduinoOTA.h>", "ArduinoOTA"},
            {"#include <Update.h>", "Update"},
            {"#include <esp_sleep.h>", "esp_sleep"},
            {"#include <esp_wifi.h>", "esp_wifi"},
            {"#include <esp_bt.h>", "esp_bt"},
            {"#include <esp_timer.h>", "esp_timer"},
            {"#include <esp_system.h>", "esp_system"},
            {"#include <esp_spi_flash.h>", "esp_spi_flash"},
            {"#include <esp_partition.h>", "esp_partition"},
            {"#include <esp_ota_ops.h>", "esp_ota_ops"},
            {"#include <esp_http_client.h>", "esp_http_client"},
            {"#include <esp_websocket_client.h>", "esp_websocket_client"},
            {"#include <esp_event.h>", "esp_event"},
            {"#include <nvs_flash.h>", "nvs_flash"},
            {"#include <driver/gpio.h>", "driver/gpio"},
            {"#include <driver/adc.h>", "driver/adc"},
            {"#include <driver/dac.h>", "driver/dac"},
            {"#include <driver/i2c.h>", "driver/i2c"},
            {"#include <driver/spi_master.h>", "driver/spi_master"},
            {"#include <driver/uart.h>", "driver/uart"},
            {"#include <driver/pwm.h>", "driver/pwm"},
            {"#include <driver/ledc.h>", "driver/ledc"},
            {"#include <driver/rmt.h>", "driver/rmt"},
            {"#include <driver/can.h>", "driver/can"},
            {"#include <driver/touch_pad.h>", "driver/touch_pad"},
            {"#include <driver/hall_sensor.h>", "driver/hall_sensor"},
            {"#include <driver/rtc_io.h>", "driver/rtc_io"},
            {"#include <driver/rtc_cntl.h>", "driver/rtc_cntl"},
            {"#include <driver/rtc_wdt.h>", "driver/rtc_wdt"},
            {"#include <driver/rtc_temp.h>", "driver/rtc_temp"},
            {"#include <driver/rtc_mem.h>", "driver/rtc_mem"},
            {"#include <driver/rtc_clk.h>", "driver/rtc_clk"},
            {"#include <driver/rtc_periph.h>", "driver/rtc_periph"},
            {"#include <driver/rtc_pm.h>", "driver/rtc_pm"},
            {"#include <driver/rtc_sleep.h>", "driver/rtc_sleep"},
            {"#include <driver/rtc_wake.h>", "driver/rtc_wake"},
            {"#include <driver/rtc_init.h>", "driver/rtc_init"},
            {"#include <driver/rtc_common.h>", "driver/rtc_common"},
            {"#include <driver/rtc.h>", "driver/rtc"}
        };

        public IDEWindow(string preferredDevice = null)
        {
            InitializeComponent();
            InitializeMainLayout();
            InitializeMenu();
            InitializeToolbar();
            InitializeTabControl();
            InitializeBottomPanel();

            // Initialize managers after outputBox is created
            libraryManager = new ArduinoLibraryManager(outputBox);
            uploader = new ArduinoUploader();

            this.Load += async (s, e) => await IDEWindow_Load(s, e, preferredDevice);
        }

        private async Task IDEWindow_Load(object sender, EventArgs e, string preferredDevice)
        {
            LoadComPorts();
            LoadDevices();

            if (!string.IsNullOrEmpty(preferredDevice))
            {
                var index = deviceComboBox.Items.IndexOf(preferredDevice);
                if (index >= 0)
                    deviceComboBox.SelectedIndex = index;
            }
            else if (deviceComboBox.Items.Count > 0)
            {
                deviceComboBox.SelectedIndex = 0;
            }

            // Now it's safe to attach the event handler
            deviceComboBox.SelectedIndexChanged += DeviceComboBox_SelectedIndexChanged;

            // Perform initial board detection
            await PerformInitialBoardDetection();
        }

        private async Task PerformInitialBoardDetection()
        {
            try
            {
                AppendColoredText(outputBox, "Detecting connected boards...\n", Color.Cyan);
                var detector = new BoardDetectionService();
                
                // Add timeout to prevent hanging
                var detectionTask = detector.DetectConnectedBoardsAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10)); // 10 second timeout
                
                var completedTask = await Task.WhenAny(detectionTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    AppendColoredText(outputBox, "Board detection timed out. Please check your connections and try again.\n", Color.Red);
                    return;
                }
                
                var boards = await detectionTask;
                
                if (boards.Count > 0)
                {
                    AppendColoredText(outputBox, $"Found {boards.Count} connected board(s):\n", Color.Green);
                    foreach (var board in boards)
                    {
                        AppendColoredText(outputBox, $"  - {board.BoardName} on {board.PortName}\n", Color.White);
                    }
                }
                else
                {
                    AppendColoredText(outputBox, "No boards detected. Please connect a board and refresh.\n", Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                AppendColoredText(outputBox, $"Board detection error: {ex.Message}\n", Color.Red);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "IDEWindow";
            this.Text = "UniFlash IDE";
            this.ResumeLayout(false);
        }

        private void InitializeMainLayout()
        {
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);
        }

        private void InitializeMenu()
        {
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New Sketch", null, NewSketchHandler);
            fileMenu.DropDownItems.Add("Open", null, OpenHandler);
            fileMenu.DropDownItems.Add("Save", null, SaveHandler);
            fileMenu.DropDownItems.Add("Save As", null, SaveAsHandler);
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("Close", null, CloseHandler);

            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.Add("Library Manager", null, (s, e) => OpenLibraryManager(s, e));
            toolsMenu.DropDownItems.Add("Preferences", null, (s, e) => ShowPreferences());
            toolsMenu.DropDownItems.Add("-");
            toolsMenu.DropDownItems.Add("Debug Board Detection", null, (s, e) => DebugBoardDetection(s, e));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(toolsMenu);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void InitializeToolbar()
        {
            var toolbarPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var portLabel = new Label
            {
                Text = "Port:",
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 8, 5, 0)
            };

            portComboBox = new ComboBox
            {
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 10, 0)
            };

            var deviceLabel = new Label
            {
                Text = "Device:",
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 8, 5, 0)
            };

            deviceComboBox = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 10, 0)
            };

            baudLabel = new Label
            {
                Text = "Baud:",
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 8, 5, 0)
            };

            baudComboBox = new ComboBox
            {
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 10, 0)
            };
            baudComboBox.Items.AddRange(new object[] { 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 });
            baudComboBox.SelectedItem = 9600;

            programmerLabel = new Label
            {
                Text = "Programmer:",
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 8, 5, 0),
                Visible = false
            };

            programmerComboBox = new ComboBox
            {
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 10, 0),
                Visible = false
            };
            programmerComboBox.Items.AddRange(new string[] {
                "Atmel-ICE UPDI",
                "Curiosity Nano",
                "JTAG2UPDI",
                "JTAGICE3 UPDI",
                "microUPDI/Uno Wifi",
                "MPLAB SNAP UPDI",
                "PICkit4 UPDI",
                "SerialUPDI (115200 baud)",
                "SerialUPDI (230400 baud)",
                "SerialUPDI (460800 baud)",
                "SerialUPDI (57600 baud)",
                "Xplained Pro"
            });
            programmerComboBox.SelectedIndex = 7;
            programmerComboBox.SelectedIndexChanged += (s, e) =>
            {
                selectedProgrammer = programmerComboBox.SelectedItem?.ToString() ?? "SerialUPDI (115200 baud)";
            };

            refreshButton = new Button
            {
                Text = "⟳",
                Width = 44,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 5, 5),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            refreshButton.FlatAppearance.BorderColor = Color.HotPink;
            refreshButton.FlatAppearance.BorderSize = 1;
            refreshButton.Click += async (s, e) => { 
                LoadComPorts(); 
                await PerformInitialBoardDetection();
            };

            uploadButton = new Button
            {
                Text = "Upload",
                Width = 120,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(10, 5, 5, 5),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            uploadButton.FlatAppearance.BorderColor = Color.HotPink;
            uploadButton.FlatAppearance.BorderSize = 1;
            uploadButton.Click += UploadButton_Click;

            deviceComboBox.Items.Clear();
            deviceComboBox.Items.Add("Arduino Uno");
            deviceComboBox.Items.Add("ATmega4809");
            deviceComboBox.Items.Add("ESP32");
            deviceComboBox.Items.Add("ESP32 WROOM");
            deviceComboBox.SelectedIndex = 0;
            deviceComboBox.SelectedIndexChanged += DeviceComboBox_SelectedIndexChanged;

            toolbarPanel.Controls.AddRange(new Control[] { portLabel, portComboBox, deviceLabel, deviceComboBox, baudLabel, baudComboBox, programmerLabel, programmerComboBox, refreshButton, uploadButton });
            mainLayout.Controls.Add(toolbarPanel, 0, 0);
        }

        private void InitializeTabControl()
        {
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                DrawMode = TabDrawMode.OwnerDrawFixed
            };
            tabControl.DrawItem += TabControl_DrawItem;
            tabControl.MouseDown += TabControl_MouseDown;
            mainLayout.Controls.Add(tabControl, 0, 1);
            AddNewTab("sketch1.ino");
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = tabControl.TabPages[e.Index];
            var tabRect = tabControl.GetTabRect(e.Index);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, Color.Black);
            int x = tabRect.Right - 20;
            int y = tabRect.Top + (tabRect.Height - 16) / 2;
            e.Graphics.DrawString("×", new Font("Segoe UI", 12, FontStyle.Bold), Brushes.Red, x, y);
        }

        private void TabControl_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                var tabRect = tabControl.GetTabRect(i);
                Rectangle closeRect = new Rectangle(tabRect.Right - 24, tabRect.Top + 4, 20, tabRect.Height - 8);
                if (closeRect.Contains(e.Location))
                {
                    if (tabControl.TabPages.Count > 1)
                        tabControl.TabPages.RemoveAt(i);
                    break;
                }
            }
        }

        private void InitializeBottomPanel()
        {
            bottomTabControl = new TabControl
            {
                Dock = DockStyle.Bottom,
                Height = 180,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            outputBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(255, 85, 85),
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.None
            };
            var outputTab = new TabPage("Output") { BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            outputTab.Controls.Add(outputBox);

            var serialPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };
            serialMonitorBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(180, 255, 180),
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.None
            };
            serialConnectButton = new Button
            {
                Text = "Connect",
                Width = 100,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Margin = new Padding(10, 5, 5, 5),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top
            };
            serialConnectButton.FlatAppearance.BorderColor = Color.HotPink;
            serialConnectButton.FlatAppearance.BorderSize = 1;
            serialConnectButton.Click += SerialConnectButton_Click;
            serialPanel.Controls.Add(serialMonitorBox);
            serialPanel.Controls.Add(serialConnectButton);
            var serialTab = new TabPage("Serial Monitor") { BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            serialTab.Controls.Add(serialPanel);

            bottomTabControl.TabPages.Add(outputTab);
            bottomTabControl.TabPages.Add(serialTab);
            this.Controls.Add(bottomTabControl);
            bottomTabControl.BringToFront();
        }

        private void AddNewTab(string fileName)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };
            var lineNumbers = new RichTextBox
            {
                Width = 40,
                Dock = DockStyle.Left,
                ReadOnly = true,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.FromArgb(100, 180, 180, 180),
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                Enabled = false
            };
            var codeBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Consolas", 12),
                BorderStyle = BorderStyle.None,
                WordWrap = false,
                AcceptsTab = true
            };
            codeBox.TextChanged += (s, e) => UpdateLineNumbers(codeBox, lineNumbers);
            codeBox.VScroll += (s, e) => SyncLineNumbersScroll(codeBox, lineNumbers);
            panel.Controls.Add(codeBox);
            panel.Controls.Add(lineNumbers);
            var tab = new TabPage(fileName) { BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            tab.Controls.Add(panel);
            tabControl.TabPages.Add(tab);
            tabControl.SelectedTab = tab;
            UpdateLineNumbers(codeBox, lineNumbers);
        }

        private void UpdateLineNumbers(RichTextBox codeBox, RichTextBox lineNumbers)
        {
            int count = codeBox.Lines.Length;
            if (count == 0) count = 1;
            var text = string.Join("\n", Enumerable.Range(1, count));
            lineNumbers.Text = text;
        }

        private void SyncLineNumbersScroll(RichTextBox codeBox, RichTextBox lineNumbers)
        {
            int d = GetScrollPos(codeBox.Handle, 1);
            SetScrollPos(lineNumbers.Handle, 1, d, true);
            SendMessage(lineNumbers.Handle, 0x115, (IntPtr)4, (IntPtr)0);
        }

        [DllImport("user32.dll")] static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll")] static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")] static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private (RichTextBox codeBox, RichTextBox lineNumbers) GetCurrentEditor()
        {
            if (tabControl.SelectedTab != null && tabControl.SelectedTab.Controls.Count > 0)
            {
                var panel = tabControl.SelectedTab.Controls[0] as Panel;
                if (panel != null && panel.Controls.Count >= 2)
                {
                    var codeBox = panel.Controls[0] as RichTextBox;
                    var lineNumbers = panel.Controls[1] as RichTextBox;
                    return (codeBox, lineNumbers);
                }
            }
            return (null, null);
        }

        private void NewSketchHandler(object sender, EventArgs e)
        {
            AddNewTab($"sketch{tabControl.TabPages.Count + 1}.ino");
        }

        private void OpenHandler(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Arduino Files (*.ino)|*.ino|All Files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var code = File.ReadAllText(openFileDialog.FileName);
                    AddNewTab(Path.GetFileName(openFileDialog.FileName));
                    var (codeBox, _) = GetCurrentEditor();
                    if (codeBox != null) codeBox.Text = code;
                }
            }
        }

        private void SaveHandler(object sender, EventArgs e)
        {
            var (codeBox, _) = GetCurrentEditor();
            if (codeBox == null) return;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Arduino Files (*.ino)|*.ino|All Files (*.*)|*.*";
                saveFileDialog.FileName = tabControl.SelectedTab.Text;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, codeBox.Text);
                    tabControl.SelectedTab.Text = Path.GetFileName(saveFileDialog.FileName);
                }
            }
        }

        private void SaveAsHandler(object sender, EventArgs e) => SaveHandler(sender, e);

        private void CloseHandler(object sender, EventArgs e)
        {
            if (tabControl.TabPages.Count > 0)
                tabControl.TabPages.Remove(tabControl.SelectedTab);
        }

        private void LoadComPorts()
        {
            portComboBox.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            portComboBox.Items.AddRange(ports);
            if (portComboBox.Items.Count > 0)
                portComboBox.SelectedIndex = 0;
        }

        private void LoadDevices()
        {
            var selected = deviceComboBox.SelectedItem?.ToString();

            deviceComboBox.Items.Clear();
            
            // Standard Arduino boards
            deviceComboBox.Items.Add("Arduino Uno");
            deviceComboBox.Items.Add("Arduino Nano");
            deviceComboBox.Items.Add("Arduino Mega or Mega 2560");
            deviceComboBox.Items.Add("Arduino Leonardo");
            deviceComboBox.Items.Add("Arduino Micro");
            deviceComboBox.Items.Add("Arduino Pro or Pro Mini");
            deviceComboBox.Items.Add("Arduino Uno WiFi");
            deviceComboBox.Items.Add("Arduino Uno Mini");
            deviceComboBox.Items.Add("Arduino Yún");
            deviceComboBox.Items.Add("Arduino Yún Mini");
            deviceComboBox.Items.Add("Arduino Ethernet");
            deviceComboBox.Items.Add("Arduino Fio");
            deviceComboBox.Items.Add("Arduino Gemma");
            deviceComboBox.Items.Add("Arduino Esplora");
            deviceComboBox.Items.Add("Arduino Robot Control");
            deviceComboBox.Items.Add("Arduino Robot Motor");
            deviceComboBox.Items.Add("Arduino Industrial 101");
            deviceComboBox.Items.Add("Arduino Mega ADK");
            deviceComboBox.Items.Add("Arduino Mini");
            deviceComboBox.Items.Add("Arduino NG or older");
            deviceComboBox.Items.Add("Arduino BT");
            deviceComboBox.Items.Add("LilyPad Arduino");
            deviceComboBox.Items.Add("LilyPad Arduino USB");
            deviceComboBox.Items.Add("Linino One");
            deviceComboBox.Items.Add("Adafruit Circuit Playground");
            
            // MegaCoreX boards
            deviceComboBox.Items.Add("ATmega4809");
            deviceComboBox.Items.Add("ATmega4808");
            deviceComboBox.Items.Add("ATmega3209");
            deviceComboBox.Items.Add("ATmega3208");
            deviceComboBox.Items.Add("ATmega1609");
            deviceComboBox.Items.Add("ATmega1608");
            deviceComboBox.Items.Add("ATmega809");
            deviceComboBox.Items.Add("ATmega808");
            
            // ESP32 boards
            deviceComboBox.Items.Add("ESP32");
            deviceComboBox.Items.Add("ESP32 WROOM");
            deviceComboBox.Items.Add("ESP32 Dev Module");
            deviceComboBox.Items.Add("ESP32 WROVER");
            deviceComboBox.Items.Add("ESP32 PICO");
            deviceComboBox.Items.Add("ESP32 S2");
            deviceComboBox.Items.Add("ESP32 S3");
            deviceComboBox.Items.Add("ESP32 C3");
            
            if (selected != null && deviceComboBox.Items.Contains(selected))
            {
                deviceComboBox.SelectedItem = selected;
            }
        }

        private void AppendColoredText(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.ScrollToCaret();
        }

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            if (isUploading) return;

            var (codeBox, _) = GetCurrentEditor();
            if (codeBox == null)
            {
                MessageBox.Show("No code to upload. Please create or open a sketch.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(portComboBox.SelectedItem?.ToString()))
            {
                MessageBox.Show("Please select a COM port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedDevice = deviceComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDevice))
            {
                MessageBox.Show("Please select a device type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

                isUploading = true;
                uploadButton.Enabled = false;
            uploadButton.Text = "Uploading...";

            try
            {
                outputBox.Clear();
                AppendColoredText(outputBox, "Starting upload process...\n", Color.Yellow);

                var code = codeBox.Text;
                var port = portComboBox.SelectedItem.ToString();

                // Ensure required libraries are installed
                await EnsureLibrariesInstalled(code);

                // Ensure board core is installed
                var fqbn = GetFQBN(selectedDevice);
                if (!string.IsNullOrEmpty(fqbn))
                {
                    await EnsureBoardCoreInstalled(fqbn);
                }

                // Set the uploader's board type based on the selected device
                uploader.SetBoardType(selectedDevice);

                // Upload the code with progress reporting
                var result = await uploader.UploadCode(
                    code,
                    port,
                    new Progress<string>(msg => {
                        if (msg.Contains("failed", StringComparison.OrdinalIgnoreCase) || msg.Contains("error", StringComparison.OrdinalIgnoreCase))
                        AppendColoredText(outputBox, msg + "\n", Color.Red);
                        else if (msg.Contains("success", StringComparison.OrdinalIgnoreCase) || msg.Contains("finished", StringComparison.OrdinalIgnoreCase))
                        AppendColoredText(outputBox, msg + "\n", Color.Green);
                        else if (msg.Contains("warning", StringComparison.OrdinalIgnoreCase))
                            AppendColoredText(outputBox, msg + "\n", Color.Orange);
                        else
                            AppendColoredText(outputBox, msg + "\n", Color.White);
                    }),
                    selectedProgrammer
                );

                if (result)
                {
                    AppendColoredText(outputBox, "Upload completed successfully!\n", Color.Green);
                }
                else
                {
                    AppendColoredText(outputBox, "Upload failed.\n", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendColoredText(outputBox, $"Error during upload: {ex.Message}\n", Color.Red);
            }
            finally
            {
                isUploading = false;
                uploadButton.Enabled = true;
                uploadButton.Text = "Upload";
            }
        }

        private async Task EnsureLibrariesInstalled(string code)
        {
            var requiredLibraries = new List<string>();

            foreach (var line in code.Split('\n'))
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("#include"))
                {
                    var libraryName = ExtractLibraryName(trimmedLine);
                    if (!string.IsNullOrEmpty(libraryName) && !IsBuiltInLibrary(libraryName))
                    {
                        requiredLibraries.Add(libraryName);
                    }
                }
            }

            foreach (var library in requiredLibraries.Distinct())
            {
                AppendColoredText(outputBox, $"Checking library: {library}\n", Color.Cyan);
                var isInstalled = await libraryManager.InstallLibrary(library);
                if (isInstalled)
                {
                    AppendColoredText(outputBox, $"Library {library} installed successfully.\n", Color.Green);
                            }
                            else
                            {
                    AppendColoredText(outputBox, $"Failed to install library {library}.\n", Color.Red);
                }
            }
        }

        private string ExtractLibraryName(string includeLine)
        {
            if (HeaderToLibraryMap.TryGetValue(includeLine, out var libraryName))
            {
                return libraryName;
            }

            // Try to extract from angle brackets
            var match = System.Text.RegularExpressions.Regex.Match(includeLine, @"#include\s*[<""]([^>""]+)[>""]");
            if (match.Success)
            {
                var header = match.Groups[1].Value;
                if (HeaderToLibraryMap.TryGetValue($"#include <{header}>", out var mappedLibrary))
                {
                    return mappedLibrary;
                }
                return header;
            }

            return null;
        }

        private bool IsBuiltInLibrary(string libraryName)
        {
            var builtInLibraries = new[]
            {
                "Arduino", "Wire", "SPI", "SoftwareSerial", "EEPROM", "Servo", "Stepper",
                "LiquidCrystal", "SD", "Ethernet", "WiFi", "WebServer", "ESPAsyncWebServer",
                "PubSubClient", "ArduinoJson", "SPIFFS", "Preferences", "Update", "ArduinoOTA",
                "esp_sleep", "esp_wifi", "esp_bt", "esp_timer", "esp_system", "esp_spi_flash",
                "esp_partition", "esp_ota_ops", "esp_http_client", "esp_websocket_client",
                "esp_event", "nvs_flash", "driver/gpio", "driver/adc", "driver/dac", "driver/i2c",
                "driver/spi_master", "driver/uart", "driver/pwm", "driver/ledc", "driver/rmt",
                "driver/can", "driver/touch_pad", "driver/hall_sensor", "driver/rtc_io",
                "driver/rtc_cntl", "driver/rtc_wdt", "driver/rtc_temp", "driver/rtc_mem",
                "driver/rtc_clk", "driver/rtc_periph", "driver/rtc_pm", "driver/rtc_sleep",
                "driver/rtc_wake", "driver/rtc_init", "driver/rtc_common", "driver/rtc"
            };

            return builtInLibraries.Contains(libraryName, StringComparer.OrdinalIgnoreCase);
        }

        private async Task EnsureBoardCoreInstalled(string fqbn)
        {
            if (string.IsNullOrEmpty(fqbn)) return;

            AppendColoredText(outputBox, $"Checking board core for: {fqbn}\n", Color.Cyan);
            
            try
            {
                // For ESP32 boards, ensure the board manager URL is added
                if (fqbn.StartsWith("esp32:"))
                {
                    AppendColoredText(outputBox, "Ensuring ESP32 board manager URL is configured...\n", Color.Cyan);
                    var configManager = new ArduinoCliConfigManager();
                    var currentUrls = configManager.GetBoardsManagerUrls();
                    
                    if (!currentUrls.Contains("https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json"))
                    {
                        var newUrls = currentUrls.ToList();
                        newUrls.Add("https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json");
                        configManager.SetBoardsManagerUrls(newUrls.ToArray());
                        configManager.Save();
                        AppendColoredText(outputBox, "ESP32 board manager URL added to configuration.\n", Color.Green);
                    }
                    else
                    {
                        AppendColoredText(outputBox, "ESP32 board manager URL already configured.\n", Color.Green);
                    }
                }

                var result = await RunCliCommand($"core list");
                if (!result.Contains(fqbn.Split(':')[0]))
                {
                    AppendColoredText(outputBox, $"Installing board core for: {fqbn}\n", Color.Yellow);
                    var installResult = await RunCliCommand($"core install {fqbn.Split(':')[0]}");
                    AppendColoredText(outputBox, $"Board core installation result: {installResult}\n", Color.Green);
                }
                else
                {
                    AppendColoredText(outputBox, $"Board core for {fqbn} is already installed.\n", Color.Green);
                }
            }
            catch (Exception ex)
            {
                AppendColoredText(outputBox, $"Error checking/installing board core: {ex.Message}\n", Color.Red);
            }
        }

        private Task<string> RunCliCommand(string command)
        {
            return Task.Run(() =>
            {
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "arduino-cli",
                        Arguments = command,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = System.Diagnostics.Process.Start(startInfo);
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        return output + error;
                }
                catch (Exception ex)
                {
                    return $"Error running arduino-cli: {ex.Message}";
                }
            });
        }

        private void SerialConnectButton_Click(object sender, EventArgs e)
        {
            if (isSerialConnected)
            {
                TryCloseMonitorPort();
                serialConnectButton.Text = "Connect";
                isSerialConnected = false;
                AppendColoredText(serialMonitorBox, "Disconnected from serial port.\n", Color.Yellow);
            }
            else
            {
                if (string.IsNullOrEmpty(portComboBox.SelectedItem?.ToString()))
                {
                    MessageBox.Show("Please select a COM port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    monitorSerialPort = new SerialPort(portComboBox.SelectedItem.ToString(), (int)baudComboBox.SelectedItem);
                    monitorSerialPort.DataReceived += MonitorSerialPort_DataReceived;
                    monitorSerialPort.Open();
                    serialConnectButton.Text = "Disconnect";
                    isSerialConnected = true;
                    AppendColoredText(serialMonitorBox, "Connected to serial port.\n", Color.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to serial port: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TryCloseMonitorPort()
            {
                if (monitorSerialPort != null && monitorSerialPort.IsOpen)
            {
                    monitorSerialPort.Close();
                monitorSerialPort.Dispose();
                monitorSerialPort = null;
            }
        }

        private void MonitorSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (monitorSerialPort != null && monitorSerialPort.IsOpen)
            {
                var data = monitorSerialPort.ReadExisting();
                if (serialMonitorBox.InvokeRequired)
                {
                    serialMonitorBox.Invoke(new Action(() => AppendColoredText(serialMonitorBox, data, Color.White)));
                }
                else
                {
                    AppendColoredText(serialMonitorBox, data, Color.White);
                }
            }
        }

        private void IDEWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            TryCloseMonitorPort();
        }

        private void OpenLibraryManager(object sender, EventArgs e)
        {
            var libraryForm = new LibraryManagerForm(libraryManager);
            libraryForm.Show();
        }

        private void ShowPreferences()
        {
            var configManager = new ArduinoCliConfigManager();
            var initialUrls = configManager.GetBoardsManagerUrls();
            var preferencesForm = new PreferencesForm(initialUrls);
            preferencesForm.ShowDialog();
        }

        private async void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedDevice = deviceComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDevice)) return;

            // Show/hide programmer selection based on device type
            bool showProgrammer = selectedDevice.StartsWith("ATmega");
            programmerLabel.Visible = showProgrammer;
            programmerComboBox.Visible = showProgrammer;

            // Set appropriate baud rate
            if (selectedDevice.StartsWith("ESP32"))
            {
                baudComboBox.SelectedItem = 115200;
            }
            else if (selectedDevice.StartsWith("ATmega"))
            {
                baudComboBox.SelectedItem = 115200;
            }
            else
            {
                baudComboBox.SelectedItem = 9600;
            }

            // Update output with device information
            AppendColoredText(outputBox, $"Selected device: {selectedDevice}\n", Color.Cyan);
            
            var fqbn = GetFQBN(selectedDevice);
            if (!string.IsNullOrEmpty(fqbn))
            {
                AppendColoredText(outputBox, $"FQBN: {fqbn}\n", Color.Cyan);
            }

            // Auto-detect port for the selected device (asynchronously)
            try
            {
                var detectedPort = await DetectDevicePortAsync(selectedDevice);
                if (!string.IsNullOrEmpty(detectedPort))
                {
                    var portIndex = portComboBox.Items.IndexOf(detectedPort);
                    if (portIndex >= 0)
                    {
                        portComboBox.SelectedIndex = portIndex;
                        AppendColoredText(outputBox, $"Auto-detected port: {detectedPort}\n", Color.Green);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendColoredText(outputBox, $"Port detection failed: {ex.Message}\n", Color.Red);
            }
        }

        private async Task<string> DetectDevicePortAsync(string deviceType)
        {
            try
            {
                var detector = new BoardDetectionService();
                
                // Add timeout to prevent hanging
                var detectionTask = detector.DetectConnectedBoardsAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5)); // 5 second timeout for device-specific detection
                
                var completedTask = await Task.WhenAny(detectionTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    AppendColoredText(outputBox, "Device detection timed out.\n", Color.Red);
            return null;
                }
                
                var detectedBoards = await detectionTask;
                
                foreach (var board in detectedBoards)
                {
                    if (deviceType.StartsWith("ESP32") && board.BoardType.Contains("ESP32"))
                    {
                        return board.PortName;
                    }
                    else if (deviceType.StartsWith("ATmega") && board.BoardType.Contains("ATmega"))
                    {
                        return board.PortName;
                    }
                    else if (deviceType.StartsWith("Arduino") && board.BoardType.Contains("Arduino"))
                    {
                        return board.PortName;
                    }
                }
            }
            catch (Exception ex)
            {
                AppendColoredText(outputBox, $"Error detecting device port: {ex.Message}\n", Color.Red);
            }
            
            return null;
        }

        private async void DebugBoardDetection(object sender, EventArgs e)
        {
            AppendColoredText(outputBox, "=== Board Detection Debug ===\n", Color.Yellow);
            
            try
            {
                var detector = new BoardDetectionService();
                
                // Add timeout to prevent hanging
                var detectionTask = detector.DetectConnectedBoardsAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10)); // 10 second timeout
                
                var completedTask = await Task.WhenAny(detectionTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    AppendColoredText(outputBox, "Board detection debug timed out.\n", Color.Red);
                return;
            }

                var boards = await detectionTask;
                
                AppendColoredText(outputBox, $"Found {boards.Count} connected boards:\n", Color.Cyan);
                
                foreach (var board in boards)
                {
                    AppendColoredText(outputBox, $"Port: {board.PortName}\n", Color.White);
                    AppendColoredText(outputBox, $"Board Type: {board.BoardType}\n", Color.White);
                    AppendColoredText(outputBox, $"Board Name: {board.BoardName}\n", Color.White);
                    AppendColoredText(outputBox, $"Vendor: {board.Vendor}\n", Color.White);
                    AppendColoredText(outputBox, $"Product: {board.Product}\n", Color.White);
                    AppendColoredText(outputBox, $"Compatibility: {board.CompatibilityMessage}\n", Color.White);
                    AppendColoredText(outputBox, "---\n", Color.Gray);
                }
            }
            catch (Exception ex)
            {
                AppendColoredText(outputBox, $"Error during board detection: {ex.Message}\n", Color.Red);
            }
        }

        private string GetFQBN(string deviceType)
        {
            return deviceType switch
            {
                "Arduino Uno" => "arduino:avr:uno",
                "ATmega4809" => "megaTinyCore:megaavr:4809:pinout=48pin-standard",
                "ESP32" => "esp32:esp32:esp32",
                "ESP32 WROOM" => "esp32:esp32:esp32",
                "ESP32 Dev Module" => "esp32:esp32:esp32",
                "ESP32 WROVER" => "esp32:esp32:esp32wrover",
                "ESP32 PICO" => "esp32:esp32:esp32pico",
                "ESP32 S2" => "esp32:esp32:esp32s2",
                "ESP32 S3" => "esp32:esp32:esp32s3",
                "ESP32 C3" => "esp32:esp32:esp32c3",
                _ => null
            };
        }
    }
} 
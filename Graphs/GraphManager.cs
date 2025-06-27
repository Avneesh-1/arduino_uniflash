using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot.WinForms;
using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace UniFlash.Graphs
{
    public class GraphManager
    {
        // Dynamic data storage for plotting
        private Dictionary<string, List<double>> parameterData = new();
        private List<double> timelapses = new List<double>();
        private DateTime? startTime = null;
        private List<Form> maximizedForms = new List<Form>();
        private FormsPlot formsPlot1;
        private FormsPlot formsPlot2;
        private ComboBox paramComboBox1;
        private ComboBox paramComboBox2;
        private CheckBox splitScreenCheckBox;
        
        // Pause functionality
        private bool isPaused = false;
        private Dictionary<string, List<double>> pausedParameterData = new();
        private List<double> pausedTimelapses = new List<double>();
        private DateTime? pauseStartTime = null;
        private double lastTimeValue = 0;
        private bool isFirstPause = true;

        public GraphManager(FormsPlot plot1, FormsPlot plot2, ComboBox combo1, ComboBox combo2, CheckBox splitScreen)
        {
            formsPlot1 = plot1;
            formsPlot2 = plot2;
            paramComboBox1 = combo1;
            paramComboBox2 = combo2;
            splitScreenCheckBox = splitScreen;
        }

        public void ClearDataAndPlots()
        {
            parameterData.Clear();
            timelapses.Clear();
            startTime = null;
            formsPlot1.Plot.Clear();
            formsPlot2.Plot.Clear();
            paramComboBox1.Items.Clear();
            paramComboBox2.Items.Clear();
            
            // Clear pause data too
            isPaused = false;
            pausedParameterData.Clear();
            pausedTimelapses.Clear();
            pauseStartTime = null;
            lastTimeValue = 0;
            isFirstPause = true;
        }

        public void ParseAndPlotSensorData(string data, Action updateUI)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Parsing data: '{data.Trim()}'");
                if (startTime == null) startTime = DateTime.Now;
                
                double t = (DateTime.Now - startTime.Value).TotalSeconds;

                // 1. Extract value at the start of the line (e.g., 62.9%)
                var startValueMatch = Regex.Match(data, @"^([\d\.\-]+)%?");
                if (startValueMatch.Success)
                {
                    string valueStr = startValueMatch.Groups[1].Value;
                    if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                    {
                        string param = "Humidity"; // or whatever you want to call it
                        if (isPaused)
                        {
                            if (!pausedParameterData.ContainsKey(param))
                                pausedParameterData[param] = new List<double>();
                            pausedParameterData[param].Add(value);
                        }
                        else
                        {
                            if (!parameterData.ContainsKey(param))
                            {
                                parameterData[param] = new List<double>();
                                paramComboBox1?.Invoke((Action)(() => {
                                    if (!paramComboBox1.Items.Contains(param)) paramComboBox1.Items.Add(param);
                                    if (paramComboBox1.Items.Count == 1) paramComboBox1.SelectedIndex = 0;
                                }));
                                paramComboBox2?.Invoke((Action)(() => {
                                    if (!paramComboBox2.Items.Contains(param)) paramComboBox2.Items.Add(param);
                                    if (paramComboBox2.Items.Count == 1) paramComboBox2.SelectedIndex = 0;
                                }));
                            }
                            parameterData[param].Add(value);
                        }
                    }
                }

                // 2. Existing logic for $Parameter$ value pairs
                var matches = Regex.Matches(data, @"\$(\w+)\$\s*([\d\.\-]+[a-zA-Z%]*)");
                bool foundAny = false;
                foreach (Match match in matches)
                {
                    string param = match.Groups[1].Value;
                    string valueWithUnit = match.Groups[2].Value;
                    string valueStr = Regex.Match(valueWithUnit, @"[\d\.\-]+", RegexOptions.Compiled).Value;
                    if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                    {
                        if (isPaused)
                        {
                            if (!pausedParameterData.ContainsKey(param))
                                pausedParameterData[param] = new List<double>();
                            pausedParameterData[param].Add(value);
                        }
                        else
                        {
                            if (!parameterData.ContainsKey(param))
                            {
                                parameterData[param] = new List<double>();
                                paramComboBox1?.Invoke((Action)(() => {
                                    if (!paramComboBox1.Items.Contains(param)) paramComboBox1.Items.Add(param);
                                    if (paramComboBox1.Items.Count == 1) paramComboBox1.SelectedIndex = 0;
                                }));
                                paramComboBox2?.Invoke((Action)(() => {
                                    if (!paramComboBox2.Items.Contains(param)) paramComboBox2.Items.Add(param);
                                    if (paramComboBox2.Items.Count == 1) paramComboBox2.SelectedIndex = 0;
                                }));
                            }
                            parameterData[param].Add(value);
                        }
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    if (isPaused)
                        pausedTimelapses.Add(t);
                    else
                        timelapses.Add(t);
                }
                // Debug: print all parameter data
                foreach (var key in parameterData.Keys)
                {
                    System.Diagnostics.Debug.WriteLine($"param={key}, count={parameterData[key].Count}, values={string.Join(",", parameterData[key])}");
                }
                if (foundAny && !isPaused) updateUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing data: {ex.Message}");
            }
        }

        public void UpdateScottPlot()
        {
            if (formsPlot1 == null || formsPlot2 == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateScottPlot: FormsPlot controls are null!");
                return;
            }
                
            // Get selected parameters, with fallback to first available parameter if none selected
            string param1 = paramComboBox1?.SelectedItem?.ToString();
            string param2 = paramComboBox2?.SelectedItem?.ToString();
            
            System.Diagnostics.Debug.WriteLine($"UpdateScottPlot: param1='{param1}', param2='{param2}', total params={parameterData.Count}");
            
            // If no parameter is selected for plot1, try to select the first available parameter
            if (string.IsNullOrEmpty(param1) && parameterData.Count > 0)
            {
                param1 = parameterData.Keys.First();
                // Update the combo box selection if possible
                if (paramComboBox1 != null && paramComboBox1.Items.Contains(param1))
                {
                    paramComboBox1.SelectedItem = param1;
                    System.Diagnostics.Debug.WriteLine($"Auto-selected param1: {param1}");
                }
            }
            
            // If no parameter is selected for plot2, try to select a different parameter
            if (string.IsNullOrEmpty(param2) && parameterData.Count > 1)
            {
                param2 = parameterData.Keys.Skip(1).FirstOrDefault();
                // Update the combo box selection if possible
                if (paramComboBox2 != null && paramComboBox2.Items.Contains(param2))
                {
                    paramComboBox2.SelectedItem = param2;
                    System.Diagnostics.Debug.WriteLine($"Auto-selected param2: {param2}");
                }
            }
            
            // Update Plot 1
            formsPlot1.Plot.Clear();
            if (!string.IsNullOrEmpty(param1) && parameterData.ContainsKey(param1))
            {
                var y1 = parameterData[param1];
                System.Diagnostics.Debug.WriteLine($"Plotting param1 '{param1}' with {y1.Count} data points");
                if (y1.Count > 0)
                {
                    var xs = Enumerable.Range(0, y1.Count).Select(i => timelapses.ElementAtOrDefault(i)).ToArray();
                    var ys = y1.ToArray();
                    var scatter = formsPlot1.Plot.Add.Scatter(xs, ys, color: ScottPlot.Color.FromColor(System.Drawing.Color.DeepSkyBlue));
                    scatter.LineWidth = 2;
                    scatter.LegendText = param1;
                    formsPlot1.Plot.Title($"Live {param1} vs Timelapse");
                    formsPlot1.Plot.XLabel("Timelapse (s)");
                    formsPlot1.Plot.YLabel(param1);
                    formsPlot1.Plot.Legend.IsVisible = true;
                    if (xs.Length > 0)
                    {
                        double windowSize = 30;
                        double maxX = xs[xs.Length - 1];
                        formsPlot1.Plot.Axes.Bottom.Min = maxX - windowSize;
                        formsPlot1.Plot.Axes.Bottom.Max = maxX;
                    }
                    double minY = ys.Min();
                    double maxY = ys.Max();
                    double range = maxY - minY;
                    formsPlot1.Plot.Axes.Left.Min = minY - (range * 0.1);
                    formsPlot1.Plot.Axes.Left.Max = maxY + (range * 0.1);
                }
            }
            formsPlot1.Refresh();
            System.Diagnostics.Debug.WriteLine("Plot1 refreshed");

            // Update Plot 2 (only if split screen is enabled)
            if (splitScreenCheckBox != null && splitScreenCheckBox.Checked)
            {
                formsPlot2.Visible = true;
                formsPlot2.Plot.Clear();
                if (!string.IsNullOrEmpty(param2) && parameterData.ContainsKey(param2))
                {
                    var y2 = parameterData[param2];
                    System.Diagnostics.Debug.WriteLine($"Plotting param2 '{param2}' with {y2.Count} data points");
                    if (y2.Count > 0)
                    {
                        var xs2 = Enumerable.Range(0, y2.Count).Select(i => timelapses.ElementAtOrDefault(i)).ToArray();
                        var ys2 = y2.ToArray();
                        var scatter2 = formsPlot2.Plot.Add.Scatter(xs2, ys2, color: ScottPlot.Color.FromColor(System.Drawing.Color.HotPink));
                        scatter2.LineWidth = 2;
                        scatter2.LegendText = param2;
                        formsPlot2.Plot.Title($"Live {param2} vs Timelapse");
                        formsPlot2.Plot.XLabel("Timelapse (s)");
                        formsPlot2.Plot.YLabel(param2);
                        formsPlot2.Plot.Legend.IsVisible = true;
                        if (xs2.Length > 0)
                        {
                            double windowSize = 30;
                            double maxX = xs2[xs2.Length - 1];
                            formsPlot2.Plot.Axes.Bottom.Min = maxX - windowSize;
                            formsPlot2.Plot.Axes.Bottom.Max = maxX;
                        }
                        double minY = ys2.Min();
                        double maxY = ys2.Max();
                        double range = maxY - minY;
                        if (range < 1)
                        {
                            minY = -1;
                            maxY = 1;
                        }
                        formsPlot2.Plot.Axes.Left.Min = minY - (range * 0.1);
                        formsPlot2.Plot.Axes.Left.Max = maxY + (range * 0.1);
                    }
                }
                formsPlot2.Refresh();
                System.Diagnostics.Debug.WriteLine("Plot2 refreshed");
            }
            else if (formsPlot2 != null)
            {
                formsPlot2.Visible = false;
            }
        }

        public void ShowMaximizedPlot(int plotNumber)
        {
            var maxForm = new Form
            {
                Text = plotNumber == 1 ? "Maximized Graph 1" : "Maximized Graph 2",
                WindowState = FormWindowState.Maximized,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var maxPlot = new FormsPlot
            {
                Dock = DockStyle.Fill
            };
            maxForm.Controls.Add(maxPlot);

            var closeButton = new Button
            {
                Text = "✕",
                Size = new Size(40, 40),
                Location = new Point(maxForm.Width - 50, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Font = new Font("Consolas", 16, FontStyle.Bold)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => maxForm.Close();
            maxForm.Controls.Add(closeButton);

            var titleLabel = new Label
            {
                Text = plotNumber == 1 ? "Graph 1 - Full Screen" : "Graph 2 - Full Screen",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Consolas", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                BackColor = Color.Transparent
            };
            maxForm.Controls.Add(titleLabel);

            maxForm.Tag = plotNumber;
            maximizedForms.Add(maxForm);
            maxForm.FormClosing += (s, e) => { maximizedForms.Remove(maxForm); };
            UpdateMaximizedPlot(maxPlot, plotNumber);
            maxForm.Show();
        }

        public void UpdateMaximizedPlot(FormsPlot plot, int plotNumber)
        {
            if (plot == null || paramComboBox1 == null || paramComboBox2 == null)
                return;
            plot.Plot.Clear();
            string param = plotNumber == 1 ? paramComboBox1.SelectedItem?.ToString() : paramComboBox2.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(param) && parameterData.ContainsKey(param))
            {
                var y = parameterData[param];
                if (y.Count > 0)
                {
                    var xs = Enumerable.Range(0, y.Count).Select(i => timelapses.ElementAtOrDefault(i)).ToArray();
                    var ys = y.ToArray();
                    var scatter = plot.Plot.Add.Scatter(xs, ys,
                        color: ScottPlot.Color.FromColor(plotNumber == 1 ? System.Drawing.Color.DeepSkyBlue : System.Drawing.Color.HotPink));
                    scatter.LineWidth = 3;
                    scatter.LegendText = param;
                    plot.Plot.Title($"Live {param} vs Timelapse", size: 24);
                    plot.Plot.XLabel("Timelapse (s)", size: 20);
                    plot.Plot.YLabel(param, size: 20);
                    plot.Plot.Legend.IsVisible = true;
                    plot.Plot.Legend.FontSize = 16;
                    if (xs.Length > 0)
                    {
                        double windowSize = 30;
                        double maxX = xs[xs.Length - 1];
                        plot.Plot.Axes.Bottom.Min = maxX - windowSize;
                        plot.Plot.Axes.Bottom.Max = maxX;
                    }
                    double minY = ys.Min();
                    double maxY = ys.Max();
                    double range = maxY - minY;
                    plot.Plot.Axes.Left.Min = minY - (range * 0.1);
                    plot.Plot.Axes.Left.Max = maxY + (range * 0.1);
                    plot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
                    plot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
                }
            }
            plot.Refresh();
        }

        public void ExportToExcelWithDialog(Form parent)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                    sfd.Title = "Save Sensor Data as Excel";
                    sfd.FileName = $"SensorData_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    if (sfd.ShowDialog(parent) == DialogResult.OK)
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var ws = workbook.Worksheets.Add("SensorData");
                            // Write headers
                            ws.Cell(1, 1).Value = "S.no";
                            ws.Cell(1, 2).Value = "Timestamp";
                            int col = 3;
                            var paramList = parameterData.Keys.ToList();
                            foreach (var param in paramList)
                            {
                                ws.Cell(1, col++).Value = param;
                            }
                            ws.Cell(1, col).Value = "Time (s)";

                            // Write data
                            int rowCount = timelapses.Count;
                            for (int i = 0; i < rowCount; i++)
                            {
                                ws.Cell(i + 2, 1).Value = i + 1; // S.no
                                ws.Cell(i + 2, 2).Value = startTime.HasValue ? startTime.Value.AddSeconds(timelapses[i]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                                col = 3;
                                foreach (var param in paramList)
                                {
                                    var list = parameterData[param];
                                    ws.Cell(i + 2, col++).Value = list.Count > i ? list[i] : "";
                                }
                                ws.Cell(i + 2, col).Value = timelapses[i];
                            }

                            // Format headers
                            var headerRange = ws.Range(1, 1, 1, 2 + paramList.Count + 1);
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                            // Format data
                            var dataRange = ws.Range(2, 1, rowCount + 1, 2 + paramList.Count + 1);
                            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            // Auto-fit columns
                            ws.Columns().AdjustToContents();

                            // Save the file
                            workbook.SaveAs(sfd.FileName);
                            MessageBox.Show("Data exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetPaused(bool paused)
        {
            if (paused && !isPaused)
            {
                // Starting pause
                isPaused = true;
                pauseStartTime = DateTime.Now;
                if (timelapses.Count > 0)
                {
                    lastTimeValue = timelapses[timelapses.Count - 1];
                }
                isFirstPause = true;
            }
            else if (!paused && isPaused)
            {
                // Ending pause - restore paused data
                isPaused = false;
                foreach (var kvp in pausedParameterData)
                {
                    if (!parameterData.ContainsKey(kvp.Key))
                    {
                        parameterData[kvp.Key] = new List<double>();
                    }
                    parameterData[kvp.Key].AddRange(kvp.Value);
                }
                timelapses.AddRange(pausedTimelapses);
                
                // Clear paused data
                pausedParameterData.Clear();
                pausedTimelapses.Clear();
                pauseStartTime = null;
                lastTimeValue = 0;
                isFirstPause = true;
            }
        }

        // Add a method to force refresh the UI when parameters are added
        public void ForceUIUpdate()
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"ForceUIUpdate called - Parameter count: {parameterData.Count}");
            foreach (var param in parameterData.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"  Parameter: {param}, Data points: {parameterData[param].Count}");
            }
            
            // Ensure combo boxes are properly populated
            if (paramComboBox1 != null && parameterData.Count > 0)
            {
                paramComboBox1.Invoke((Action)(() => {
                    // Add any missing parameters
                    foreach (var param in parameterData.Keys)
                    {
                        if (!paramComboBox1.Items.Contains(param))
                            paramComboBox1.Items.Add(param);
                    }
                    // Select first parameter if none selected
                    if (paramComboBox1.SelectedIndex == -1 && paramComboBox1.Items.Count > 0)
                        paramComboBox1.SelectedIndex = 0;
                    
                    System.Diagnostics.Debug.WriteLine($"ComboBox1 items: {paramComboBox1.Items.Count}, selected: {paramComboBox1.SelectedIndex}");
                }));
            }
            
            if (paramComboBox2 != null && parameterData.Count > 1)
            {
                paramComboBox2.Invoke((Action)(() => {
                    // Add any missing parameters
                    foreach (var param in parameterData.Keys)
                    {
                        if (!paramComboBox2.Items.Contains(param))
                            paramComboBox2.Items.Add(param);
                    }
                    // Select second parameter if none selected
                    if (paramComboBox2.SelectedIndex == -1 && paramComboBox2.Items.Count > 1)
                        paramComboBox2.SelectedIndex = 1;
                    
                    System.Diagnostics.Debug.WriteLine($"ComboBox2 items: {paramComboBox2.Items.Count}, selected: {paramComboBox2.SelectedIndex}");
                }));
            }
            
            // Force update the plots
            UpdateScottPlot();
        }
    }
} 
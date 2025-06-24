using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot.WinForms;
using ClosedXML.Excel;

namespace UniFlash.Graphs
{
    public class GraphManager
    {
        // Data storage for plotting
        private List<double> voltages = new List<double>();
        private List<double> tdsValues = new List<double>();
        private List<double> tempValues = new List<double>();
        private List<double> timelapses = new List<double>();
        private DateTime? startTime = null;

        private List<Form> maximizedForms = new List<Form>();

        private FormsPlot formsPlot1;
        private FormsPlot formsPlot2;
        private ComboBox paramComboBox1;
        private ComboBox paramComboBox2;
        private CheckBox splitScreenCheckBox;

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
            voltages.Clear();
            tdsValues.Clear();
            tempValues.Clear();
            timelapses.Clear();
            startTime = null;
            formsPlot1.Plot.Clear();
            formsPlot2.Plot.Clear();
        }

        public void ParseAndPlotSensorData(string data, Action updateUI)
        {
            try
            {
                // Debug: Log the incoming data
                System.Diagnostics.Debug.WriteLine($"Parsing data: '{data.Trim()}'");
                
                // Try parsing as CSV first
                if (data.Contains(","))
                {
                    var values = data.Split(',');
                    if (values.Length >= 3)
                    {
                        double v = double.NaN, tdsVal = double.NaN, tempVal = double.NaN;
                        double.TryParse(values[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v);
                        double.TryParse(values[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tdsVal);
                        double.TryParse(values[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempVal);

                        if (startTime == null) startTime = DateTime.Now;
                        double t = (DateTime.Now - startTime.Value).TotalSeconds;
                        timelapses.Add(t);
                        voltages.Add(v);
                        tdsValues.Add(tdsVal);
                        tempValues.Add(tempVal);

                        System.Diagnostics.Debug.WriteLine($"CSV parsed: V={v}, TDS={tdsVal}, Temp={tempVal}");
                        updateUI();
                        return;
                    }
                }

                // If not CSV, try parsing with markers
                string voltage = null, tds = null, temp = null;
                var parts = data.Split(' ');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Contains("$Voltage$"))
                    {
                        if (i + 1 < parts.Length)
                            voltage = parts[i + 1].Replace("V", "").Trim();
                    }
                    else if (parts[i].Contains("$TDS$"))
                    {
                        if (i + 1 < parts.Length)
                            tds = parts[i + 1].Trim();
                    }
                    else if (parts[i].Contains("$Temp$"))
                    {
                        if (i + 1 < parts.Length)
                            temp = parts[i + 1].Trim();
                    }
                }

                double v2 = double.NaN, tdsVal2 = double.NaN, tempVal2 = double.NaN;
                double.TryParse(voltage, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v2);
                double.TryParse(tds, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tdsVal2);
                double.TryParse(temp, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempVal2);

                // Debug: Log parsed values
                System.Diagnostics.Debug.WriteLine($"Marker parsed: V='{voltage}'->{v2}, TDS='{tds}'->{tdsVal2}, Temp='{temp}'->{tempVal2}");

                // Only add data if we successfully parsed at least one value
                if (!double.IsNaN(v2) || !double.IsNaN(tdsVal2) || !double.IsNaN(tempVal2))
                {
                    if (startTime == null) startTime = DateTime.Now;
                    double t2 = (DateTime.Now - startTime.Value).TotalSeconds;
                    timelapses.Add(t2);
                    voltages.Add(v2);
                    tdsValues.Add(tdsVal2);
                    tempValues.Add(tempVal2);

                    System.Diagnostics.Debug.WriteLine($"Data added: Time={t2}, V={v2}, TDS={tdsVal2}, Temp={tempVal2}");
                    updateUI();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No valid data parsed from input");
                }
            }
            catch (Exception ex)
            {
                // Log parsing error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error parsing data: {ex.Message}");
            }
        }

        public void UpdateScottPlot()
        {
            List<double> GetData(string param)
            {
                if (param == "Voltage") return voltages;
                if (param == "TDS") return tdsValues;
                if (param == "Temperature") return tempValues;
                return new List<double>();
            }
            string param1 = paramComboBox1.SelectedItem?.ToString() ?? "Voltage";
            string param2 = paramComboBox2.SelectedItem?.ToString() ?? "TDS";
            formsPlot1.Plot.Clear();
            var y1 = GetData(param1);
            if (timelapses.Count > 0 && y1.Count > 0)
            {
                var xs = timelapses.ToArray();
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
            formsPlot1.Refresh();

            if (splitScreenCheckBox.Checked)
            {
                formsPlot2.Visible = true;
                formsPlot2.Plot.Clear();
                var y2 = GetData(param2);
                if (timelapses.Count > 0 && y2.Count > 0)
                {
                    var xs2 = timelapses.ToArray();
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
                formsPlot2.Refresh();
            }
            else
            {
                formsPlot2.Visible = false;
            }

            // Update all maximized plots
            if (maximizedForms != null)
            {
                foreach (var form in maximizedForms.ToList())
                {
                    if (!form.IsDisposed)
                    {
                        var plotNumber = (int)form.Tag;
                        var plot = form.Controls.OfType<FormsPlot>().FirstOrDefault();
                        if (plot != null)
                        {
                            UpdateMaximizedPlot(plot, plotNumber);
                        }
                    }
                }
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
            plot.Plot.Clear();
            string param;
            List<double> y;
            if (plotNumber == 1)
            {
                param = paramComboBox1.SelectedItem?.ToString() ?? "Voltage";
                y = param == "Voltage" ? voltages : param == "TDS" ? tdsValues : tempValues;
            }
            else
            {
                param = paramComboBox2.SelectedItem?.ToString() ?? "TDS";
                y = param == "Voltage" ? voltages : param == "TDS" ? tdsValues : tempValues;
            }
            if (timelapses.Count > 0 && y.Count > 0)
            {
                var xs = timelapses.ToArray();
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
                            ws.Cell(1, 3).Value = "Voltage";
                            ws.Cell(1, 4).Value = "TDS";
                            ws.Cell(1, 5).Value = "Temperature";
                            ws.Cell(1, 6).Value = "Time (s)";

                            // Write data
                            for (int i = 0; i < timelapses.Count; i++)
                            {
                                ws.Cell(i + 2, 1).Value = i + 1; // S.no
                                ws.Cell(i + 2, 2).Value = startTime.HasValue ? startTime.Value.AddSeconds(timelapses[i]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                                ws.Cell(i + 2, 3).Value = voltages.Count > i ? voltages[i] : "";
                                ws.Cell(i + 2, 4).Value = tdsValues.Count > i ? tdsValues[i] : "";
                                ws.Cell(i + 2, 5).Value = tempValues.Count > i ? tempValues[i] : "";
                                ws.Cell(i + 2, 6).Value = timelapses[i];
                            }

                            // Format headers
                            var headerRange = ws.Range(1, 1, 1, 6);
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                            // Format data
                            var dataRange = ws.Range(2, 1, timelapses.Count + 1, 6);
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
    }
} 
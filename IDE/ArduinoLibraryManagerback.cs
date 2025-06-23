using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UniFlash.IDE
{
    public class LibraryInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Sentence { get; set; }
        public string Paragraph { get; set; }
        public string Website { get; set; }
        public bool IsInstalled { get; set; }
        public string InstalledVersion { get; set; }
    }

    public class ArduinoLibraryManager
    {
        private string arduinoDataPath;
        private RichTextBox outputBox;

        public ArduinoLibraryManager(RichTextBox outputBox)
        {
            this.outputBox = outputBox;
            this.arduinoDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Arduino15"
            );
        }

        public async Task<List<LibraryInfo>> SearchLibraries(string query)
        {
            var result = await RunCliCommand($"arduino-cli lib search {query}");
            return ParseSearchResults(result);
        }

        public async Task<List<LibraryInfo>> ListInstalledLibraries()
        {
            var result = await RunCliCommand("arduino-cli lib list");
            return ParseInstalledResults(result);
        }

        public async Task<bool> RemoveLibrary(string libraryName)
        {
            var result = await RunCliCommand($"arduino-cli lib uninstall {libraryName}");
            outputBox.AppendText(result);
            return !result.ToLower().Contains("error");
        }

        public async Task<bool> UpdateLibrary(string libraryName)
        {
            var result = await RunCliCommand($"arduino-cli lib update {libraryName}");
            outputBox.AppendText(result);
            return !result.ToLower().Contains("error");
        }

        public async Task<LibraryInfo> GetLibraryDetails(string libraryName)
        {
            var search = await SearchLibraries(libraryName);
            return search.Find(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> InstallLibrary(string libraryName, string version = "latest")
        {
            try
            {
                // Skip installation for built-in libraries
                if (IsBuiltInLibrary(libraryName))
                {
                    outputBox.AppendText($"[Library '{libraryName}' is built into Arduino core, skipping installation]\n");
                    return true;
                }

                // 1. First check if library exists in Arduino's library manager
                outputBox.AppendText($"[Checking library: {libraryName}]\n");
                var searchResult = await RunCliCommand($"arduino-cli lib search \"{libraryName}\"");
                
                // Extract exact library name from search results
                var exactName = ExtractExactLibraryName(searchResult, libraryName);
                if (string.IsNullOrEmpty(exactName))
                {
                    // Try installing with the original name as a fallback
                    outputBox.AppendText($"[Trying direct installation of: {libraryName}]\n");
                    var directInstallResult = await RunCliCommand($"arduino-cli lib install \"{libraryName}\"");
                    if (!directInstallResult.Contains("Error"))
                    {
                        outputBox.AppendText($"[Successfully installed {libraryName}]\n");
                        return true;
                    }
                    outputBox.AppendText($"[Error: Library '{libraryName}' not found in Arduino Library Manager]\n");
                    return false;
                }

                // 2. Check if library is already installed
                var listResult = await RunCliCommand("arduino-cli lib list");
                if (listResult.Contains(exactName))
                {
                    outputBox.AppendText($"[Library '{exactName}' is already installed]\n");
                    return true;
                }

                // 3. Install the library using exact name
                outputBox.AppendText($"[Installing library: {exactName}]\n");
                string installCmd = (version == "latest" || string.IsNullOrWhiteSpace(version))
                    ? $"arduino-cli lib install \"{exactName}\""
                    : $"arduino-cli lib install \"{exactName}@{version}\"";
                var installResult = await RunCliCommand(installCmd);
                
                if (installResult.Contains("Error"))
                {
                    outputBox.AppendText($"[Error installing {exactName}: {installResult}]\n");
                    return false;
                }

                outputBox.AppendText($"[Successfully installed {exactName}]\n");
                return true;
            }
            catch (Exception ex)
            {
                outputBox.AppendText($"[Error: {ex.Message}]\n");
                return false;
            }
        }

        private string ExtractExactLibraryName(string searchResult, string searchedName)
        {
            try
            {
                var lines = searchResult.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Name:"))
                    {
                        // Extract name between quotes if present, otherwise take the whole string after "Name:"
                        var name = line.Substring(5).Trim();
                        if (name.StartsWith("\"") && name.EndsWith("\""))
                        {
                            name = name.Substring(1, name.Length - 2);
                        }
                        
                        // First try exact match
                        if (name.Equals(searchedName, StringComparison.OrdinalIgnoreCase))
                        {
                            return name;
                        }
                        
                        // Then try contains match
                        if (name.Contains(searchedName, StringComparison.OrdinalIgnoreCase))
                        {
                            return name;
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> EnsureMegaCoreXInstalled(Action<string>? progressCallback = null)
        {
            try
            {
                progressCallback?.Invoke("[Setting up MegaCoreX for ATmega4809...]");
                
                // First, check if MegaCoreX is already installed
                progressCallback?.Invoke("[Checking if MegaCoreX is already installed...]");
                var checkResult = await RunCliCommand("arduino-cli core list");
                if (checkResult.Contains("MegaCoreX:megaavr"))
                {
                    progressCallback?.Invoke("[MegaCoreX is already installed]");
                    return true;
                }

                // Add MegaCoreX board manager URL
                progressCallback?.Invoke("[Adding MegaCoreX board manager URL...]");
                var addUrlResult = await RunCliCommand("arduino-cli config add board_manager.additional_urls https://mcudude.github.io/MegaCoreX/package_MCUdude_MegaCoreX_index.json");
                progressCallback?.Invoke($"[Config add output: {addUrlResult.Trim()}]");
                
                // Read and log the config file contents
                string configPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arduino15", "arduino-cli.yaml");
                if (System.IO.File.Exists(configPath))
                {
                    progressCallback?.Invoke($"[arduino-cli.yaml contents:]");
                    foreach (var line in System.IO.File.ReadAllLines(configPath))
                        progressCallback?.Invoke(line);
                }
                else
                {
                    progressCallback?.Invoke($"[arduino-cli.yaml not found at {configPath}]");
                }
                
                if (addUrlResult.Contains("Error"))
                {
                    progressCallback?.Invoke($"[Error adding board manager URL: {addUrlResult}]");
                    return false;
                }
                
                // Update board index
                progressCallback?.Invoke("[Updating board index...]");
                var updateResult = await RunCliCommand("arduino-cli core update-index");
                if (updateResult.Contains("Error"))
                {
                    progressCallback?.Invoke($"[Error updating board index: {updateResult}]");
                    return false;
                }
                
                // Install MegaCoreX core (correct core name)
                progressCallback?.Invoke("[Installing MegaCoreX core...]");
                var installResult = await RunCliCommand("arduino-cli core install MegaCoreX:megaavr");
                if (installResult.Contains("Error"))
                {
                    progressCallback?.Invoke($"[Error installing MegaCoreX core: {installResult}]");
                    return false;
                }
                
                progressCallback?.Invoke("[MegaCoreX installation completed successfully]");
                return true;
            }
            catch (Exception ex)
            {
                progressCallback?.Invoke($"[Error installing MegaCoreX: {ex.Message}]");
                return false;
            }
        }

        public async Task<bool> InstallMegaCoreX()
        {
            return await EnsureMegaCoreXInstalled(msg => outputBox.AppendText(msg + "\n"));
        }

        public async Task<bool> InstallMegaCoreXWithProgress(Action<string> progressCallback)
        {
            return await EnsureMegaCoreXInstalled(progressCallback);
        }

        private bool IsBuiltInLibrary(string libraryName)
        {
            // List of libraries that are built into the Arduino core
            string[] builtInLibraries = new[]
            {
                "SoftwareSerial",
                "Wire",
                "SPI",
                "EEPROM",
                "Servo",
                "Stepper",
                "LiquidCrystal",
                "SD",
                "Firmata",
                "GSM",
                "TFT",
                "WiFi",
                "Ethernet",
                "Keyboard",
                "Mouse",
                // Add common Arduino functions that are built-in
                "io",
                "delay",
                "digitalWrite",
                "digitalRead",
                "analogWrite",
                "analogRead",
                "pinMode",
                "Serial",
                "millis",
                "micros",
                "delayMicroseconds",
                "tone",
                "noTone",
                "attachInterrupt",
                "detachInterrupt",
                "interrupts",
                "noInterrupts",
                "random",
                "randomSeed",
                "map",
                "constrain",
                "min",
                "max",
                "abs",
                "pow",
                "sqrt",
                "sin",
                "cos",
                "tan",
                "isnan",
                "isinf"
            };

            return builtInLibraries.Contains(libraryName, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> InstallLocalLibrary(string libraryPath)
        {
            try
            {
                if (!Directory.Exists(libraryPath))
                {
                    outputBox.AppendText($"[Error: Library path not found: {libraryPath}]\n");
                    return false;
                }

                // Copy library to Arduino libraries folder
                string arduinoLibrariesPath = Path.Combine(arduinoDataPath, "libraries");
                string libraryName = Path.GetFileName(libraryPath);
                string targetPath = Path.Combine(arduinoLibrariesPath, libraryName);

                if (Directory.Exists(targetPath))
                {
                    outputBox.AppendText($"[Library already exists, updating...]\n");
                    Directory.Delete(targetPath, true);
                }

                Directory.CreateDirectory(targetPath);
                CopyDirectory(libraryPath, targetPath);
                outputBox.AppendText($"[Successfully installed local library: {libraryName}]\n");
                return true;
            }
            catch (Exception ex)
            {
                outputBox.AppendText($"[Error installing local library: {ex.Message}]\n");
                return false;
            }
        }

        private void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var file in Directory.GetFiles(source))
            {
                string destFile = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                string destDir = Path.Combine(target, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        public async Task<string> RunCliCommand(string command)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        return output + error;
                    }
                }
                catch (Exception ex)
                {
                    return $"[CLI Error: {ex.Message}]\n";
                }
            });
        }

        private List<LibraryInfo> ParseSearchResults(string cliOutput)
        {
            var list = new List<LibraryInfo>();
            LibraryInfo current = null;
            var lines = cliOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("Name:"))
                {
                    if (current != null)
                        list.Add(current);
                    current = new LibraryInfo();
                    // Extract name (remove quotes if present)
                    var name = line.Substring(5).Trim().Trim('"');
                    current.Name = name;
                }
                else if (current != null && line.TrimStart().StartsWith("Author:"))
                {
                    var author = line.Split(new[] { ':' }, 2)[1].Trim();
                    current.Author = author;
                }
                else if (current != null && line.TrimStart().StartsWith("Sentence:"))
                {
                    var sentence = line.Split(new[] { ':' }, 2)[1].Trim();
                    current.Sentence = sentence;
                }
            }
            if (current != null)
                list.Add(current);
            return list;
        }

        private List<LibraryInfo> ParseInstalledResults(string cliOutput)
        {
            var list = new List<LibraryInfo>();
            var lines = cliOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("Name") || line.StartsWith("--") || string.IsNullOrWhiteSpace(line))
                    continue;
                var parts = Regex.Split(line, @"\s{2,}");
                if (parts.Length >= 2)
                {
                    list.Add(new LibraryInfo
                    {
                        Name = parts[0].Trim(),
                        InstalledVersion = parts[1].Trim(),
                        IsInstalled = true
                    });
                }
            }
            return list;
        }
    }

    // --- Arduino Library Index Manager ---
    public class LibraryIndexManager
    {
        public class ArduinoLibrary
        {
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("version")] public string Version { get; set; }
            [JsonPropertyName("author")] public string Author { get; set; }
            [JsonPropertyName("sentence")] public string Sentence { get; set; }
            [JsonPropertyName("paragraph")] public string Paragraph { get; set; }
            [JsonPropertyName("website")] public string Website { get; set; }
            [JsonPropertyName("architectures")] public string[] Architectures { get; set; }
        }

        public class LibraryIndex
        {
            [JsonPropertyName("libraries")] public List<ArduinoLibrary> Libraries { get; set; }
        }

        public List<ArduinoLibrary> Libraries { get; private set; } = new();
        public bool IsLoaded { get; private set; } = false;

        private const string IndexUrl = "https://downloads.arduino.cc/libraries/library_index.json";
        private readonly string localIndexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library_index.json");

        public async Task<bool> LoadAsync()
        {
            // Try to download the index
            try
            {
                using var http = new HttpClient();
                var json = await http.GetStringAsync(IndexUrl);
                var index = JsonSerializer.Deserialize<LibraryIndex>(json);
                if (index?.Libraries != null)
                {
                    Libraries = index.Libraries;
                    IsLoaded = true;
                    // Save a local copy for offline use
                    File.WriteAllText(localIndexPath, json);
                    return true;
                }
            }
            catch { }
            // Fallback to local file
            return LoadFromLocal();
        }

        public bool LoadFromLocal()
        {
            try
            {
                if (File.Exists(localIndexPath))
                {
                    var json = File.ReadAllText(localIndexPath);
                    var index = JsonSerializer.Deserialize<LibraryIndex>(json);
                    if (index?.Libraries != null)
                    {
                        Libraries = index.Libraries;
                        IsLoaded = true;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public List<ArduinoLibrary> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Libraries;
            query = query.ToLowerInvariant();
            return Libraries.FindAll(lib =>
                (lib.Name != null && lib.Name.ToLowerInvariant().Contains(query)) ||
                (lib.Author != null && lib.Author.ToLowerInvariant().Contains(query)) ||
                (lib.Sentence != null && lib.Sentence.ToLowerInvariant().Contains(query)) ||
                (lib.Paragraph != null && lib.Paragraph.ToLowerInvariant().Contains(query))
            );
        }
    }
} 
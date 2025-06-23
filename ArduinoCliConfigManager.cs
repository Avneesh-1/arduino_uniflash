using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace UniFlash
{
    public class ArduinoCliConfigManager
    {
        private readonly string configPath;
        private YamlStream yaml;
        private YamlMappingNode root;

        public ArduinoCliConfigManager()
        {
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arduino15", "arduino-cli.yaml");
            if (!File.Exists(configPath))
            {
                var psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c arduino-cli config init")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            Load();
        }

        public void Load()
        {
            yaml = new YamlStream();
            using (var reader = new StreamReader(configPath))
            {
                yaml.Load(reader);
                root = (YamlMappingNode)yaml.Documents[0].RootNode;
            }
        }

        public string[] GetBoardsManagerUrls()
        {
            if (root.Children.TryGetValue("board_manager", out var bmNode) && bmNode is YamlMappingNode bmMap)
            {
                if (bmMap.Children.TryGetValue("additional_urls", out var urlsNode) && urlsNode is YamlSequenceNode urlsSeq)
                {
                    return urlsSeq.Children.Select(u => u.ToString().Trim('"')).ToArray();
                }
            }
            return Array.Empty<string>();
        }

        public void SetBoardsManagerUrls(string[] urls)
        {
            if (!root.Children.TryGetValue("board_manager", out var bmNode) || !(bmNode is YamlMappingNode bmMap))
            {
                bmMap = new YamlMappingNode();
                root.Add("board_manager", bmMap);
            }
            var seq = new YamlSequenceNode(urls.Select(u => new YamlScalarNode(u)));
            bmMap.Children["additional_urls"] = seq;
        }

        public void Save()
        {
            using (var writer = new StreamWriter(configPath))
            {
                yaml.Save(writer, false);
            }
        }
    }
} 
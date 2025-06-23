using System.Diagnostics;
using System.Threading.Tasks;

namespace UniFlash
{
    public class ArduinoCliService
    {
        public static async Task<string> RunCommandAsync(string command)
        {
            return await Task.Run(() =>
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
            });
        }
    }
} 
using System.Diagnostics;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to run commands locally on the same machine as the game servers
    /// </summary>
    public class ProcessCommandExecutioner : ICommandExecutioner
    {
        private readonly Logger<ProcessCommandExecutioner> _logger;

        public ProcessCommandExecutioner()
        {
            _logger = new();
        }

        public Result Execute(string command)
            => Execute(command, []);

        public Result Execute(string command, string[] args)
        {
            ProcessStartInfo runningInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process? process = Process.Start(runningInfo);

            if (process is null)
            {
                _logger.LogError("Process failed to start");
                return new Result(CommandStatus.Error, "Process failed to start");
            }

            process.WaitForExit();

            int exitCode = process.ExitCode;
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            // Specific to the versionCheck script
            // exitCode will be 1, stderr will have the new version number and stdout will be empty
            if ((exitCode == 1) && (stderr != string.Empty) && (stdout == string.Empty))
                return new Result(stdout);

            // Exit code 0 and no error probably means success
            if (exitCode == 0 && stderr == string.Empty)
                return new Result(stdout);

            // Default to error
            return new Result(CommandStatus.Error, stderr);
        }
    }
}

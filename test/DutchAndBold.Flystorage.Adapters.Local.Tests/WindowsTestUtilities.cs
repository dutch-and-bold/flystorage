using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DutchAndBold.Flystorage.Adapters.Local.Tests
{
    public static class WindowsTestUtilities
    {
        [SupportedOSPlatform("windows")]
        public static void CreateSymbolicLink(string linkName, string sourcePath)
        {
            var psi= new ProcessStartInfo("cmd.exe",$" /C mklink \"{linkName}\" \"{sourcePath}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

            Process.Start(psi)?.WaitForExit();
        }
    }
}
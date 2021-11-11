using UnityEngine;
using System.Diagnostics;

namespace com.tencent.pandora.tools
{
    public class ImageChannelSpliterWrapper
    {
        public static void Execute(string sourcePath)
        {
            sourcePath = string.Concat(Application.dataPath.Replace("/Assets", "/"), sourcePath);
            string toolPath = string.Concat(Application.dataPath, "/Tool");
            string batPath = toolPath + "/ImageChannelSpliter.bat";
            string rgbPath = sourcePath.Replace(".png", "_rgb.png");
            string alphaPath = sourcePath.Replace(".png", "_alpha.png");
            Process process = new Process();
            string paramContnet = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", sourcePath, rgbPath, alphaPath, toolPath);
            ProcessStartInfo info = new ProcessStartInfo(batPath, paramContnet);
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.StartInfo = info;
            process.Start();
            process.WaitForExit();
        }
    }
}


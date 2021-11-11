using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

/// <summary>
/// 在macOS上启动Unity时为mac平台的编译工具添加可执行权限
/// </summary>
namespace com.tencent.pandora.tools
{
    [InitializeOnLoad]
    public class OSXToolsPreprocessor
    {
        static string SHELL_PATH = "/bin/sh";

        /// <summary>
        /// 调用shell执行脚本
        /// </summary>
        /// <param name="workingDirectory">工作目录</param>
        /// <param name="args">传递给shell的参数</param>
        /// <param name="output">shell输出</param>
        /// <returns>shell返回值</returns>
        private static int CallShell(string workingDirectory, string args, out string output)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = SHELL_PATH;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.CreateNoWindow = true;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.WorkingDirectory = workingDirectory;
            processInfo.Arguments = args;

            Process process = Process.Start(processInfo);
            output = process.StandardOutput.ReadToEnd();
            output = output.Trim();
            process.WaitForExit();
            int ret = process.ExitCode;

            return ret;
        }

        static OSXToolsPreprocessor()
        {
#if UNITY_EDITOR_OSX
            Main();
#endif
        }

        public static void Main()
        {
            string scriptPath = Path.Combine(Application.dataPath, "Tool/chmod_OSX.sh");
            string workingDirectory = Path.Combine(Application.dataPath, "Tool/");
            try
            {
                string output;
                int ret = CallShell(workingDirectory, scriptPath, out output);

                if (ret != 0)
                {
                    string message = string.Format("脚本 {0} 执行错误，请检查！  {1}", scriptPath, output);
                    UnityEngine.Debug.LogError(message);
                    return;
                }
                if (output != "")
                {
                    string message = string.Format("<color=#D00FDBFF>下列文件已添加可执行权限，请提交变更到版本控制工具: {0}</color>", output);
                    UnityEngine.Debug.Log(message);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }
    }
}

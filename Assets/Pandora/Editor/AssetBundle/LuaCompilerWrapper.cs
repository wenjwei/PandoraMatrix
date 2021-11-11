using UnityEditor;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.tencent.pandora.tools
{
    public class LuaCompilerWrapper
    {
#if UNITY_EDITOR_OSX
        private static string COMPILER_32 = Application.dataPath + "/Tool/sluac_OSX_32";
        private static string COMPILER_64 = Application.dataPath + "/Tool/sluac_OSX_64";
		private static string ENCRYPTOR_PATH = Application.dataPath + "/Tool/xxtea";
#else
        private static string COMPILER_32 = Application.dataPath + "/Tool/sluac_32.exe";
        private static string COMPILER_64 = Application.dataPath + "/Tool/sluac_64.exe";
        private static string ENCRYPTOR_PATH = Application.dataPath + "/Tool/xxtea.exe";
#endif

        public static string Error { get; set; }

        public static void Compile(string path)
        {
            CompileAndEncrypt(path, "/Lua32/", COMPILER_32, ENCRYPTOR_PATH);
            CompileAndEncrypt(path, "/Lua64/", COMPILER_64, ENCRYPTOR_PATH);
        }

        private static void CompileAndEncrypt(string luaPath, string folderToken, string compilerPath, string encryptorPath)
        {
            Regex regex = new Regex(@"/Lua[/|\\]");
            string outputPath = regex.Replace(luaPath, folderToken, 1);

            string directoryPath = Path.GetDirectoryName(outputPath);
            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }
            RunCompiler(compilerPath, luaPath, outputPath);
            RunEncryptor(encryptorPath, outputPath, outputPath);
        }

        private static void RunCompiler(string compilerPath, string path, string outputPath)
        {
            RunProcess(compilerPath, string.Format("-o {0} {1}", outputPath, path));
        }

        private static void RunEncryptor(string encryptorPath, string path, string outputPath)
        {
#if UNITY_EDITOR_OSX
            RunProcess(encryptorPath, string.Format("pandora PDCODE {0} {1}", path, outputPath));

#else
            RunProcess(encryptorPath, string.Format("{0} {1}", path, outputPath));
#endif
        }

        private static void RunProcess(string exePath, string args)
        {
            Process process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = exePath;
            process.ErrorDataReceived += HandleError;
            process.StartInfo.Arguments = args;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private static void HandleOutput(object sender, DataReceivedEventArgs e)
        {
            string output = e.Data;
            if (string.IsNullOrEmpty(output) == false)
            {
                UnityEngine.Debug.Log(output);
            }
        }

        private static void HandleError(object sender, DataReceivedEventArgs e)
        {
            string msg = e.Data;
            if (string.IsNullOrEmpty(msg) == false)
            {
                Error += msg + "\n";
            }
        }
    }
}
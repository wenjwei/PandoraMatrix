using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace com.tencent.pandora
{
    internal class AutoConfig
    {
        #region 参数
        //相对路径
        private const string ERROR_CODE_CONFIG_PATH = "Pandora/Scripts/ErrorCodeConfig.cs";
        private const string PANDORA_PATH = "Pandora/Scripts/Pandora.cs";
        private const string PACKAGE_EXPORTER_PATH = "Pandora/Editor/PackageExporter/PackageExporter.cs";

        private const string CONFIG_FILE_PATH = "Pandora/Editor/Autoconfig/config.txt";
        private const string FILE_BACKUP_FOLDER_PATH = "Pandora/Editor/Autoconfig/FileBackup";
        //存储路径
        private static Dictionary<string, string> _fileBackupPathDict = new Dictionary<string, string>();
        #endregion

        #region 配置
        public static void Config(bool hasConfiged, Dictionary<string, System.Object> configDict)
        {

            FillFileBackupPathDict();
            if (hasConfiged)
            {
                RestoreFile();
                ExecuteConfig(configDict);
                return;
            }
            BackupFile();
            bool isSuccess = ExecuteConfig(configDict);
            if (isSuccess == false)
            {
                return;
            }
        }

        private static void FillFileBackupPathDict()
        {
            _fileBackupPathDict.Clear();
            string backupFolderPath = Path.Combine(Application.dataPath, FILE_BACKUP_FOLDER_PATH);
            string errorCodeConfigPath = Path.Combine(Application.dataPath, ERROR_CODE_CONFIG_PATH);
            string errorCodeBackupPath = Path.Combine(backupFolderPath, "ErrorCodeConfig.txt");

            string pandoraPath = Path.Combine(Application.dataPath, PANDORA_PATH);
            string pandoraBackupPath = Path.Combine(backupFolderPath, "Pandora.txt");

            string packageExporterPath = Path.Combine(Application.dataPath, PACKAGE_EXPORTER_PATH);
            string packageExporterBackupPath = Path.Combine(backupFolderPath, "PackageExporter.txt");

            _fileBackupPathDict.Add(errorCodeConfigPath, errorCodeBackupPath);
            _fileBackupPathDict.Add(pandoraPath, pandoraBackupPath);
            _fileBackupPathDict.Add(packageExporterPath, packageExporterBackupPath);

        }

        private static void RestoreFile()
        {
            foreach (var item in _fileBackupPathDict)
            {
                File.Delete(item.Key);
                File.Copy(item.Value, item.Key);
            }
        }

        private static void BackupFile()
        {
            string backupFolderPath = Path.Combine(Application.dataPath, FILE_BACKUP_FOLDER_PATH);
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
            }
            foreach (var item in _fileBackupPathDict)
            {
                File.Copy(item.Key, item.Value, true);
            }

        }

        private static bool ExecuteConfig(Dictionary<string, System.Object> configDict)
        {
            //先记录配置
            WriteConfig(configDict);
            //获取配置
            Dictionary<string, string> _errorCodeConfigDict = configDict["_errorCodeConfigDict"] as Dictionary<string, string>;
            Dictionary<string, string> _pandoraConfigDict = configDict["_pandoraConfigDict"] as Dictionary<string, string>;
            Dictionary<string, string> _otherConfigDict = configDict["_otherConfigDict"] as Dictionary<string, string>;

            SubstituteErrorCodeConfigFile(_errorCodeConfigDict);
            SubstitutePandoraFile(_pandoraConfigDict);
            SubstitutePandoraFileExtend(_otherConfigDict);
            SubstitutePackageExportFile(_otherConfigDict);
            return true;
        }

        #region 读写配置
        private static void WriteConfig(Dictionary<string, System.Object> configDict)
        {
            Dictionary<string, string> configStateDict = configDict["_configStateDict"] as Dictionary<string, string>;
            configStateDict["hasConfiged"] = "true";
            string configJson = MiniJSON.Json.Serialize(configDict);
            string configPath = Path.Combine(Application.dataPath, CONFIG_FILE_PATH);
            File.WriteAllText(configPath, configJson);
            AssetDatabase.Refresh();
        }

        #endregion

        #region 替换函数
        static void SubstituteErrorCodeConfigFile(Dictionary<string, string> errorCodeConfigDict)
        {
            //tnm2 配置
            EOL endOfLine = GetEndLineType(ERROR_CODE_CONFIG_PATH);
            string iosPrefix = @"#if\s*UNITY_IOS\s*" + GetEscapedEndLine(endOfLine) + @"\s*ErrorCode\.";
            string prefixPattern = string.Empty;
            string fixedPattern = @"\s*=\s*\d+";
            string value = string.Empty;
            string pattern = string.Empty;
            string targetContent = string.Empty;
            Dictionary<string, string> patternAndTargetContentDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in errorCodeConfigDict)
            {
                value = kv.Value.Trim();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                prefixPattern = kv.Key;
                if (!prefixPattern.Contains("_IOS"))
                {
                    pattern = prefixPattern + fixedPattern;
                    targetContent = prefixPattern + " = " + value;
                }
                else
                {
                    prefixPattern = prefixPattern.Replace("_IOS", "");
                    pattern = iosPrefix + prefixPattern + fixedPattern;
                    targetContent = "#if UNITY_IOS" + GetNotEscapedEndLine(endOfLine) + "\t\t\tErrorCode." + prefixPattern + " = " + value;
                }

                if (patternAndTargetContentDict.ContainsKey(pattern))
                {
                    patternAndTargetContentDict.Remove(pattern);
                }
                patternAndTargetContentDict.Add(pattern, targetContent);
            }

            RegexReplace(patternAndTargetContentDict, ERROR_CODE_CONFIG_PATH);
        }

        //假设一个文件中换行符一致
        static EOL GetEndLineType(string relativePath)
        {
            string filePath = Path.Combine(Application.dataPath, relativePath);
            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.LogError(filePath + "不存在");
                return EOL.Native;
            }
            string context = File.ReadAllText(filePath);

            if (Regex.IsMatch(context, @"\r\n"))
            {
                return EOL.CRLF;
            }

            if (Regex.IsMatch(context, @"\r"))
            {
                return EOL.CR;
            }

            if (Regex.IsMatch(context, @"\n"))
            {
                return EOL.LF;
            }

            return EOL.Native;
        }

        static string GetEscapedEndLine(EOL eol)
        {
            string endLine = System.Environment.NewLine;
            switch (eol)
            {
                case EOL.CRLF:
                    endLine = @"\r\n";
                    break;
                case EOL.CR:
                    endLine = @"\r";
                    break;
                case EOL.LF:
                    endLine = @"\n";
                    break;
                default:
                    break;
            }
            return endLine;
        }

        static string GetNotEscapedEndLine(EOL eol)
        {
            string endLine = System.Environment.NewLine;
            switch (eol)
            {
                case EOL.CRLF:
                    endLine = "\r\n";
                    break;
                case EOL.CR:
                    endLine = "\r";
                    break;
                case EOL.LF:
                    endLine = "\n";
                    break;
                default:
                    break;
            }
            return endLine;
        }

        static void SubstitutePandoraFile(Dictionary<string, string> pandoraConfigDict)
        {
            // Pandora.cs配置
            string pattern = "";
            string targetContent = "";
            string value = string.Empty;
            Dictionary<string, string> patternAndTargetContentDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in pandoraConfigDict)
            {
                value = kv.Value.Trim();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                if (kv.Key == "Font/")
                {
                    if (value.Substring(value.Length - 1) != "/")
                    {
                        value = value + "/";
                    }
                }

                pattern = kv.Key;
                targetContent = value;
                if (kv.Key == "UI")
                {
                    //注意双引号的匹配
                    pattern = @"""UI""";
                    targetContent = "\"" + value + "\"";
                }

                if (patternAndTargetContentDict.ContainsKey(pattern))
                {
                    patternAndTargetContentDict.Remove(pattern);
                }
                patternAndTargetContentDict.Add(pattern, targetContent);
            }

            RegexReplace(patternAndTargetContentDict, PANDORA_PATH);
        }

        static void SubstitutePandoraFileExtend(Dictionary<string, string> otherConfigDict)
        {
            string pattern = "SPEEDM";
            string targetContent = otherConfigDict["GameName"].Trim();
            Dictionary<string, string> patternAndTargetContentDict = new Dictionary<string, string>();
            //根据游戏简称，修改sdk版本号
            if (string.IsNullOrEmpty(targetContent))
            {
                return;
            }
            if (patternAndTargetContentDict.ContainsKey(pattern))
            {
                patternAndTargetContentDict.Remove(pattern);
            }
            patternAndTargetContentDict.Add(pattern, targetContent);
            RegexReplace(patternAndTargetContentDict, PANDORA_PATH);
        }

        static void SubstitutePackageExportFile(Dictionary<string, string> otherConfigDict)
        {
            // PackageExporter.cs配置
            string pattern;
            string targetContent;
            Dictionary<string, string> patternAndTargetContentDict = new Dictionary<string, string>();
            //替换包名
            pattern = "{bizCode}";
            targetContent = otherConfigDict["GameName"].Trim();
            if (string.IsNullOrEmpty(targetContent))
            {
                return;
            }
            if (patternAndTargetContentDict.ContainsKey(pattern))
            {
                patternAndTargetContentDict.Remove(pattern);
            }
            patternAndTargetContentDict.Add(pattern, targetContent);
            RegexReplace(patternAndTargetContentDict, PACKAGE_EXPORTER_PATH);
        }

        public static void RegexReplace(Dictionary<string, string> dict, string relativePath)
        {
            string filePath = Path.Combine(Application.dataPath, relativePath);
            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.LogError(filePath + "不存在，无法进行内容替换");
                return;
            }
            string fileContent = File.ReadAllText(filePath);
            foreach (var item in dict)
            {
                //替换后会返回一个新字符串，原字符串是不变的，一定要赋下新值
                if (!fileContent.Contains(item.Value))
                {
                    fileContent = Regex.Replace(fileContent, item.Key, item.Value);
                }
            }
            File.WriteAllText(filePath, fileContent);
        }
        #endregion


        public static void DisplayWarningDialog(string message, string title = "")
        {
            EditorUtility.DisplayDialog(title, message, "我知道了");
        }

        #endregion
    }
}

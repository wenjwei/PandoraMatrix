using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using com.tencent.pandora;

namespace com.tencent.pandora.tools
{
    public enum CopyDirection
    {
        DecodeToEncode,
        EncodeToDecode,
    }
    public class LuaProcessor
    {
        //标识是否需要预处理和后处理lua文件
        private static bool _needProcess = false;
        //private static bool _needProcess = true;
        private static Dictionary<string, string> _cookieNameAndLuaPathDict = new Dictionary<string, string>()
        {
            { "PandoraToolBoxFunctionConfig.txt", "Actions/Resources/PandoraToolBox/Lua/PandoraToolBoxFunctionConfig.lua.bytes" },
            { "PandoraToolBoxProtocolConfig.txt", "Actions/Resources/PandoraToolBox/Lua/PandoraToolBoxProtocolConfig.lua.bytes" },
            { "PandoraToolBoxVendorConfig.txt", "Actions/Resources/PandoraToolBox/Lua/PandoraToolBoxVendorConfig.lua.bytes" },
        };

        //需要加解密的lua文件
        private static string[] _luaFilePath = new string[] { "Actions/Resources/PandoraToolBox/Lua/PandoraToolBoxReflection.lua.bytes", };

        private static int _id = int.MaxValue;
        //key：自定义值    value：要被替换的JSON格式的字符串
        private static Dictionary<string, string> _substituteDict = new Dictionary<string, string>();

        private static void Json2LuaTable()
        {
            foreach (var item in _cookieNameAndLuaPathDict)
            {
                string jsonConfig = CookieHelper.Read(item.Key);
                if (string.IsNullOrEmpty(jsonConfig))
                {
                    return;
                }

                string luaTableConfig = jsonConfig;
                //先把JSON格式的字符串提取出来，将外部JSON结构转换好后，再填充回去。
                //使用"{}"来匹配提取
                Regex jsonValue2CustomValueRegex = new Regex("\"\\{.+?\\}\"", RegexOptions.None);
                luaTableConfig = jsonValue2CustomValueRegex.Replace(luaTableConfig, new MatchEvaluator(JsonValue2CustomValue));

                //将[] 替换为{}
                luaTableConfig = Regex.Replace(luaTableConfig, @"\[", "{");
                luaTableConfig = Regex.Replace(luaTableConfig, @"\]", "}");

                //将JSON形式的key替换为Lua Table形式的  "key": -> ["key"] =  
                Regex keyRegex = new Regex("\"[^\"]+?\":", RegexOptions.None);
                luaTableConfig = keyRegex.Replace(luaTableConfig, new MatchEvaluator(ConvertKey));

                luaTableConfig = FormatLuaTable(luaTableConfig);

                //还原JSON格式的字符串
                foreach (var kv in _substituteDict)
                {
                    luaTableConfig = Regex.Replace(luaTableConfig, kv.Key, kv.Value);
                }

                //去掉原JSON中 "\/" 的转义字符"\"
                Regex escapeRegex = new Regex(@"\\/", RegexOptions.None);
                luaTableConfig = escapeRegex.Replace(luaTableConfig, "/");

                _substituteDict.Clear();
                _id = int.MaxValue;

                string content = "PandoraToolBoxConfig" + luaTableConfig;
                string configPath = Path.Combine(Application.dataPath, item.Value);
                File.WriteAllText(configPath, content);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        //将 JSON 格式的键值替换为Lua table格式的
        private static string ConvertKey(Match match)
        {
            string capture = match.ToString();
            return string.Format("[{0}] = ", capture.Substring(0, capture.LastIndexOf("\"") + 1));
        }
        //先将json格式字符串替换为自定义内容
        private static string JsonValue2CustomValue(Match match)
        {
            string capture = match.ToString();
            string key = GenSubstituteValue();
            _substituteDict.Add(key, capture);
            return key;
        }

        private static string GenSubstituteValue()
        {
            return string.Format("sub_{0}", _id--);
        }

        private static string FormatLuaTable(string original)
        {
            StringBuilder sb = new StringBuilder(original.Length + 100);
            int indentedNum = 0;
            for (int i = 0; i < original.Length; i++)
            {
                //处理左{
                if (original[i] == '{')
                {
                    indentedNum++;
                }

                //处理左[
                if (original[i] == '[')
                {
                    sb.Append('\n');
                    sb.Append(new string('\t', indentedNum));
                }

                //处理右}
                if (original[i] == '}')
                {
                    indentedNum--;
                    sb.Append('\n');
                    sb.Append(new string('\t', indentedNum));
                }

                sb.Append(original[i]);
            }
            return sb.ToString();
        }

        public static void PreProcessLuaFile()
        {
            Json2LuaTable();
            if (!_needProcess)
            {
                return;
            }
            CopyLuaFile(CopyDirection.EncodeToDecode);
            DecodeLuaFile();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public static void PostProcessLuaFile()
        {
            if (!_needProcess)
            {
                return;
            }
            DeleteLuaFile();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void CopyLuaFile(CopyDirection direction)
        {
            string sourcePath = string.Empty;
            string destPath = string.Empty;
            for (int i = 0; i < _luaFilePath.Length; i++)
            {
                sourcePath = Path.Combine(Application.dataPath, _luaFilePath[i]);
                destPath = sourcePath.Replace("Lua", "EncodedFile");
                if (direction == CopyDirection.EncodeToDecode)
                {
                    string middleStr = sourcePath;
                    sourcePath = destPath;
                    destPath = middleStr;
                }

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
            }
        }

        private static void DeleteLuaFile()
        {
            string absoluteLuaFilePath = string.Empty;
            for (int i = 0; i < _luaFilePath.Length; i++)
            {
                absoluteLuaFilePath = Path.Combine(Application.dataPath, _luaFilePath[i]);
                File.Delete(absoluteLuaFilePath);
            }
        }

        //简单加密文件  base64
        private static void EncodeLuaFile()
        {
            string absoluteLuaFilePath = string.Empty;
            for (int i = 0; i < _luaFilePath.Length; i++)
            {
                absoluteLuaFilePath = Path.Combine(Application.dataPath, _luaFilePath[i]);
                if (File.Exists(absoluteLuaFilePath))
                {
                    string encodedLuaFile = Convert.ToBase64String(File.ReadAllBytes(absoluteLuaFilePath));
                    File.WriteAllText(absoluteLuaFilePath, encodedLuaFile);
                }

            }
        }

        private static void DecodeLuaFile()
        {
            string absoluteLuaFilePath = string.Empty;
            for (int i = 0; i < _luaFilePath.Length; i++)
            {
                absoluteLuaFilePath = Path.Combine(Application.dataPath, _luaFilePath[i]);
                if (File.Exists(absoluteLuaFilePath))
                {
                    byte[] decodedLuaFile = Convert.FromBase64String(File.ReadAllText(absoluteLuaFilePath));
                    File.WriteAllBytes(absoluteLuaFilePath, decodedLuaFile);
                }

            }
        }


    }
}

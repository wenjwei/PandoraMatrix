using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace com.tencent.pandora.tools
{
    public class DecryptionTool
    {
        [MenuItem("PandoraTools/日志解密")]
        public static void Decrpt()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("选择加密日志文件", LocalDirectoryHelper.GetLogFolderPath(), new string[] { "Log", "txt,log" });
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("没有选取加密日志文件");
                return;
            }

            string folder = Path.GetDirectoryName(path);
            string fileName = "decrypted_" + Path.GetFileName(path);
            string destPath = Path.Combine(folder, fileName);

            StreamReader reader = new StreamReader(path);
            StreamWriter writer = new StreamWriter(destPath);

            string line;
            string decrpted;
            while ((line = reader.ReadLine()) != null)
            {
                decrpted = EncryptionHelper.DecryptDES(line);
                writer.WriteLine(decrpted);
            }
            writer.Flush();
            reader.Close();
            writer.Close();
            Debug.Log(string.Format("解密完成，请去对应路径 {0} 查看", destPath));
        }
    }
}

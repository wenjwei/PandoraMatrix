using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{
    public class FileViewer
    {
        public static string ProductName { get; set; }

        public Folder GenerateDirectoryTree()
        {
            string rootPath = GetRootPath();
            int depth = 0;
            List<Folder> folderList = new List<Folder>();
            FillFolderList(rootPath, depth, folderList);
            return TreeUtility.ListToTree<Folder>(folderList);
        }

        private string GetRootPath()
        {
            string temporaryCachePath = PandoraSettings.GetTemporaryCachePath();
            return Directory.GetParent(temporaryCachePath).ToString();
        }

        private void FillFolderList(string path, int depth, List<Folder> folderList)
        {
            folderList.Add(GetFolder(path, depth));
            string[] subDirectories = null;
            if (depth == 0)
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                subDirectories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
#else
                //在移动平台上这一层用户是无权限使用Directory.GetDirectories获取子文件夹权限，需手动填充
                string persistentPath = Application.persistentDataPath;
                string cachePath = Application.temporaryCachePath;
                subDirectories = new string[2] { persistentPath, cachePath };
#endif
            }

            if (depth > 0)
            {
                try
                {
                    subDirectories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
                }
                catch (Exception e)
                {
                    //对于某些系统生成的文件夹没有权限访问，调用GetDirectory时会抛异常
                    Logger.LogWarning("获取子文件夹失败：" + e.Message);
                }
            }

            if (subDirectories == null)
            {
                return;
            }

            depth++;

            for (int i = 0; i < subDirectories.Length; i++)
            {
                FillFolderList(subDirectories[i], depth, folderList);
            }
        }

        private Folder GetFolder(string path, int depth)
        {
            Folder folder = new Folder();
            folder.Name = GetNameByPath(path);
            if (depth == 0)
            {
                //根节点取固定名
                folder.Name = ProductName;
            }
            folder.Path = path;
            folder.Depth = depth;
            return folder;
        }

        private static string GetNameByPath(string path)
        {
            return path.Substring(path.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, '/' }) + 1);
        }

        public static string GetFileTypeByPath(string path)
        {
            if (path.Contains(".") == false)
            {
                return "文件";
            }

            return path.Substring(path.IndexOf(".") + 1).ToUpper();
        }

        public static string FormatDateTime(DateTime time)
        {
            return time.ToString("MM-dd HH:mm:ss");
        }

        public static List<FileSystemEntry> GetFolderInfoList(string[] pathList, string parentPath)
        {
            List<FileSystemEntry> list = new List<FileSystemEntry>();
            for (int i = 0; i < pathList.Length; i++)
            {
                string path = pathList[i];
                FileSystemEntry fileItem = new FileSystemEntry();
                fileItem.LastWriteTime = Directory.GetCreationTime(path);
                fileItem.Name = GetNameByPath(path);
                fileItem.ParentPath = parentPath;
                fileItem.Path = path;
                fileItem.Type = "文件夹";
                list.Add(fileItem);
            }
            return list;
        }

        public static List<FileSystemEntry> GetFileInfoList(string[] pathList, string parentPath)
        {
            List<FileSystemEntry> list = new List<FileSystemEntry>();
            for (int i = 0; i < pathList.Length; i++)
            {
                string path = pathList[i];
                FileSystemEntry fileItem = new FileSystemEntry();
                FileInfo fi = new FileInfo(path);
                fileItem.Length = fi.Length;
                fileItem.LastWriteTime = fi.LastWriteTime;
                fileItem.Name = GetNameByPath(path);
                fileItem.ParentPath = parentPath;
                fileItem.Path = path;
                fileItem.Type = GetFileTypeByPath(path);
                list.Add(fileItem);
            }
            return list;
        }

        public static void Delete(Folder parentDirectory, FileSystemEntry entry, Action callback)
        {
            if (entry.Type == "文件夹")
            {
                Directory.Delete(entry.Path, true);
                var childen = parentDirectory.Children;
                for (int i = childen.Count - 1; i >= 0; i--)
                {
                    if ((childen[i] as Folder).Path == entry.Path)
                    {
                        childen.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                File.Delete(entry.Path);
            }
            entry = null;
            callback();
        }
    }

    public class Folder : TreeElement
    {
        public string Path { get; set; }
        public bool IsFolded { get; set; }
    }

    public class FileSystemEntry
    {
        public string Name { get; set; }
        public long Length { get; set; }
        //可能的值为：文件夹、文件、文件后缀名
        public string Type { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string ParentPath { get; set; }
        public string Path { get; set; }
        public int Id { get; set; }
    }
}
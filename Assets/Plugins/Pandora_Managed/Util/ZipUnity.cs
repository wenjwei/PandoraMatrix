using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace com.tencent.pandora
{
    public class ZipUnity
    {
        #region ZipCallback
        public abstract class ZipCallback
        {
            /// <summary>
            /// 压缩单个文件或者文件夹前执行的回调
            /// </summary>
            /// <param name="_entry"></param>
            /// <returns>如果返回true，则进行压缩，反之不压缩</returns>
            public virtual bool OnPreZip(ZipEntry _entry)
            {
                return true;
            }
            /// <summary>
            /// 压缩单个文件或者文件夹后执行的回调
            /// </summary>
            /// <param name="_entry"></param>
            public virtual void OnPostZip(ZipEntry _entry) { }
            /// <summary>
            /// 压缩执行完毕后的回调
            /// </summary>
            /// <param name="_result">true表示压缩成功，false表示压缩失败</param>
            public virtual void OnFinished(bool _result) { }
        }
        #endregion

        #region UnZipCallback
        public abstract class UnZipCallback
        {
            /// <summary>
            /// 解压单个文件或者文件夹前执行的回调
            /// </summary>
            /// <param name="_entry"></param>
            /// <returns>如果返回true，则压缩文件或者文件夹，反之不压缩</returns>
            public virtual bool OnPreUnzip(ZipEntry _entry)
            {
                return true;
            }
            /// <summary>
            /// 解压单个文件或者文件夹后执行的回调
            /// </summary>
            /// <param name="_entry"></param>
            public virtual void OnPostUnzip(ZipEntry _entry) { }

            public virtual void OnPostUnzip(ZipEntry _entry, string _fileName, byte[] _fileContent) { }
            /// <summary>
            /// 解压执行完毕后的回调
            /// </summary>
            /// <param name="_result">true表示解压成功，false表示解压失败</param>
            public virtual void OnFinished(bool _result) { }
        }
        #endregion

        /// <summary>
        /// 压缩文件和文件夹
        /// </summary>
        /// <param name="_fileOrDirectory">文件夹路径和文件名</param>
        /// <param name="_outputPath">压缩后的输出路径</param>
        /// <param name="_password">压缩密码</param>
        /// <param name="_zipCallback">负责回调</param>
        /// <returns></returns>
        public bool Zip(string[] _fileOrDirectory, string _outputPath, string _password = null, ZipCallback _zipCallback = null)
        {
            if ((null == _fileOrDirectory) || string.IsNullOrEmpty(_outputPath))
            {
                if (null != _zipCallback)
                    _zipCallback.OnFinished(false);

                return false;
            }

            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_outputPath));
            zipOutputStream.SetLevel(6);
            if (!string.IsNullOrEmpty(_password))
                zipOutputStream.Password = _password;

            for (int i = 0; i < _fileOrDirectory.Length; ++i)
            {
                bool result = false;
                string fileOrDirectory = _fileOrDirectory[i];
                if (Directory.Exists(fileOrDirectory))
                    result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream, _zipCallback);
                else if (File.Exists(fileOrDirectory))
                    result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream, _zipCallback);

                if (!result)
                {
                    if (null != _zipCallback)
                        _zipCallback.OnFinished(false);

                    return false;
                }
            }

            zipOutputStream.Finish();
            zipOutputStream.Close();

            Debug.Log("Zip success!");
            if (null != _zipCallback)
                _zipCallback.OnFinished(true);
            return true;
        }
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="_filename">文件路径名</param>
        /// <param name="_parentPath">压缩文件的父相对文件夹</param>
        /// <param name="_zipOutputStream">压缩输出流</param>
        /// <param name="_zipCallback">负责回调</param>
        /// <returns></returns>
        private bool ZipFile(string _filename, string _parentPath, ZipOutputStream _zipOutputStream, ZipCallback _zipCallback = null)
        {
            ZipEntry entry = null;
            FileStream fileStream = null;
            try
            {
                string entryName = _parentPath + '/' + Path.GetFileName(_filename);
                entry = new ZipEntry(entryName);
                entry.DateTime = System.DateTime.Now;

                if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                    return true;

                fileStream = File.OpenRead(_filename);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();

                entry.Size = buffer.Length;
                _zipOutputStream.PutNextEntry(entry);
                _zipOutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (System.Exception e)
            {
                Debug.LogError("ZipUnity.ZipFile: " + e.ToString());
                return false;
            }
            finally
            {
                if (null != fileStream)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
            if (null != _zipCallback)
                _zipCallback.OnPostZip(entry);

            return true;
        }
        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="_path">要进行压缩的文件夹</param>
        /// <param name="_parentPath">要压缩的文件夹的父相对文件夹</param>
        /// <param name="_zipOutputStream">压缩输出流</param>
        /// <param name="_zipCallback">负责回调</param>
        /// <returns></returns>
        private bool ZipDirectory(string _path, string _parentPath, ZipOutputStream _zipOutputStream, ZipCallback _zipCallback = null)
        {
            ZipEntry entry = null;
            try
            {
                string entryName = Path.Combine(_parentPath, Path.GetFileName(_path) + '/');
                entry = new ZipEntry(entryName);
                entry.DateTime = System.DateTime.Now;
                entry.Size = 0;

                if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                    return true;

                _zipOutputStream.PutNextEntry(entry);
                _zipOutputStream.Flush();

                string[] files = Directory.GetFiles(_path);
                for (int i = 0; i < files.Length; ++i)
                    ZipFile(files[i], Path.Combine(_parentPath, Path.GetFileName(_path)), _zipOutputStream, _zipCallback);
            }
            catch (System.Exception e)
            {
                Debug.LogError("ZipUnity.ZipDirectory: " + e.ToString());
            }
            string[] directory = Directory.GetDirectories(_path);
            for (int i = 0; i < directory.Length; ++i)
            {
                if (!ZipDirectory(directory[i], Path.Combine(_parentPath, Path.GetFileName(_path)), _zipOutputStream, _zipCallback))
                    return false;

            }

            if (null != _zipCallback)
                _zipCallback.OnPostZip(entry);
            return true;
        }
        /// <summary>
        /// 解压Zip包
        /// </summary>
        /// <param name="_filename">Zip包的文件路径名</param>
        /// <param name="_outputPath">解压输出路径</param>
        /// <param name="_password">解压密码</param>
        /// <param name="_unzipCallback">负责回调</param>
        /// <returns></returns>
        public bool UnZipFile(string _filename, string _outputPath, string _password = null, UnZipCallback _unzipCallback = null)
        {
            if (string.IsNullOrEmpty(_filename) || string.IsNullOrEmpty(_outputPath))
            {
                if (null != _unzipCallback)
                    _unzipCallback.OnFinished(false);
                return false;
            }
            try
            {
                Debug.Log("xuetongma111111");
                return UnZipFile(File.OpenRead(_filename), _outputPath, _password, _unzipCallback);
            }
            catch (System.Exception e)
            {
                Debug.LogError("ZipUnity.UnZipFile: " + e.ToString());
                if (null != _unzipCallback)
                    _unzipCallback.OnFinished(false);

                return false;
            }
        }
        /// <summary>
        /// 解压zip包
        /// </summary>
        /// <param name="_filebytes">Zip包字节数据</param>
        /// <param name="_outputPath">解压输出路径</param>
        /// <param name="_password">解压密码</param>
        /// <param name="_unzipCallback">负责回调</param>
        /// <returns></returns>
        public bool UnZipFile(byte[] _filebytes, string _outputPath, string _password = null, UnZipCallback _unzipCallback = null)
        {
            if ((null == _filebytes) || string.IsNullOrEmpty(_outputPath))
            {
                if (null != _unzipCallback)
                    _unzipCallback.OnFinished(false);
                return false;
            }

            bool result = UnZipFile(new MemoryStream(_filebytes), _outputPath, _password, _unzipCallback);
            if (!result)
            {
                if (null != _unzipCallback)
                    _unzipCallback.OnFinished(false);
            }
            return result;
        }
        /// <summary>
        /// 解压zip包
        /// </summary>
        /// <param name="_inputStream">Zip包输入流</param>
        /// <param name="_outputPath">解压输出路径</param>
        /// <param name="_password">解压密码</param>
        /// <param name="_unzipCallback">负责回调</param>
        /// <returns></returns>
        public bool UnZipFile(Stream _inputStream, string _outputPath, string _password = null, UnZipCallback _unzipCallback = null)
        {
            if ((null == _inputStream) || string.IsNullOrEmpty(_outputPath))
            {
                if (null != _unzipCallback)
                    _unzipCallback.OnFinished(false);
                return false;
            }

            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);

            ZipEntry entry = null;
            using (ZipInputStream zipInputStream = new ZipInputStream(_inputStream))
            {
                if (!string.IsNullOrEmpty(_password))
                    zipInputStream.Password = _password;

                while (null != (entry = zipInputStream.GetNextEntry()))
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if ((null != _unzipCallback) && !_unzipCallback.OnPreUnzip(entry))
                        continue;

                    string filePathName = Path.Combine(_outputPath, entry.Name);
                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(filePathName);
                        continue;
                    }
                    //写入文件
                    try
                    {
                        using (FileStream fileStream = File.Create(filePathName))
                        {
                            byte[] bytes = new byte[1024];
                            while (true)
                            {
                                int count = zipInputStream.Read(bytes, 0, bytes.Length);
                                if (count > 0)
                                    fileStream.Write(bytes, 0, count);
                                else
                                {
                                    if (null != _unzipCallback)
                                        _unzipCallback.OnPostUnzip(entry);
                                    break;
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("ZipUnity.UnZipFile: " + e.ToString());
                        if (null != _unzipCallback)
                            _unzipCallback.OnFinished(false);
                        return false;
                    }
                }
            }
            Debug.Log("unzip success!");
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(true);
            return true;

        }

        /// <summary>
        /// 解压字节流
        /// </summary>
        /// <param name="_inputStream"></param>
        /// <param name="_outputPath"></param>
        /// <param name="_password"></param>
        /// <param name="_unzipCallback"></param>
        /// <returns></returns>
        public bool UnZipBytes(byte[] zipData, string _password = null, UnZipCallback _unzipCallback = null, bool ignoreDirectory = true)
        {
            if (zipData == null)
            {
                if (null != _unzipCallback)
                    _unzipCallback.OnFinished(false);
                return false;
            }

            ZipEntry entry = null;
            using (ZipInputStream zipInputStream = new ZipInputStream(new MemoryStream(zipData)))
            {
                if (!string.IsNullOrEmpty(_password))
                    zipInputStream.Password = _password;

                while (null != (entry = zipInputStream.GetNextEntry()))
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    if ((null != _unzipCallback) && !_unzipCallback.OnPreUnzip(entry))
                        continue;

                    if (entry.IsDirectory && ignoreDirectory)
                    {
                        continue;
                    }
                    //解压文件
                    try
                    {
                        byte[] fileContent = new byte[entry.Size];
                        zipInputStream.Read(fileContent, 0, fileContent.Length);
                        if (null != _unzipCallback)
                            _unzipCallback.OnPostUnzip(entry, entry.Name, fileContent);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("ZipUnity.UnZipBytes: " + e.ToString());
                        if (null != _unzipCallback)
                            _unzipCallback.OnFinished(false);
                        return false;
                    }
                }
            }
            Debug.Log("unzip success!");
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(true);
            return true;
        }
    }
}


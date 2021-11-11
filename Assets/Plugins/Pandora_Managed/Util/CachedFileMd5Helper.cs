using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace com.tencent.pandora
{
    /// <summary>
    /// 检查本地文件方案2：
    /// 1.检查待加载的文件在Cache目录下是否存在
    /// 2.若文件存在，计算其Md5码，再与RemoteConfig中的值比较
    /// </summary>
    public class CachedFileMd5Helper
    {
        private static MD5CryptoServiceProvider MD5_SERVICE = new MD5CryptoServiceProvider();
        private static StringBuilder _sharedBuilder = new StringBuilder();

        public static string GetFileMd5(string path)
        {
            byte[] bytes = null;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch
            {
                Logger.LogError("读取本地资源失败： " + path);
            }
            if(bytes == null)
            {
                return string.Empty;
            }
            return GetFileMd5(bytes);
        }

        public static string GetFileMd5(byte[] fileBytes)
        {
            if(fileBytes.Length == 0)
            {
                return string.Empty;
            }
            byte[] fileMd5Bytes = MD5_SERVICE.ComputeHash(fileBytes);
            _sharedBuilder.Length = 0;
            for(int i = 0; i < fileMd5Bytes.Length; i++)
            {
                _sharedBuilder.Append(fileMd5Bytes[i].ToString("X2"));
            }
            string fileMd5 = _sharedBuilder.ToString();
            //加盐，文件的Md5加上Key再计算一次Md5
            string key = "pandora20151019";
            byte[] combinedBytes = Encoding.UTF8.GetBytes(key + fileMd5);
            byte[] combinedMd5Bytes = MD5_SERVICE.ComputeHash(combinedBytes);
            _sharedBuilder.Length = 0;
            for(int i = 0; i < combinedMd5Bytes.Length; i++)
            {
                _sharedBuilder.Append(combinedMd5Bytes[i].ToString("X2"));
            }
            return _sharedBuilder.ToString();
        }
    }
}

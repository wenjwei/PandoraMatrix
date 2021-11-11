using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;

namespace com.tencent.pandora
{
    public class FileUploader
    {
        private const string UPLOAD_URL = "https://pdrlog.game.qq.com/?c=PandoraSDKLogUpload&a=batch";
        private const int SLICE_SIZE = (1 << 20) * 10; //10Mb 
        private byte[] _rawData;
        private int _lengthRead;
        private string _filePath;
        private bool _uploading = false;

        public void Upload(string filePath)
        {
            if (_uploading == true)
            {
                Logger.LogWarning("有文件正在上传，请稍后再试！");
                return;
            }

            if (string.IsNullOrEmpty(filePath) == true)
            {
                Logger.LogWarning("待上传文件路径为空，请检查！");
                return;
            }

            if (File.Exists(filePath) == false)
            {
                Logger.LogWarning("待上传文件不存在，请检查！");
                return;
            }

            _filePath = filePath;
            _rawData = File.ReadAllBytes(filePath);
            _lengthRead = 0;
            byte[] data = GetSlice();
            if (data.Length == 0)
            {
                Logger.LogWarning("待上传文件内容为空，请检查！");
                return;
            }
            SendRequest(data);
        }

        private byte[] GetSlice()
        {
            int dataLength = _rawData.Length;
            if (_lengthRead == dataLength)
            {
                return new byte[0];
            }

            int size = Math.Min(SLICE_SIZE, dataLength - _lengthRead);
            byte[] slice = new byte[size];
            Array.Copy(_rawData, _lengthRead, slice, 0, size);
            _lengthRead += size;
            return slice;
        }

        private void SendRequest(byte[] data)
        {
            _uploading = true;
            string jsonRequest = GetJsonRequest(data);
            AssetManager.LoadWww(UPLOAD_URL, jsonRequest, data, true, UploadCallBack);
        }

        private string GetJsonRequest(byte[] data)
        {
            UserData userData = DelegateAggregator.GetUserData();
            Dictionary<string, string> request = new Dictionary<string, string>()
            {
                {"openid",userData.sOpenId },
                {"appid",userData.sAppId },
                {"platid",userData.sPlatID },
                {"logintype",userData.sAcountType },
                {"access",userData.sGameName },
                {"accessToken",userData.sAccessToken },
                { "file_name",GenerateUniqueName(data,_filePath)},
            };

            string baseInfo = string.Format("{0}{1}{2}{3}{4}", request["logintype"], request["accessToken"], request["appid"], request["openid"], request["access"]);
            string baseKey = GetStringSha256Hash(baseInfo);

            string date = DateTime.Now.ToString("yyyyMMdd");
            string dateKey = GetStringMd5(date);

            string joinedKey = baseKey + dateKey;
            request["skey"] = GetStringSha256Hash(joinedKey);

            string jsonRequest = MiniJSON.Json.Serialize(request);
            return jsonRequest;
        }

        private string GetStringMd5(string content)
        {
            byte[] convertedContent = Encoding.UTF8.GetBytes(content);
            byte[] hashValue = new MD5CryptoServiceProvider().ComputeHash(convertedContent);
            return ToHexadecimalString(hashValue);
        }

        private string GetStringSha256Hash(string content)
        {
            byte[] convertedContent = Encoding.UTF8.GetBytes(content);
            byte[] hashValue = new SHA256Managed().ComputeHash(convertedContent);
            return ToHexadecimalString(hashValue);
        }

        //byte 数组转换为16进制小写字符串
        private string ToHexadecimalString(byte[] byteArray)
        {
            string hexadecimalString = BitConverter.ToString(byteArray);
            hexadecimalString = hexadecimalString.Replace("-", "");
            hexadecimalString = hexadecimalString.ToLower();
            return hexadecimalString;
        }

        //为文件名添加MD5前缀，防止文件名相同导致文件覆盖，同时能校验上传的文件
        private string GenerateUniqueName(byte[] data, string path)
        {
            byte[] hashValue = new MD5CryptoServiceProvider().ComputeHash(data);
            string md5 = ToHexadecimalString(hashValue);
            string fileName = GetNameByPath(path);
            return string.Format("{0}-{1}", md5, fileName);
        }

        private string GetNameByPath(string path)
        {
            return path.Substring(path.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, '/' }) + 1);
        }

        private void UploadCallBack(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                _uploading = false;
                Logger.LogWarning("文件上传失败。\n");
                return;
            }

            Dictionary<string, object> responseDict = MiniJSON.Json.Deserialize(response) as Dictionary<string, object>;
            int ret = Convert.ToInt32(responseDict["ret"]);
            if (ret != 0)
            {
                _uploading = false;
                Logger.LogWarning("文件上传失败。\n" + response);
                return;
            }

            byte[] data = GetSlice();
            if (data.Length == 0)
            {
                _uploading = false;
                Logger.LogInfo("<color=#00ff00>文件上传成功,请到 http://pandora.ied.com/sdklog/index 查看</color>");
                return;
            }
            SendRequest(data);
        }
    }
}
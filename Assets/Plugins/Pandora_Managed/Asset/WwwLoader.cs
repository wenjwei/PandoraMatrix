using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 通用Http，Https访问基础设施
/// </summary>
namespace com.tencent.pandora
{
    public class WwwLoader: MonoBehaviour
    {
        //通用访问Url
        public void LoadWww(string url, string requestJson, bool isPost, Action<string> callback)
        {
            StartCoroutine(RequestWww(url, requestJson, isPost, callback));
        }

        public void LoadWww(string url, string requestJson, Byte[] binaryData, bool isPost, Action<string> callback)
        {
            StartCoroutine(RequestWww(url, requestJson, isPost, callback, binaryData));
        }

        public void PostLocalFile(string url, string requestJson, string localPath, Action<string> callback)
        {
            Byte[] binary = File.ReadAllBytes(localPath);
            LoadWww(url, requestJson, binary, true, callback);
        }

        private IEnumerator RequestWww(string url, string requestJson, bool isPost, Action<string> callback, Byte[] binaryData = null)
        {
			WwwWrapper wwwWrapper = null;
            if(isPost == true)
            {
                WWWForm wwwForm = new WWWForm();
                GeneratePostData(wwwForm, requestJson, binaryData);
				wwwWrapper = WwwWrapper.Post(url, wwwForm);
                Logger.Log("Post网址： " + url + " PostData: " + requestJson, PandoraSettings.SDKTag);
            }
            else
            {
                url = url + GenerateGetParams(requestJson);
				wwwWrapper = WwwWrapper.Get(url);
                Logger.Log("Get网址： " + url, PandoraSettings.SDKTag);
            }
			yield return wwwWrapper.YieldObject;
			if (string.IsNullOrEmpty(wwwWrapper.Error))
			{
				string result = Encoding.UTF8.GetString(wwwWrapper.Bytes);
				callback(result);
            }
            else
            {
				string error = "访问网址出错，url: " + url + "  " + wwwWrapper.Error;
				Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error);
                callback(string.Empty);
            }
			wwwWrapper.Dispose();
			wwwWrapper = null;
        }

        private string GenerateGetParams(string requestJson)
        {
            if (string.IsNullOrEmpty(requestJson))
            {
                return string.Empty;
            }
            Dictionary<string, System.Object> dict = MiniJSON.Json.Deserialize(requestJson) as Dictionary<string, System.Object>;
            if(dict == null)
            {
                return string.Empty;
            }
            bool first = true;
            StringBuilder builder = new StringBuilder();
            foreach(string key in dict.Keys)
            {
                if(dict[key] != null)
                {
                    if(first == true)
                    {
                        first = false;
                        builder.Append("?");
                    }
                    else
                    {
                        builder.Append("&");
                    }
                    builder.Append(key + "=");
                    builder.Append(dict[key].ToString());
                }
            }
            return builder.ToString();
        }

        private void GeneratePostData(WWWForm wwwForm, string requestJson, Byte[] binaryData = null)
        {
            if (string.IsNullOrEmpty(requestJson))
            {
                return;
            }
            Dictionary<string, System.Object> dict = MiniJSON.Json.Deserialize(requestJson) as Dictionary<string, System.Object>;
            if (dict == null)
            {
                return;
            }

            foreach (KeyValuePair<string, System.Object> pair in dict)
            {
                wwwForm.AddField(pair.Key, pair.Value as string);
            }

            if (binaryData != null)
            {
                wwwForm.AddBinaryData("binaryData", binaryData);
            }

            return;
        }
    }
}

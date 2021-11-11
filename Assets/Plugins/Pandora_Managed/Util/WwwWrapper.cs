using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;
#endif

namespace com.tencent.pandora
{
    public enum RequestResourceType
    {
        Default,
        AssetBundle,
        Texture,
        AudioClip,
    }

    public class WwwWrapper : IDisposable
    {
        WwwWrapper()
        {
            _resourceRequest = CreateResourceRequeset();
        }

        private ResourceRequest _resourceRequest;

        public object YieldObject { get { return _resourceRequest.YieldObject; } }

        public string Error { get { return _resourceRequest.Error; } }

        public byte[] Bytes { get { return _resourceRequest.Bytes; } }

        public float DownloadProgress { get { return _resourceRequest.DownloadProgress; } }

        public AssetBundle AssetBundle { get { return _resourceRequest.AssetBundle; } }

        public Texture2D TextureNonReadable { get { return _resourceRequest.TextureNonReadable; } }

        public AudioClip AudioClipCompressed { get { return _resourceRequest.AudioClipCompressed; } }

        private void GetResource(string url, RequestResourceType resourceType)
        {
            _resourceRequest.Get(ProcessUri(url), resourceType);
        }

        private void PostData(string url, WWWForm form)
        {
            _resourceRequest.Post(ProcessUri(url), form);
        }

        private void PostData(string url, byte[] postData, Dictionary<string, string> headers)
        {
            _resourceRequest.Post(ProcessUri(url), postData, headers);
        }

        private string ProcessUri(string url)
        {
            return EscapeUriString(ReplaceHttpToken(url));
        }

        private string ReplaceHttpToken(string url)
        {
            if (PandoraSettings.UseHttps == true)
            {
                return url.Replace("http://", "https://");
            }
            return url;
        }

        private string EscapeUriString(string url)
        {
#if UNITY_IOS
			return Uri.EscapeUriString(url);
#else
            return url;
#endif
        }

        /// <summary>
        /// 通过在管理端增加FunctionSwitch的方式，控制前端资源加载步骤中，WwwWrapper实际使用的加载接口
        /// 在Unity2017及以上版本中默认为true
        /// </summary>
        private static bool CanUseWebRequest
        {
            get
            {
                return !DelegateAggregator.GetRemoteConfig().GetFunctionSwitch("WwwRequest");
            }
        }

        private static ResourceRequest CreateResourceRequeset()
        {
#if UNITY_2018_3_OR_NEWER
			if (CanUseWebRequest == true)
			{
				return new WebRequest();
			}
			else
#endif
            {
                return new WwwRequest();
            }
        }

        public static WwwWrapper Get(string url, RequestResourceType resourceType = RequestResourceType.Default)
        {
            WwwWrapper wwwWrapper = new WwwWrapper();
            wwwWrapper.GetResource(url, resourceType);
            return wwwWrapper;
        }

        public static WwwWrapper Post(string url, WWWForm form)
        {
            WwwWrapper wwwWrapper = new WwwWrapper();
            wwwWrapper.PostData(url, form);
            return wwwWrapper;
        }

        public static WwwWrapper Post(string url, byte[] postData, Dictionary<string, string> headers)
        {
            WwwWrapper wwwWrapper = new WwwWrapper();
            wwwWrapper.PostData(url, postData, headers);
            return wwwWrapper;
        }

        public void Dispose()
        {
            _resourceRequest.Dispose();
            _resourceRequest = null;
        }
    }

    public abstract class ResourceRequest
    {
        abstract public object YieldObject { get; }
        abstract public string Error { get; }
        abstract public byte[] Bytes { get; }
        abstract public float DownloadProgress { get; }
        abstract public AssetBundle AssetBundle { get; }
        abstract public Texture2D TextureNonReadable { get; }
        abstract public AudioClip AudioClipCompressed { get; }

        public abstract void Get(string url, RequestResourceType resourceType);
        public abstract void Post(string url, WWWForm form);
        public abstract void Post(string url, byte[] postData, Dictionary<string, string> headers);
        public abstract void Dispose();
    }

#if UNITY_2018_3_OR_NEWER
    public class WebRequest : ResourceRequest
    {
        public override object YieldObject { get { return _uwrAsyncOperation; } }
        public override string Error { get { return _uwr.error; } }
        public override byte[] Bytes { get { return _uwr.downloadHandler.data; } }
        public override float DownloadProgress { get { return _uwr.downloadProgress; } }

        public override AssetBundle AssetBundle
        {
            get
            {
                DownloadHandlerAssetBundle handler = _uwr.downloadHandler as DownloadHandlerAssetBundle;
                return handler != null ? handler.assetBundle : null;
            }
        }

        public override Texture2D TextureNonReadable
        {
            get
            {
                DownloadHandlerTexture handler = _uwr.downloadHandler as DownloadHandlerTexture;
                return handler != null ? handler.texture : null;
            }
        }

        public override AudioClip AudioClipCompressed
        {
            get
            {
                DownloadHandlerAudioClip handler = _uwr.downloadHandler as DownloadHandlerAudioClip;
                if (handler != null)
                {
                    handler.compressed = true;
                    return handler.audioClip;
                }
                return null;
            }
        }

        private UnityWebRequest _uwr;
        private UnityWebRequestAsyncOperation _uwrAsyncOperation;

        public override void Get(string url, RequestResourceType resourceType)
        {
            Logger.Log(string.Format("<color=#00ff00>WebRequest.Get</color>, url: {0}; resourceType: {1}", url, resourceType),PandoraSettings.SDKTag);
            _uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            switch (resourceType)
            {
                case RequestResourceType.Default:
                    GetDefault(_uwr);
                    break;
                case RequestResourceType.AssetBundle:
                    GetAssetBundle(_uwr, url);
                    break;
                case RequestResourceType.Texture:
                    //GetTexture(_uwr, false);
                    GetTexture(_uwr, true);
                    break;
                case RequestResourceType.AudioClip:
                    GetAudioClip(_uwr, url);
                    break;
            }
            _uwrAsyncOperation = _uwr.SendWebRequest();
        }

        private void GetDefault(UnityWebRequest uwr)
        {
            uwr.downloadHandler = new DownloadHandlerBuffer();
        }

        private void GetAssetBundle(UnityWebRequest uwr, string url)
        {
            uwr.downloadHandler = new DownloadHandlerAssetBundle(url, 0);
        }

        private void GetTexture(UnityWebRequest uwr, bool readable)
        {
            uwr.downloadHandler = new DownloadHandlerTexture(readable);
        }

        private void GetAudioClip(UnityWebRequest uwr, string url)
        {
            uwr.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.UNKNOWN);
        }

        public override void Post(string url, WWWForm form)
        {
            Logger.Log(string.Format("<color=#00ff00>WebRequest.PostForm</color>, url: {0}", url),PandoraSettings.SDKTag);
            _uwr = UnityWebRequest.Post(url, form);
            _uwr.chunkedTransfer = false;
            _uwrAsyncOperation = _uwr.SendWebRequest();
        }

        public override void Post(string url, byte[] postData, Dictionary<string, string> headers)
        {
            Logger.Log(string.Format("<color=#00ff00>WebRequest.PostData</color>, url: {0}", url),PandoraSettings.SDKTag);
            var verb = postData == null ? UnityWebRequest.kHttpVerbGET : UnityWebRequest.kHttpVerbPOST;
            _uwr = new UnityWebRequest(url, verb);
            _uwr.chunkedTransfer = false;
            UploadHandler formUploadHandler = new UploadHandlerRaw(postData);
            formUploadHandler.contentType = "application/x-www-form-urlencoded";
            _uwr.uploadHandler = formUploadHandler;
            _uwr.downloadHandler = new DownloadHandlerBuffer();
            foreach (var header in headers)
                _uwr.SetRequestHeader(header.Key, header.Value);
            _uwrAsyncOperation = _uwr.SendWebRequest();
        }

        public override void Dispose()
        {
            if (_uwr != null)
            {
                _uwr.Dispose();
                _uwr = null;

            }
        }
    }
#endif

    public class WwwRequest : ResourceRequest
    {
        public override object YieldObject { get { return _www; } }
        public override string Error { get { return _www.error; } }
        public override byte[] Bytes { get { return _www.bytes; } }
        public override float DownloadProgress { get { return _www.progress; } }
        public override AssetBundle AssetBundle { get { return _www.assetBundle; } }
        public override Texture2D TextureNonReadable { get { return _www.textureNonReadable; } }
        public override AudioClip AudioClipCompressed { get { return _www.GetAudioClipCompressed(); } }

#if UNITY_2018_3_OR_NEWER
#pragma warning disable CS0618 // 类型或成员已过时
#endif
        private WWW _www;

        public override void Get(string url, RequestResourceType resourceType)
        {
            Logger.Log(string.Format("<color=#00ff00>WwwRequest.Get</color>, url: {0}; resourceType: {1}", url, resourceType), PandoraSettings.SDKTag);
            _www = new WWW(url);
        }

        public override void Post(string url, WWWForm form)
        {
            Logger.Log(string.Format("<color=#00ff00>WwwRequest.PostForm</color>, url: {0}", url), PandoraSettings.SDKTag);
            _www = new WWW(url, form);
        }

        public override void Post(string url, byte[] postData, Dictionary<string, string> headers)
        {
            Logger.Log(string.Format("<color=#00ff00>WwwRequest.PostData</color>, url: {0}", url), PandoraSettings.SDKTag);
            _www = new WWW(url, postData, headers);
        }

        public override void Dispose()
        {
            if (_www != null)
            {
                _www.Dispose();
                _www = null;
            }
        }
#if UNITY_2018_3_OR_NEWER
#pragma warning restore CS0618 // 类型或成员已过时
#endif
    }
}
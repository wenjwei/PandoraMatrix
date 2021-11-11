using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{
    public abstract class AbstractLoader : MonoBehaviour
    {
        //加载出错的队列，会静默加载（不断的尝试加载），为避免过于频繁设置一个间隔
        protected Queue<string> _errorQueue;
        //重试间隔，以帧数为单位
        protected int _errorRetryMaxSpan = 300;
        protected int _errorRetrySpan = 0;
        //Key为资源路径，不含?seed=的
        protected Dictionary<string, int> _errorRetryCountDict;
        //Key为资源路径，不含?seed=的
        protected Dictionary<string, int> _errorMaxRetryCountDict;

        //下载并发数
        protected int _concurrentCount = 4;
        protected Queue<string> _toLoadQueue;
        protected Dictionary<string, WwwWrapper> _loadingDict;
        protected HashSet<string> _loadingSet;
        protected HashSet<string> _loadedSet;
        protected HashSet<string> _failedSet;
        protected List<Task> _taskList;
        private List<Coroutine> _coroutineList;

        public virtual void Initialize()
        {
            _toLoadQueue = new Queue<string>();
            _loadingDict = new Dictionary<string, WwwWrapper>();
            _loadingSet = new HashSet<string>();
            _loadedSet = new HashSet<string>();
            _failedSet = new HashSet<string>();
            _taskList = new List<Task>();
            _errorQueue = new Queue<string>();
            _errorRetryCountDict = new Dictionary<string, int>();
            _errorMaxRetryCountDict = new Dictionary<string, int>();
            _coroutineList = new List<Coroutine>();
        }

        private void InspectLoad()
        {
            if (_toLoadQueue.Count > 0 && _coroutineList.Count == 0)
            {
                for (int i = 0; i < _concurrentCount; i++)
                {
                    _coroutineList.Add(StartCoroutine(DaemonLoad()));
                }
            }
            if (_loadingSet.Count == 0)
            {
                for (int i = 0; i < _coroutineList.Count; i++)
                {
                    StopCoroutine(_coroutineList[i]);
                }
                _coroutineList.Clear();
            }
        }

        protected virtual IEnumerator DaemonLoad()
        {
            while (true)
            {
                if (_toLoadQueue.Count > 0 && PandoraSettings.PauseDownloading == false)
                {
                    string url = _toLoadQueue.Dequeue();
                    string originalUrl = GetOriginalUrl(url);
                    WwwWrapper wwwWrapper = CreateWwwWrapper(url);
                    _loadingDict.Add(originalUrl, wwwWrapper);
                    _loadingSet.Add(originalUrl);
                    yield return wwwWrapper.YieldObject;
                    AssetBundleRequest request = GetAssetBundleRequest(originalUrl, wwwWrapper);
                    if (request != null)
                    {
#if UNITY_EDITOR && UNITY_2018_4_16
                        // UNITY2018.4.16 Editor版本执行yield return request函数存在Bug
                        // 对于Prefab类型资源，会卡住无返回，这里先单独特殊处理
                        while (true)
                        {
                            if (request.progress > 0.99f)
                                break;
                            yield return null;
                        }
#else
                        //https://docs.unity3d.com/ScriptReference/AsyncOperation.html
                        yield return request;
#endif
                    }
                    _loadingDict.Remove(originalUrl);
                    _loadingSet.Remove(originalUrl);

                    if (string.IsNullOrEmpty(wwwWrapper.Error) == false)
                    {
                        Logger.LogError("资源加载错误： " + originalUrl + " " + wwwWrapper.Error,PandoraSettings.SDKTag);
                        //错误处理，放入_errorQueue, 稍后重新加载
                        _errorQueue.Enqueue(GetRandomSeedUrl(url));
                    }
                    else
                    {
                        if (HandleLoadedContent(originalUrl, wwwWrapper, request) == true)
                        {
                            _errorMaxRetryCountDict.Remove(originalUrl);
                            _errorRetryCountDict.Remove(originalUrl);
                            _loadedSet.Add(originalUrl);
                        }
                        else
                        {
                            _errorQueue.Enqueue(GetRandomSeedUrl(url));
                        }
                    }

                }
                yield return null;
            }
        }

        //发生错误重连时需要加上?seed=xxx以消除cdn节点缓存的影响
        public string GetRandomSeedUrl(string url)
        {
            return GetOriginalUrl(url) + "?seed=" + UnityEngine.Random.Range(0, 10000).ToString();
        }

        public string GetOriginalUrl(string url)
        {
            if (url.Contains("?seed=") == false)
            {
                return url;
            }
            int index = url.IndexOf("?seed=");
            return url.Substring(0, index);
        }

        private void DaemonHandleTaskLoaded()
        {
            for (int i = _taskList.Count - 1; i >= 0; i--)
            {
                Task task = _taskList[i];
                if (task.IsFinished() == true)
                {
                    _taskList.RemoveAt(i);
                    task.ExecuteLoadedCallback();
                }
            }
        }

        private void DaemonHandleError()
        {
            if (_errorQueue.Count > 0)
            {
                if (_errorRetrySpan < _errorRetryMaxSpan)
                {
                    _errorRetrySpan += 1;
                }
                else
                {
                    _errorRetrySpan = 0;
                    string url = _errorQueue.Dequeue();
                    string originalUrl = GetOriginalUrl(url);
                    if ((_errorRetryCountDict.ContainsKey(originalUrl) && _errorMaxRetryCountDict.ContainsKey(originalUrl))
                        && _errorRetryCountDict[originalUrl] < _errorMaxRetryCountDict[originalUrl])
                    {
                        _errorRetryCountDict[originalUrl] = _errorRetryCountDict[originalUrl] + 1;
                        Logger.Log("重连： " + url, PandoraSettings.SDKTag);
                        _toLoadQueue.Enqueue(url);
                    }
                    else
                    {
                        _errorMaxRetryCountDict.Remove(originalUrl);
                        _errorRetryCountDict.Remove(originalUrl);
                        _loadingDict.Remove(originalUrl);
                        _loadingSet.Remove(originalUrl);
                        _failedSet.Add(originalUrl);
                        HandleError(originalUrl);
                    }
                }
            }
        }

        protected virtual WwwWrapper CreateWwwWrapper(string url)
        {
            return WwwWrapper.Get(url);
        }

        protected virtual AssetBundleRequest GetAssetBundleRequest(string url, WwwWrapper wwwWrapper)
        {
            return null;
        }

        /// <summary>
        /// 资源加载后的操作
        /// </summary>
        /// <param name="url"></param>
        /// <param name="www"></param>
        protected virtual bool HandleLoadedContent(string url, WwwWrapper wwwWrapper, AssetBundleRequest request)
        {
            return false;
        }
        /// <summary>
        /// 重试次数用完后依然没有获取到数据时的处理
        /// </summary>
        /// <param name="url"></param>
        protected abstract void HandleError(string url);

        public bool IsLoaded(string url)
        {
            return _loadedSet.Contains(GetOriginalUrl(url));
        }

        public bool IsFailed(string url)
        {
            return _failedSet.Contains(GetOriginalUrl(url));
        }

        public bool RemoveFormLoadedSet(string url)
        {
            return _loadedSet.Remove(GetOriginalUrl(url));
        }

        public virtual void LoadAsset(string url, Action<string> callback, int maxRetryCount)
        {
            AddAsset(url, maxRetryCount);
            SingleTask task = new SingleTask(url, callback, this);
            _taskList.Insert(0, task);
        }

        /// <summary>
        /// 将资源加载存放到
        /// </summary>
        /// <param name="urlList"></param>
        /// <param name="callback"></param>
        public virtual void LoadAssetList(List<string> urlList, Action<List<string>> callback, int maxRetryCount)
        {
            for (int i = 0; i < urlList.Count; i++)
            {
                AddAsset(urlList[i], maxRetryCount);
            }
            BatchTask task = new BatchTask(urlList, callback, this);
            _taskList.Insert(0, task);
        }

        private void AddAsset(string url, int maxRetryCount)
        {
            if (_loadingSet.Contains(GetOriginalUrl(url)) || _loadedSet.Contains(GetOriginalUrl(url)) || _toLoadQueue.Contains(url))
            {
                return;
            }

            _failedSet.Remove(url);
            _toLoadQueue.Enqueue(url);
            _errorRetryCountDict[GetOriginalUrl(url)] = 1;
            _errorMaxRetryCountDict[GetOriginalUrl(url)] = maxRetryCount;
        }

        protected virtual void Update()
        {
            DaemonHandleTaskLoaded();
            InspectLoad();
            DaemonHandleError();
        }

        public virtual void Clear()
        {
            _toLoadQueue.Clear();
            _loadingDict.Clear();
            _loadingSet.Clear();
            _loadedSet.Clear();
            _failedSet.Clear();
            _taskList.Clear();
            _errorQueue.Clear();
            _errorRetryCountDict.Clear();
            _errorMaxRetryCountDict.Clear();
        }

        public abstract class Task
        {
            protected AbstractLoader _loader;
            protected bool _isSuccessful = true;
            public abstract bool IsFinished();
            public abstract void ExecuteLoadedCallback();
        }

        /// <summary>
        /// 批量加载模式
        /// </summary>
        public class BatchTask : Task
        {
            private List<string> _urlList;
            private Action<List<string>> _callback;

            public BatchTask(List<string> urlList, Action<List<string>> callback, AbstractLoader loadedr)
            {
                _urlList = urlList;
                _callback = callback;
                _loader = loadedr;
            }

            public override bool IsFinished()
            {
                bool finished = true;
                for (int j = 0; j < _urlList.Count; j++)
                {
                    string url = _urlList[j];

                    if (_loader.IsLoaded(url) == false && _loader.IsFailed(url) == false)
                    {
                        finished = false;
                        break;
                    }
                }
                return finished;
            }

            public override void ExecuteLoadedCallback()
            {
                for (int i = 0; i < _urlList.Count; i++)
                {
                    if (_loader.IsFailed(_urlList[i]) == true)
                    {
                        _isSuccessful = false;
                        break;
                    }
                }
                if (_callback != null && _isSuccessful == true)
                {
                    _callback(_urlList);
                    _callback = null;
                }
            }
        }

        internal class SingleTask : Task
        {
            private string _url;
            private Action<string> _callback;

            public SingleTask(string url, Action<string> callback, AbstractLoader loader)
            {
                _url = url;
                _callback = callback;
                _loader = loader;
            }

            public override bool IsFinished()
            {
                return _loader.IsLoaded(_url) || _loader.IsFailed(_url);
            }

            public override void ExecuteLoadedCallback()
            {
                _isSuccessful = !_loader.IsFailed(_url);
                if (_callback != null /*&& _isSuccessful == true*/)
                {
                    _callback(_url);
                    _callback = null;
                }
            }
        }

    }
}

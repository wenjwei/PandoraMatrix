using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
#endif

namespace com.tencent.pandora
{
    public class LuaStateManager
    {
        private static LuaSvr _luaSvr;
        private static bool _isInitialized;

        private static HashSet<string> _executingLuaGroupSet = new HashSet<string>();
        private static HashSet<string> _executingLuaEntrySet = new HashSet<string>();
        private static HashSet<string> _executedLuaEntrySet = new HashSet<string>();

        public static HashSet<string> GetExecutedLuaEntrySet()
        {
            return _executedLuaEntrySet;
        }

        /// <summary>
        /// 启动lua虚拟机
        /// </summary>
        public static void Initialize()
        {
            _luaSvr = new LuaSvr();
            _luaSvr.init(null, OnComplete, LuaSvrFlag.LSF_BASIC | LuaSvrFlag.LSF_EXTLIB);
        }

        private static void OnComplete()
        {
            LuaDLL.releaseLuaAsset = AssetManager.ReleaseLuaAsset;
            LuaState.loaderDelegate = AssetManager.GetLuaBytes;
            LuaState.errorDelegate = ReportLuaError;
            _isInitialized = true;
            Logger.LogInfo("Lua虚拟机初始化成功", PandoraSettings.SDKTag);
        }

        public static void ReportLuaError(string error)
        {
            DelegateAggregator.ReportError(error, ErrorCode.LUA_SCRIPT_EXCEPTION);
            DelegateAggregator.Report(error, ErrorCode.LUA_SCRIPT_EXCEPTION_DETAIL, ErrorCode.TNM2_TYPE_LITERALS);
            Logger.LogError(error, PandoraSettings.SDKTag);
        }

        public static void Reset()
        {
            _isInitialized = false;
            _executingLuaGroupSet.Clear();
            _executingLuaEntrySet.Clear();
            _executedLuaEntrySet.Clear();
            //lua虚拟机尚未初始化的情况下logout时需要判断一下
            if (_luaSvr != null)
            {
                _luaSvr.close();
				_luaSvr = null;
            }
        }

        public static bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        /// <summary>
        /// 执行Lua文件入口
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static System.Object DoFile(string fileName)
        {
            byte[] bytes = AssetManager.GetLuaBytes(fileName);
            return DoBuffer(bytes, fileName);
        }

        public static System.Object DoBuffer(byte[] bytes, string fileName)
        {
            if(bytes == null || bytes.Length == 0)
            {
                Logger.LogError("没有找到Lua文件： " + fileName, PandoraSettings.SDKTag);
                return null;
            }
            System.Object result;
            if (_luaSvr.luaState[0].doBuffer(bytes, fileName, out result) == true)
            {
                return result;
            }
            return null;
        }

        public static bool IsGroupLuaExecuting(string group)
        {
            return _executingLuaGroupSet.Contains(group);
        }


        /// <summary>
        /// 加载lua活动代码
        /// </summary>
        /// <param name="group">文件所在的组名称</param>
        /// <param name="luaName">需要加载的文件名称</param>
        public static void LoadAction(string group, string luaName)
        {
            try
            {
                if (IsInitialized == true)
                {
                    object o = CallLuaFunction("Common.LoadAction", group, luaName);
                    Release(o);
                }
            }
            catch (Exception e)
            {
                string error = string.Format("加载{0}中的{1}文件出现异常 {2}", group, luaName, e.Message);
                Logger.LogError(error, PandoraSettings.SDKTag);
            }
        }
        /// <summary>
        /// 执行一个文件组中的Lua文件，在Editor环境下从Actions/Resources下加载
        /// </summary>
        /// <param name="fileInfoList"></param>
        public static void DoLuaFileInFileInfoList(string group, List<RemoteConfig.AssetInfo> fileInfoList)
        {
            if(_executingLuaGroupSet.Contains(group) == true)
            {
                string error = "资源组 " + group + " 中的Lua文件正在运行中~";
                Logger.LogError(error, PandoraSettings.SDKTag);
                DelegateAggregator.ReportError(error);
                return;
            }
            _executingLuaGroupSet.Add(group);
            for(int i  = 0; i < fileInfoList.Count; i++)
            {
                string name = fileInfoList[i].name;
                if(name.ToLower().Contains("_lua") == true)
                {
                    string luaName = GetLuaName(name);
                    if(_executingLuaEntrySet.Contains(luaName) == true)
                    {
                        continue;
                    }
                    _executingLuaEntrySet.Add(luaName);
                    string msg = "资源组加载完成，开始执行入口Lua文件： " + luaName;
                    Logger.Log(msg, PandoraSettings.SDKTag);
                    try
                    {
                        
                        if (AssetPool.NeedSandBox(luaName))
                        {
                            LoadAction(group, luaName);
                        }
                        else
                        {
                            object o = DoFile(luaName);
                            Release(o);
                        }

                        _executedLuaEntrySet.Add(luaName);
                    }
                    catch(Exception e)
                    {
                        string error = "Lua DoFile 失败， FileName: " + luaName + " : " + e.Message;
                        DelegateAggregator.ReportError(error, ErrorCode.LUA_DO_FILE_EXCEPTION);
                        DelegateAggregator.Report(error, ErrorCode.LUA_SCRIPT_EXCEPTION_DETAIL, ErrorCode.TNM2_TYPE_LITERALS);
                    }
                }
            }
            //只上报第一个模块执行，用于标记成功执行到lua的用户数量
            if(_executingLuaGroupSet.Count == 1)
            {
                string msg = "执行Lua了";
                DelegateAggregator.ReportError(msg, ErrorCode.EXECUTE_ENTRY_LUA);
            }
        }

        private static string GetLuaName(string name)
        {
            return name.Split('_')[1];
        }

        public static System.Object CallLuaFunction(string functionName, params System.Object[] args)
        {
            LuaFunction func = (LuaFunction)_luaSvr.luaState[0][functionName];
            if(func != null)
            {
                object result = null;
                try
                {
#if PANDORA_PROFILE
                    LuaObject.beginSample("[C#]LuaStateManager.CallLuaFunction " + functionName);
#endif
                    result = func.call(args);
                }
                catch(System.Exception e)
                {
                    Logger.LogError(e.Message, PandoraSettings.SDKTag);
                }
#if PANDORA_PROFILE
                finally
                {
                    LuaObject.endSample();
                }
#endif
                func.Dispose();
                return result;
            }
            return null;
        }

        public static void Release(object o)
        {
            IDisposable disposable = o as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public static void ClearObject(UnityEngine.Object obj)
        {
            if (_isInitialized == true)
            {
                _luaSvr.luaState[0].ClearObject(obj);
            }
        }
    }
}

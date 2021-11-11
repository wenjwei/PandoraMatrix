using System;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

namespace com.tencent.pandora
{
    //Event codes
    public enum LuaEventCode 
    {
        LUA_HOOKCALL = 0,
        LUA_HOOKRET = 1,
        LUA_HOOKLINE = 2,
        LUA_HOOKCOUNT = 3,
        LUA_HOOKTAILRET = 4,
    }

    //Event masks
    public enum LuaEventMask 
    {
        LUA_MASKCALL = 1,   //1 << LUA_HOOKCALL
        LUA_MASKRET = 2,    //(1 << LUA_HOOKRET)
        LUA_MASKLINE = 4,   //(1 << LUA_HOOKLINE)
        LUA_MASKCOUNT = 8,  //(1 << LUA_HOOKCOUNT)
    }

    //activation record，注意后续代码中出现的参数ar即为activation record缩写，内容为Lua_Debug结构体
    [StructLayout(LayoutKind.Sequential)]
    public struct Lua_Debug
    {
        public int eventCode;
        public IntPtr name;                 /* (n) */
        public IntPtr namewhat;             /* (n) `global', `local', `field', `method' */
        public IntPtr what;                 /* (S) `Lua', `C', `main', `tail' */
        public IntPtr source;               /* (S) */
        public int currentline;             /* (l) */
        public int nups;                    /* (u) number of upvalues */
        public int linedefined;             /* (S) */
        public int lastlinedefined;         /* (S) */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] short_src;
        public int i_ci;                    /* active function */
    }

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LuaHookFunc(IntPtr L, IntPtr ar);

    static class LuaProfiler
    {

        private static EditorWindow _profilerWindow;
        const string LUADLL = "pandora";

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pua_sethook(IntPtr L, LuaHookFunc func, int mask, int count);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pua_getinfo(IntPtr luaState, string what, IntPtr ar);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pua_getstack(IntPtr luaState, int level, IntPtr ar);

        private static Stack<int> _callStack; //Lua函数调用深度值记录栈
        private static int _depth; //Lua函数调用深度值
        private static StringBuilder _trace;
        private static Dictionary<int, string> _sampleLabelDict = new Dictionary<int, string>();

        private static int line = 0;
        private static string name = string.Empty;
        private static string what = string.Empty;
        private static string source = string.Empty;

        [com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaHookFunc))]
        private static void DebugHook(IntPtr luaState, IntPtr ar)
        {
            try
            {
                pua_getinfo(luaState, "nS", ar);
                Lua_Debug luaDebug = (Lua_Debug)Marshal.PtrToStructure(ar, typeof(Lua_Debug));
                switch ((LuaEventCode)luaDebug.eventCode)
                {
                    case LuaEventCode.LUA_HOOKCALL:
                        _depth += 1;
                        if (IsValidLuaCode(luaDebug, ref line, ref what,  ref source) == true)
                        {
                            _callStack.Push(_depth);
                            name = Marshal.PtrToStringAnsi(luaDebug.name);
                            if (string.IsNullOrEmpty(name) == true) name = "CALL_FROM_C#_OR_C";

							// https://unity3d.com/cn/unity/whats-new/unity-5.5.0
							// Profiler class moved from UnityEngine namespace to UnityEngine.Profiling.
#if UNITY_5_5_OR_NEWER
							UnityEngine.Profiling.Profiler.BeginSample(GetSampleLabel(line, what, source, name));
#else
							Profiler.BeginSample(GetSampleLabel(line, what, source, name));
#endif
						}
						break;
                    case LuaEventCode.LUA_HOOKRET:
                    case LuaEventCode.LUA_HOOKTAILRET:
                        if(_depth == _callStack.Peek())
                        {
                            _callStack.Pop();
#if UNITY_5_5_OR_NEWER
							UnityEngine.Profiling.Profiler.EndSample();
#else
							Profiler.EndSample();
#endif
						}
						_depth -= 1;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Lua hook exception: " + ex.Message);
                pua_sethook(GetLuaStatePointer(), DebugHook, 0, 0);
            }
        }

        private static bool IsValidLuaCode(Lua_Debug luaDebug, ref int line, ref string what, ref string source)
        {
            line = luaDebug.linedefined;
            if(line == -1)
            {
                return false;
            }
            what = Marshal.PtrToStringAnsi(luaDebug.what);
            if(what == "C")
            {
                return false;
            }
            source = Marshal.PtrToStringAnsi(luaDebug.source);
            if(source.Contains(".bytes") == false)
            {
                return false;
            }
            return true;
        }

        private static string GetSampleLabel(int line, string what, string source, string name)
        {
            int key = what.GetHashCode() + name.GetHashCode() + source.GetHashCode() + line;
            if(_sampleLabelDict.ContainsKey(key))
            {
                return _sampleLabelDict[key];
            }
            string label = string.Format("[{0}]{1}: {2} {3}", what, name, line, source);
            _sampleLabelDict.Add(key, label);
            return label;
        }

        [MenuItem ("PandoraTools/Profiler/Attach Lua Profiler")]
        private static void ExecuteAttach ()
        {
            if (Application.isPlaying == false)
            {
                _profilerWindow = null;
                
                EditorUtility.DisplayDialog("提示", "需要运行后再开启Lua Profiler", "好的");
                return;
            }
            if(GetLuaStatePointer() == IntPtr.Zero)
            {
                _profilerWindow = null;
                EditorUtility.DisplayDialog("提示", "请确认Lua虚拟机正确启动后再开启Lua Profiler", "好的");
                return;
            }
            if (_profilerWindow == null && LuaStateManager.IsInitialized == true)
            {
                _callStack = new Stack<int>();
                _trace = new StringBuilder();
                _depth = -1;
                _sampleLabelDict = new Dictionary<int, string>();
                pua_sethook(GetLuaStatePointer(), DebugHook, (int)LuaEventMask.LUA_MASKCALL | (int)LuaEventMask.LUA_MASKRET, 0);
            }
            OpenUnityProfilerWindow();
        }

        private static void OpenUnityProfilerWindow()
        {
            if(_profilerWindow == null)
            {
                Type type = GetTypeByFullName("UnityEditor.ProfilerWindow");
                if (type != null)
                {
                    _profilerWindow = EditorWindow.GetWindow(type);
                    _profilerWindow.Show();
                }
            }
            else
            {
                _profilerWindow.Show();
            }
        }

        private static void CloseUnityProfilerWindow()
        {
            if(_profilerWindow != null)
            {
                _profilerWindow.Close();
                _profilerWindow = null;
            }
        }

        private static Type GetTypeByFullName(string typeFullName)
        {
            var assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblyArray)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Equals(typeFullName))
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        [MenuItem("PandoraTools/Profiler/Detach Lua Profiler")]
        private static void ExecuteDetach ()
        {
            Debug.LogError(_trace.ToString());
            if (LuaStateManager.IsInitialized == true)
            {
               pua_sethook(GetLuaStatePointer(), DebugHook, 0, 0);
            }
            CloseUnityProfilerWindow();
        }

        private static IntPtr GetLuaStatePointer()
        {
            GameObject sluaSvrGameObject = GameObject.Find("LuaStateProxy_0");
            if (sluaSvrGameObject == null)
            {
                string error = "lua 虚拟机未在运行中，请运行游戏工程后做此操作！";
                Logger.LogError(error,PandoraSettings.SDKTag);
                return IntPtr.Zero;
            }
            return sluaSvrGameObject.GetComponent<LuaSvrGameObject>().state.L;
        }
    }
}
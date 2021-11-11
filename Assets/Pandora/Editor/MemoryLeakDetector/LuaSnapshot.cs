/*
    [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr pua_topointer(IntPtr l, int index);
 */
using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace com.tencent.pandora.tools
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LuaDebug
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


    public class LuaSnapshot
    {

        //ar 是LuaDebug 型指針
        [DllImport("pandora", CallingConvention = CallingConvention.Cdecl)]
        public static extern int pua_getinfo(IntPtr L, string what, IntPtr ar);

        [DllImport("pandora", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr pua_topointer(IntPtr l, int index);

        [DllImport("pandora", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr pua_getupvalue(IntPtr l, int funcIndex, int n);

        private static int LUA_MINSTACK = 20;

        private const int TABLE = 1;
        private const int FUNCTION = 2;
        private const int SOURCE = 3;
        private const int USERDATA = 4;
        private const int MARK = 5;

        private static Dictionary<IntPtr, string> _snapshot;

        private static List<string> _sentryExcludeLIst = new List<string>()
        {
            "[registry]",
            "[metatable]",
        };

        private static HashSet<IntPtr> _initStateTable = new HashSet<IntPtr>();
        private static HashSet<IntPtr> _activityOpenedStateTable = new HashSet<IntPtr>();
        private static bool _isPreparePeriod = false;

        public static void PrepareToInsertSentry()
        {
            _initStateTable.Clear();
            _isPreparePeriod = true;
            Snapshot();
        }

        public static  void ExecuteInsertSentry()
        {
            _activityOpenedStateTable.Clear();
            _isPreparePeriod = false;
            Snapshot();
        }

        public static Dictionary<IntPtr, string> Snapshot()
        {
            var sluaSvrGameObject = GameObject.Find("LuaStateProxy_0");
            if (sluaSvrGameObject == null)
            {
                Debug.LogWarning("lua 虚拟机还未运行,请先运行工程");
                return new Dictionary<IntPtr, string>();
            }
            IntPtr luaState = sluaSvrGameObject.GetComponent<LuaSvrGameObject>().state.L;
            LuaDLL.pua_gc(luaState, LuaGCOptions.LUA_GCCOLLECT, 0);

            // 创建新的虚拟机
            IntPtr dumpLuaState = LuaDLL.puaL_newstate();
            for (int i = 0; i < MARK; i++)
            {
                LuaDLL.pua_newtable(dumpLuaState);
            }

            LuaDLL.pua_pushvalue(luaState, LuaIndexes.LUA_REGISTRYINDEX);
            MarkTable(luaState, dumpLuaState, IntPtr.Zero, "[registry]");
            LuaDLL.pua_close(dumpLuaState);
            return _snapshot;
        }

        static void WriteFile(Dictionary<IntPtr, string> dict)
        {
            string path = Path.Combine(Application.dataPath,"../luasnapshot.txt");
            var sw = File.AppendText(path);
            foreach (var item in dict)
            {
                sw.WriteLine(string.Format("{0}:{1}",item.Key,item.Value));
            }
            sw.Flush();
            sw.Close();
        }

        static void MarkTable(IntPtr luaState, IntPtr dumpLuaState, IntPtr parent, string description)
        {
            IntPtr tablePointer = ReadObject(luaState, dumpLuaState, parent, description);
            if (tablePointer == IntPtr.Zero)
            {
                return;
            }

            bool weakKey = false;
            bool weakValue = false;

            //读取元表 
            if (LuaDLL.pua_getmetatable(luaState, -1) != 0)
            {
                LuaDLL.pua_pushstring(luaState, "__mode");
                LuaDLL.pua_rawget(luaState, -2);
                if (LuaDLL.pua_isstring(luaState, -1))
                {
                    string mode = LuaDLL.pua_tostring(luaState, -1);
                    if (mode.Contains("k"))
                    {
                        weakKey = true;
                    }
                    if (mode.Contains("v"))
                    {
                        weakValue = true;
                    }
                }
                LuaDLL.pua_pop(luaState, 1);
                //扩展栈大小，以防溢出
                LuaDLL.pua_checkstack(luaState, LUA_MINSTACK);
                MarkTable(luaState, dumpLuaState, tablePointer, "[metatable]");
            }

            //遍历table
            LuaDLL.pua_pushnil(luaState);
            while (LuaDLL.pua_next(luaState, -2) != 0)
            {
                if (weakValue)
                {
                    //弱引用的对象不用记录，这里的引用不会造成其释放不了
                    LuaDLL.pua_pop(luaState, 1);
                }
                else
                {
                    MarkObject(luaState, dumpLuaState, tablePointer, GetKeyDescription(luaState, -2));
                }

                if (weakKey == false)
                {
                    //把键值再压入一次,因为标记后会被弹出
                    LuaDLL.pua_pushvalue(luaState, -1);
                    MarkObject(luaState, dumpLuaState, tablePointer, "[key]");
                }
            }

            LuaDLL.pua_pop(luaState, 1);
        }

        static bool IsNecessaryInsertSentry(string tableType)
        {
            for (int i = 0; i < _sentryExcludeLIst.Count; i++)
            {
                if (_sentryExcludeLIst[i] == tableType)
                {
                    return false;
                }
            }

            return true;
        }

        /*
        //记录形式 
       TABLE_tbl = {
            //每条记录记录其指针,父指针和描述.
            pointer1 =  {
                            parent1 = description1，
                            parent2 = description2,
                            ... 
                         },
            
            pointer2 =  {
                            parent1 = description1，
                            parent2 = description2,
                            ... 
                         },           
        */
        static IntPtr ReadObject(IntPtr luaState, IntPtr dumpLuaState, IntPtr parent, string description)
        {
            LuaTypes t = LuaDLL.pua_type(luaState, -1);
            int tableIndex = 0;
            switch (t)
            {
                case LuaTypes.LUA_TTABLE:
                    tableIndex = TABLE;
                    //插入sentry；
                    InsertSentryIntoTbl(luaState,description);
                    break;
                case LuaTypes.LUA_TFUNCTION: 
                    tableIndex = FUNCTION;
                    break;
                case LuaTypes.LUA_TUSERDATA:
                    tableIndex = USERDATA;
                    break;
                default:
                    //弹出 保持栈平衡
                    LuaDLL.pua_pop(luaState, 1);
                    return IntPtr.Zero;
            }



            IntPtr pointer = pua_topointer(luaState, -1);

            if (IsMarked(dumpLuaState, pointer))
            {
                RawGet(dumpLuaState, tableIndex, pointer);
                if (LuaDLL.pua_isnil(dumpLuaState, -1) == false)
                {
                    //插入记录  {parent1 = description1，parent2 = description2 ... }
                    LuaDLL.pua_pushstring(dumpLuaState, description);
                    RawSet(dumpLuaState, -2, parent);
                }
                LuaDLL.pua_pop(dumpLuaState, 1);
                LuaDLL.pua_pop(luaState, 1);
                return IntPtr.Zero;
            }
            else
            {
                LuaDLL.pua_newtable(dumpLuaState);
                LuaDLL.pua_pushstring(dumpLuaState, description);
                RawSet(dumpLuaState, -2, parent);
                RawSet(dumpLuaState, tableIndex, pointer);
                return pointer;
            }
        }

        //元表不插入sentry
        static void InsertSentryIntoTbl(IntPtr luaState, string description)
        {
            var ptr = pua_topointer(luaState, -1);
            if (_isPreparePeriod)
            {
                _initStateTable.Add(ptr);
            }
            else if (IsNecessaryInsertSentry(description) && !_initStateTable.Contains(ptr))
            {
                string tableContent = GetTableContent(luaState, -1);
                LuaDLL.pua_pushstring(luaState, "sentry");
                LuaObject.pushObject(luaState, new LuaSentry(tableContent));
                LuaDLL.pua_rawset(luaState, -3);
                _activityOpenedStateTable.Add(ptr);
            }
        }

        static string GetTableContent(IntPtr luaState,int tableIndex)
        {
            int maxNum = 10;//最多枚举10个元素
            int count = 0;
    
            StringBuilder sb = new StringBuilder();
            sb.Append("table size:"+GetCurrentTableSize(luaState)+"\n");
    
            LuaDLL.pua_pushnil(luaState);
            while (LuaDLL.pua_next(luaState, -2) != 0)
            {
                if (count > maxNum)
                {
                    sb.Append("......");
                    LuaDLL.pua_pop(luaState,2);
                    break;
                }
                sb.Append(GetKeyDescription(luaState,-2)); //key
                sb.Append(":");
                sb.Append(GetValueDescription(luaState, -1)); //value
                sb.Append("\n");
                LuaDLL.pua_pop(luaState,1);
                count ++;
            }

            return sb.ToString();
        }

        static int GetCurrentTableSize(IntPtr luaState)
        {
            int size = 0;
            LuaDLL.pua_pushnil(luaState);
            while (LuaDLL.pua_next(luaState, -2) != 0)
            {
                size ++;
                LuaDLL.pua_pop(luaState,1);
            }

            return size;
        }

        //标记
        /*
            MARK_Tbl={
                pointer1 = true,
                pointer2 = true,
            }

        */
        static bool IsMarked(IntPtr dumpLuaState, IntPtr pointer)
        {
            RawGet(dumpLuaState, MARK, pointer);
            if (LuaDLL.pua_isnil(dumpLuaState, -1))
            {
                LuaDLL.pua_pop(dumpLuaState, 1);
                LuaDLL.pua_pushboolean(dumpLuaState, true);
                RawSet(dumpLuaState, MARK, pointer);
                return false;
            }
            else
            {
                LuaDLL.pua_pop(dumpLuaState, 1);
                return true;
            }
        }

        static void RawGet(IntPtr luaState, int index, IntPtr pointer)
        {
            index = GetAbsIndex(luaState, index);
            LuaDLL.pua_pushlightuserdata(luaState, pointer);
            LuaDLL.pua_rawget(luaState, index);
        }
        static void RawSet(IntPtr luaState, int index, IntPtr pointer)
        {
            index = GetAbsIndex(luaState, index);
            LuaDLL.pua_pushlightuserdata(luaState, pointer);
            LuaDLL.pua_insert(luaState, -2);
            LuaDLL.pua_rawset(luaState, index);
        }

        static int GetAbsIndex(IntPtr luaState, int index)
        {
            return index > 0 ? index : LuaDLL.pua_gettop(luaState) + index + 1;
        }

        //获取表键值的描述
        static string GetKeyDescription(IntPtr luaState, int index)
        {
            LuaTypes keyType = LuaDLL.pua_type(luaState, index);
            string keyDescription;
            switch (keyType)
            {
                case LuaTypes.LUA_TNIL:
                    keyDescription = "[nil]";
                    break;
                case LuaTypes.LUA_TBOOLEAN:
                    keyDescription = string.Format("[{0}]", LuaDLL.pua_toboolean(luaState, index));
                    break;
                case LuaTypes.LUA_TNUMBER:
                    keyDescription = string.Format("[{0}]", LuaDLL.pua_tonumber(luaState, index));
                    break;
                case LuaTypes.LUA_TSTRING:
                    keyDescription = LuaDLL.pua_tostring(luaState, index);
                    break;
                default:
                    keyDescription = string.Format("[{0}]", LuaDLL.pua_typenamestr(luaState, keyType));
                    break;
            }

            return keyDescription;
        }

        static string GetValueDescription(IntPtr luaState, int index)
        {
            LuaTypes valueType = LuaDLL.pua_type(luaState, index);
            string valueDescription;
            switch (valueType)
            {
                case LuaTypes.LUA_TNIL:
                    valueDescription = "nil";
                    break;
                case LuaTypes.LUA_TBOOLEAN:
                    valueDescription = string.Format("{0}", LuaDLL.pua_toboolean(luaState, index));
                    break;
                case LuaTypes.LUA_TNUMBER:
                    valueDescription = string.Format("{0}", LuaDLL.pua_tonumber(luaState, index));
                    break;
                case LuaTypes.LUA_TSTRING:
                    valueDescription = LuaDLL.pua_tostring(luaState, index);
                    break;
                default:
                    valueDescription = string.Format("{0}", LuaDLL.pua_typenamestr(luaState, valueType));
                    break;
            }

            return valueDescription;
        }

        //路由作用
        static void MarkObject(IntPtr luaState, IntPtr dumpLuaState, IntPtr parent, string description)
        {
            //扩展栈  因为要在栈上做递归，需要很多空间
            if (LuaDLL.pua_checkstack(luaState, LUA_MINSTACK) == false)
            {
                Debug.LogWarning(string.Format("不能保证｛0｝luaState 栈上还有空余的{1}个槽，有可能会出现栈OverFlow", luaState, LUA_MINSTACK));
            }

            LuaTypes objType = LuaDLL.pua_type(luaState, -1);
            switch (objType)
            {
                case LuaTypes.LUA_TTABLE:
                    MarkTable(luaState, dumpLuaState, parent, description);
                    break;
                case LuaTypes.LUA_TFUNCTION:
                    MarkFunction(luaState, dumpLuaState, parent, description);
                    break;
                default:
                    LuaDLL.pua_pop(luaState, 1);
                    break;
            }
        }

        //标记其environment表和upvalues
        static void MarkFunction(IntPtr luaState, IntPtr dumpLuaState, IntPtr parent, string description)
        {
            IntPtr functionPointer = ReadObject(luaState, dumpLuaState, parent, description);
            if (functionPointer == IntPtr.Zero)
            {
                return;
            }

            //不记录 c function
            if (LuaDLL.pua_iscfunction(luaState, -1))
            {
                LuaDLL.pua_pop(luaState, 1);
                return;
            }

            MarkFunctionEnvironment(luaState, dumpLuaState, functionPointer);

            int i;
            for (i = 1; i < int.MaxValue; i++)
            {
                IntPtr upvalueNamePointer = pua_getupvalue(luaState, -1, i);
                if (upvalueNamePointer == IntPtr.Zero)
                {
                    break;
                }

                string name = Marshal.PtrToStringAnsi(upvalueNamePointer);

                MarkObject(luaState, dumpLuaState, functionPointer, name != "" ? name : "[upvalue]");
            }

            //LuaDebug luaDebug = new LuaDebug();
            //IntPtr luaDebugPtr = Marshal.AllocHGlobal(Marshal.SizeOf(luaDebug));
            //Marshal.StructureToPtr(luaDebug, luaDebugPtr, true);
            //pua_getinfo(luaState, ">nS", luaDebugPtr);
            ////LuaDebug luaDebugInfo = (LuaDebug)Marshal.PtrToStructure(luaDebugPtr, typeof(LuaDebug));

            //这里可能会生成很多字符串,注意看内存
            //string functionDescrition = string.Format("{0}", Encoding.UTF8.GetString(luaDebugInfo.short_src).Substring(3));
            //functionDescrition += luaDebugInfo.linedefined.ToString();
            //LuaDLL.pua_pushstring(dumpLuaState, functionDescrition);
            //RawSet(dumpLuaState, SOURCE, functionPointer);
            //Marshal.FreeHGlobal(luaDebugPtr);

            LuaDLL.pua_pop(luaState, 1);
        }

        static void MarkFunctionEnvironment(IntPtr luaState, IntPtr dumpLuaState, IntPtr parent)
        {
            LuaDLL.pua_getfenv(luaState, -1);
            if (LuaDLL.pua_istable(luaState, -1))
            {
                MarkTable(luaState, dumpLuaState, parent, "[environment]");
            }
            else
            {
                LuaDLL.pua_pop(luaState, 1);
            }
        }

        static void MarkUserdata(IntPtr luaState, IntPtr dumpLuaState, IntPtr parent, string description)
        {
            IntPtr userdataPointer = ReadObject(luaState, dumpLuaState, parent, description);
            if (userdataPointer == IntPtr.Zero)
            {
                return;
            }

            if (LuaDLL.pua_getmetatable(luaState, -1) != 0)
            {
                MarkTable(luaState, dumpLuaState, userdataPointer, "[metatable]");
            }

            LuaDLL.pua_getfenv(luaState, -1);
            if (LuaDLL.pua_isnil(luaState, -1))
            {
                LuaDLL.pua_pop(luaState, 2);
            }
            else
            {
                MarkTable(luaState, dumpLuaState, userdataPointer, "[uservalue]");
                LuaDLL.pua_pop(luaState, 1);
            }
        }

        static void GenResult(IntPtr luaState, IntPtr dumpLuaState)
        {
            int count = 0;
            count += GetCount(dumpLuaState, TABLE);
            count += GetCount(dumpLuaState, FUNCTION);
            //count += GetCount(dumpLuaState, USERDATA);

            //把内容填充到LuaState中的新建表中
            LuaDLL.pua_createtable(luaState, 0, count);
            PushDescription(luaState, dumpLuaState, TABLE, "table");
            //PushDescription(luaState, dumpLuaState, USERDATA, "userdata");
            PushDescription(luaState, dumpLuaState, FUNCTION, "function");
        }

        private static int GetCount(IntPtr luaState, int index)
        {
            int count = 0;
            LuaDLL.pua_pushnil(luaState);
            while (LuaDLL.pua_next(luaState, index) != 0)
            {
                count++;
                LuaDLL.pua_pop(luaState, 1);
            }
            return count;
        }

        private static void PushDescription(IntPtr luaState, IntPtr dumpLuaState, int index, string typeName)
        {
            LuaDLL.pua_pushnil(dumpLuaState);
            while (LuaDLL.pua_next(dumpLuaState, index) != 0)
            {
                IntPtr key = LuaDLL.pua_touserdata(dumpLuaState, -2);

                StringBuilder sb = new StringBuilder(128);

                //获取function info 失败，暂时不去SOURCE中读取内容
                //if (index == FUNCTION)
                //{
                //    RawGet(dumpLuaState, SOURCE, key);
                //    if (LuaDLL.pua_isnil(dumpLuaState, -1))
                //    {
                //        sb.Append("this function has no details");
                //        sb.Append("\n");
                //    }
                //    else
                //    {
                //        sb.Append(LuaDLL.pua_tostring(dumpLuaState, -1));
                //        sb.Append("\n");
                //    }
                //    LuaDLL.pua_pop(dumpLuaState, 1);
                //}
                //else
                //{
                //    sb.Append(typeName);
                //    sb.Append("\n");
                //}

                sb.Append(typeName);
                sb.Append("\n");

                LuaDLL.pua_pushnil(dumpLuaState);
                while (LuaDLL.pua_next(dumpLuaState, -2) != 0)
                {
                    sb.Append(LuaDLL.pua_touserdata(dumpLuaState, -2).ToString());
                    sb.Append(":");
                    sb.Append(LuaDLL.pua_tostring(dumpLuaState, -1));
                    sb.Append("\n");
                    LuaDLL.pua_pop(dumpLuaState, 1);
                }
                LuaDLL.pua_pushstring(luaState, sb.ToString());
                RawSet(luaState, -2, key);
                LuaDLL.pua_pop(dumpLuaState, 1);
            }
        }

        private static Dictionary<IntPtr, string> GetSnapshot(IntPtr luaState)
        {
            //遍历栈上的信息表,转存到Dict中
            Dictionary<IntPtr, string> snapshot = new Dictionary<IntPtr, string>();
            LuaDLL.pua_pushnil(luaState);
            IntPtr pointer;
            while (LuaDLL.pua_next(luaState, -2) != 0)
            {
                pointer = LuaDLL.pua_touserdata(luaState, -2);
                if (snapshot.ContainsKey(pointer) == false)
                {
                    snapshot.Add(pointer, LuaDLL.pua_tostring(luaState, -1));
                }
                else
                {
                    Logger.LogError(string.Format("snap shot 中已经含有{0}项了", pointer));
                }
                LuaDLL.pua_pop(luaState, 1);
            }
            LuaDLL.pua_pop(luaState, 1);
            return snapshot;
        }

       
    }
}
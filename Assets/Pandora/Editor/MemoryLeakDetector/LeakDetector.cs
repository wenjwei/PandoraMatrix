using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace com.tencent.pandora.tools
{
    public class LeakDetector
    {
        private static LeakDetector _instance;

        //key:C#对象 value:对象id
        private Dictionary<object, int> _objMapWhenPanelOpened;
        private Dictionary<object, int> _objMapWhenPanelClosed;

        //key:对象id,作为_objMapWhenPanelClosed和_objDescriptionDict的连接桥梁，value:对象的描述信息，包括其类型、路径或描述信息
        private Dictionary<int, string> _objDescriptionDict = new Dictionary<int, string>();

        public static LeakDetector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LeakDetector();
                }
                return _instance;
            }
        }

        private LeakDetector()
        {

        }

        public void RecordWhenPanelOpened()
        {
            _objMapWhenPanelOpened = GetObjMap();
            FillObjDescriptionDict();
        }

        public void GC()
        {
            LuaGC();
            UnityEngine.Resources.UnloadUnusedAssets();
        }

        //一般情况下，关闭面板后，objMap中还存在的GameObject，Component，LuaSentry类型的对象，就是泄漏的对象
        //泄漏对象的描述信息在_targetObjectsDescriptionMap中查询
        public List<string> GetLeakInfoList()
        {
            _objMapWhenPanelClosed = GetObjMap();
            List<string> leakInfoList = new List<string>();
            string description = "";
            foreach (var item in _objMapWhenPanelClosed)
            {
                if (_objDescriptionDict.TryGetValue(item.Value, out description))
                {
                    leakInfoList.Add(description);
                }
            }
            return leakInfoList;
        }

        private Dictionary<object, int> GetObjMap()
        {
            IntPtr luaStatePointer = GetLuaStatePointer();
            if (luaStatePointer == IntPtr.Zero)
            {
                return new Dictionary<object, int>();
            }
            ObjectCache objectCache = ObjectCache.get(luaStatePointer);
            Type objectCacheType = objectCache.GetType();
            FieldInfo objMapField = objectCacheType.GetField("objMap", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Dictionary<object, int> objMap = objMapField.GetValue(objectCache) as Dictionary<object, int>;
            return objMap;
        }

        private IntPtr GetLuaStatePointer()
        {
            GameObject sluaSvrGameObject = GameObject.Find("LuaStateProxy_0");
            if (sluaSvrGameObject == null)
            {
                DisplayWarningDialog("lua 虚拟机未在运行中，请运行游戏工程后做此操作！");
                return IntPtr.Zero;
            }
            return sluaSvrGameObject.GetComponent<LuaSvrGameObject>().state.L;
        }

        private void FillObjDescriptionDict()
        {
            _objDescriptionDict.Clear();
            GameObject go = null;
            Transform trans = null;
            Component component = null;
            LuaSentry sentry = null;
            string description = "";
            try
            {
                foreach (var item in _objMapWhenPanelOpened)
                {
                    description = "";
                    if (item.Key == null)
                    {
                        continue;
                    }
                    if (item.Key.ToString() == "null") 
                    {
                        description = string.Format("[C# null]: {0}\n", "该对象在第一次记录时已被销毁，但lua层未释放，无法获取该对象的详细信息");
                    }
                    else if (item.Key is GameObject)
                    {
                        go = item.Key as GameObject;
                        description = string.Format("[C# GameObject]: {0}\n type:{1}\t Path In Hierarchy: {2}", item.Key, item.Key.GetType(), GetTransformPath(go.transform));
                    }
                    else if (item.Key is Transform)
                    {
                        trans = item.Key as Transform;
                        description = string.Format("[C# Transform]: {0}\n type:{1}\t Path In Hierarchy: {2}", item.Key, item.Key.GetType(), GetTransformPath(trans));
                    }
                    else if (item.Key is Component)
                    {
                        component = item.Key as Component;
                        description = string.Format("[C# Component]: {0}\n type:{1}\t Path In Hierarchy: {2}", item.Key, item.Key.GetType(), GetTransformPath(component.transform));
                    }
                    else if (item.Key is LuaSentry)
                    {
                        sentry = item.Key as LuaSentry;
                        description = string.Format("{0}\n", sentry.ToString());
                    }
                    else
                    {
                        description = string.Format("[C# Other]: {0}\n type:{1}",item.Key,item.Key.GetType());
                    }

                    if (string.IsNullOrEmpty(description) == false)
                    {
                        _objDescriptionDict[item.Value] = description;
                    }
                }
            }
            catch (Exception e)
            {
                //当C#层的GameObject/Component 对象被销毁,而lua层未释放对它的引用时,点击'打开活动面板后-记录'按钮会触发此异常.
                //请在正确的时机点击'打开活动面板后-记录'按钮,以记录正确的C# 对象信息
                Debug.LogWarning(e.Message + "\nStackTrace:\n" + e.StackTrace);
            }
        }

        private void LuaGC()
        {
            IntPtr luaStatePointer = GetLuaStatePointer();
            if (luaStatePointer == IntPtr.Zero)
            {
                return;
            }
            LuaDLL.pua_gc((IntPtr)luaStatePointer, LuaGCOptions.LUA_GCCOLLECT, 0);
        }

        //获取在Hierarchy下的路径
        private string GetTransformPath(Transform trans)
        {
            if (trans == null)
            {
                return "";
            }
            Transform parentTrans = trans;
            StringBuilder sb = new StringBuilder();
            while (parentTrans != null)
            {
                sb.Insert(0, parentTrans.name);
                sb.Insert(0, "/");
                parentTrans = parentTrans.parent;
            }
            string path = sb.ToString(1, sb.Length - 1);
            return path;
        }

        public static void DisplayWarningDialog(string message, string title = "")
        {
            EditorUtility.DisplayDialog(title, message, "我知道了");
        }
    }
}
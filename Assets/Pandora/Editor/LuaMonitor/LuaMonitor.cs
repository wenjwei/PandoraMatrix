
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.tencent.pandora.tools
{
    public class LuaMonitor
    {
        public static string Template = @"
-- Lua监控代码文件，打包自动生成 --

require ""Common"";

local whitelist = {
    -- 将日志控制台告警信息贴在此白名单列表

    -- !!!活动层全局变量名，必须带上活动名前缀，否则将报错!!!
    -- 如Pop活动全局变量PopActionInfo，PopSettings

    --[[ ""Pop set global var PopActionInfo, type: table,
    敏感操作：全局变量赋值，如果已知风险，可将 PopActionInfo='table', 加入PopMonitor.lua.bytes白名单列表]]
    -- PopActionInfo='table',  示例：已知风险的行为将上述告警中此信息贴在whitelist中，否则修改代码，移除不必要的全局变量操作，或者更新全局变量名
    -- PopI18N='table',
    -- PopSettings='table',
    -- PopStats='table',
    -- PopRequestAssembler='table',

    -- to-add

};

LuaMonitor.Monitor(""{ActionName}"");
LuaMonitor.AddWhitelist(""{ActionName}"", whitelist);";

        public static string EntryTemplate = @"
local WINDOWS_EDITOR = 7;
if UnityEngine.Application.platform == WINDOWS_EDITOR then
  require ""./{ActionName}Monitor"";
end";

        public static void Build(string actionName)
        {
            if (actionName.Equals("Frame"))
                return;

            var monitorPath = string.Format("Assets/Actions/Resources/{0}/Lua/{1}Monitor.lua.bytes", actionName, actionName);

            var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(monitorPath);
            if (ta != null)
                return;
            
            File.WriteAllText(monitorPath.Replace("Assets", Application.dataPath), Template.Replace("{ActionName}", actionName));

            var entryPath = string.Format("Assets/Actions/Resources/{0}/Lua/{1}.lua.bytes", actionName, actionName);
            var taEntry = AssetDatabase.LoadAssetAtPath<TextAsset>(entryPath);
            var content = string.Format("{0}\n\n{1}", EntryTemplate.Replace("{ActionName}", actionName), taEntry.text);
            File.WriteAllText(entryPath.Replace("Assets", Application.dataPath), content);

            AssetDatabase.Refresh();
        }
    }
}
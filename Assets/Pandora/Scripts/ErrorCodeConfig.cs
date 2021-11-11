
using UnityEngine;
using System.Collections;

namespace com.tencent.pandora
{
    public class ErrorCodeConfig
    {
        public static void Initialize()
        {
            #region 累加类型上报代码
            /// <summary>
            /// Assetbundle资源解包失败
            /// </summary>
            ErrorCode.ASSET_PARSE_FAILED = 1747717;
            /// <summary>
            /// 资源Md5码校验失败
            /// </summary>
            ErrorCode.MD5_VALIDATE_FAILED = 1747718;
            /// <summary>
            /// 文件写入本地失败
            /// </summary>
#if UNITY_IOS
			ErrorCode.FILE_WRITE_FAILED = 1747720;
#else
            ErrorCode.FILE_WRITE_FAILED = 1747719;
#endif
            /// <summary>
            /// 资源加载失败
            /// </summary>
            ErrorCode.ASSET_LOAD_FAILED = 1747721;
            /// <summary>
            /// 资源Meta文件读取失败
            /// </summary>
            ErrorCode.META_READ_FAILED = 1747722;
            /// <summary>
            /// 资源Meta文件更新失败
            /// </summary>
#if UNITY_IOS
			ErrorCode.META_WRITE_FAILED = 1747724;
#else
            ErrorCode.META_WRITE_FAILED = 1747723;
#endif
            /// <summary>
            /// 访问失败且超过最大重试次数
            /// </summary>
#if UNITY_IOS
			ErrorCode.CGI_TIMEOUT = 1747726;
#else
            ErrorCode.CGI_TIMEOUT = 1747725;
#endif
            /// <summary>
            /// Lua执行游戏传递的消息失败
            /// </summary>
            ErrorCode.GAME_2_PANDORA_EXCEPTION = 1747727;
            /// <summary>
            /// 执行Lua回调发生异常
            /// </summary>
            ErrorCode.EXECUTE_LUA_CALLBACK_EXCEPTION = 1747728;
            /// <summary>
            /// Lua脚本执行发生异常
            /// </summary>
            ErrorCode.LUA_SCRIPT_EXCEPTION = 1747729;
            /// <summary>
            /// Lua脚本文件解析发生异常
            /// </summary>
            ErrorCode.LUA_DO_FILE_EXCEPTION = 1747730;
            /// <summary>
            /// 开始执行模块入口Lua文件
            /// </summary>
            ErrorCode.EXECUTE_ENTRY_LUA = 1747731;
            /// <summary>
            /// 游戏执行Lua消息发生异常
            /// </summary>
            ErrorCode.PANDORA_2_GAME_EXCEPTION = 1747732;
            /// <summary>
            /// 创建面板时已存在同名面板
            /// </summary>
            ErrorCode.SAME_PANEL_EXISTS = 1747733;
            /// <summary>
            /// 面板的父节点不存在
            /// </summary>
            ErrorCode.PANEL_PARENT_INEXISTS = 1747734;
            /// <summary>
            /// Cookie写入失败
            /// </summary>
            ErrorCode.COOKIE_WRITE_FAILED = 1747735;
            /// <summary>
            /// Cookie读取失败
            /// </summary>
            ErrorCode.COOKIE_READ_FAILED = 1747736;
            /// <summary>
            /// 删除文件失败
            /// </summary>
            ErrorCode.DELETE_FILE_FAILED = 1747737;
            /// <summary>
            /// 记录用户主动重连请求
            /// </summary>
            ErrorCode.START_RELOAD = 1747738;
            /// <summary>
            /// CGI内容错误
            /// </summary>
            ErrorCode.CGI_CONTENT_ERROR = 1747739;
            /// <summary>
            /// 热更新文件冲突统计
            /// </summary>
            ErrorCode.ASSETBUNDLE_CONFLICT = 0;
            #endregion

            #region 字符型告警上报代码
            /// <summary>
            /// Lua脚本报错详情，使用tnm2字符型告警内容上报
            /// </summary>
            ErrorCode.LUA_SCRIPT_EXCEPTION_DETAIL = 1747740;
            /// <summary>
            /// CGI超时错误详情
            /// </summary>
            ErrorCode.CGI_TIMEOUT_DETAIL = 1747741;

            /// <summary>
            /// 游戏处理Pandora消息回调函数异常详情
            /// </summary>
            ErrorCode.PANDORA_2_GAME_EXCEPTION_DETAIL = 1747742;
            /// <summary>
            /// CGI内容出错详情
            /// </summary>
            ErrorCode.CGI_CONTENT_ERROR_DETAIL = 1747743;
            /// <summary>
            /// 热更新文件冲突统计详情
            /// </summary>
            ErrorCode.ASSETBUNDLE_CONFLICT_DETAIL = 0;
            #endregion
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.tencent.pandora
{
    public class ErrorCode
    {
        /// <summary>
        /// tnm2累加类型
        /// </summary>
        public const int TNM2_TYPE_ACCUMULATION = 0;
        /// <summary>
        /// tnm2平均值类型
        /// </summary>
        public const int TNM2_TYPE_AVERAGE = 1;
        /// <summary>
        /// tnm2字符型告警类型
        /// </summary>
        public const int TNM2_TYPE_LITERALS = 2;


        #region 累加类型上报代码
        /// <summary>
        /// Assetbundle资源解包失败
        /// </summary>
        public static int ASSET_PARSE_FAILED = 0;
        /// <summary>
        /// 资源Md5码校验失败
        /// </summary>
        public static int MD5_VALIDATE_FAILED = 0;
        /// <summary>
        /// 文件写入本地失败
        /// </summary>
        public static int FILE_WRITE_FAILED = 0;
        /// <summary>
        /// 资源加载失败
        /// </summary>
        public static int ASSET_LOAD_FAILED = 0;
        /// <summary>
        /// 资源Meta文件读取失败
        /// </summary>
        public static int META_READ_FAILED = 0;
        /// <summary>
        /// 资源Meta文件更新失败
        /// </summary>
        public static int META_WRITE_FAILED = 0;
        /// <summary>
        /// 访问失败且超过最大重试次数
        /// </summary>
        public static int CGI_TIMEOUT = 0;
        /// <summary>
        /// Lua执行游戏传递的消息失败
        /// </summary>
        public static int GAME_2_PANDORA_EXCEPTION = 0;
        /// <summary>
        /// 执行Lua回调发生异常
        /// </summary>
        public static int EXECUTE_LUA_CALLBACK_EXCEPTION = 0;
        /// <summary>
        /// Lua脚本执行发生异常
        /// </summary>
        public static int LUA_SCRIPT_EXCEPTION = 0;
        /// <summary>
        /// Lua脚本文件解析发生异常
        /// </summary>
        public static int LUA_DO_FILE_EXCEPTION = 0;
        /// <summary>
        /// 开始执行模块入口Lua文件
        /// </summary>
        public static int EXECUTE_ENTRY_LUA = 0;
        /// <summary>
        /// 游戏执行Lua消息发生异常
        /// </summary>
        public static int PANDORA_2_GAME_EXCEPTION = 0;
        /// <summary>
        /// 创建面板时已存在同名面板
        /// </summary>
        public static int SAME_PANEL_EXISTS = 0;
        /// <summary>
        /// 面板的父节点不存在
        /// </summary>
        public static int PANEL_PARENT_INEXISTS = 0;
        /// <summary>
        /// Cookie写入失败
        /// </summary>
        public static int COOKIE_WRITE_FAILED = 0;
        /// <summary>
        /// Cookie读取失败
        /// </summary>
        public static int COOKIE_READ_FAILED = 0;
        /// <summary>
        /// 删除文件失败
        /// </summary>
        public static int DELETE_FILE_FAILED = 0;
        /// <summary>
        /// 记录用户主动重连请求
        /// </summary>
        public static int START_RELOAD = 0;
        /// <summary>
        /// CGI内容错误
        /// </summary>
        public static int CGI_CONTENT_ERROR = 0;
        /// <summary>
        /// 热更新文件冲突统计
        /// </summary>
        public static int ASSETBUNDLE_CONFLICT = 0;
        #endregion

        #region 字符型告警上报代码
        /// <summary>
        /// Lua脚本报错详情，使用tnm2字符型告警内容上报
        /// </summary>
        public static int LUA_SCRIPT_EXCEPTION_DETAIL = 0;
        /// <summary>
        /// CGI超时错误详情
        /// </summary>
        public static int CGI_TIMEOUT_DETAIL = 0;

        /// <summary>
        /// 游戏处理Pandora消息回调函数异常详情
        /// </summary>
        public static int PANDORA_2_GAME_EXCEPTION_DETAIL = 0;
        /// <summary>
        /// CGI内容出错详情
        /// </summary>
        public static int CGI_CONTENT_ERROR_DETAIL = 0;
        /// <summary>
        /// 热更新文件冲突统计详情
        /// </summary>
        public static int ASSETBUNDLE_CONFLICT_DETAIL = 0;
        #endregion
    }
}

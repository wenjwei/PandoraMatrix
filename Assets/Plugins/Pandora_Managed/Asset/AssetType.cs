using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.tencent.pandora
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum AssetType
    {
        Lua,
        /// <summary>
        /// 普通Prefab资源
        /// </summary>
        Prefab,
        /// <summary>
        /// 普通图片资源
        /// </summary>
        Image,
        /// <summary>
        /// 普通Assetbunle资源
        /// </summary>
        Assetbundle,
        /// <summary>
        /// 普通文本资源，Json或Lua文件
        /// </summary>
        Text,
        /// <summary>
        /// 玩家头像资源，可能为GIF格式
        /// </summary>
        Portrait,
        /// <summary>
        /// 音频资源
        /// </summary>
        Audio,
		/// <summary>
		/// 二进制资源
		/// </summary>
		Binary,
        /// <summary>
        /// 原始二进制数据
        /// </summary>
        RawData,
        /// <summary>
        /// 使用Zip打包的文件
        /// </summary>
        Zip,
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tencent.pandora
{
    /// <summary>
    /// Console 子窗口接口
    /// </summary>
    interface IConsoleChildWindow
    {
        /// <summary>
        /// 初始化相关配置
        /// </summary>
        /// <param name="ratio">窗口缩放比</param>
        /// <param name="windowCenter">窗口的中心点</param>
        /// <param name="windowMaxSize">窗口最大尺寸</param>
        void Init(float ratio, Vector2 windowCenter, Vector2 windowMaxSize);

        /// <summary>
        /// 绘制
        /// </summary>
        void Draw();
    }
}

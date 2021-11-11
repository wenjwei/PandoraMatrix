using UnityEngine;

namespace com.tencent.pandora.tools
{
    /// <summary>
    /// Pandora资源打包的一些设置
    /// </summary>
    internal class BuilderSetting
    {
        //设置PixUI资源的打包格式
        public static bool UseZipResForPixUI = true;
        //活动资源根路径
        public const string ROOT_PATH = "Assets/Actions/Resources";
        //活动Prefab资源路径模版
        public const string PREFAB_PATH_TEMPLATE = "Assets/Actions/Resources/{0}/Prefabs";
        //活动Lua资源路径模版
        public const string LUA_PATH_TEMPLATE = "Assets/Actions/Resources/{0}/Lua";
        //编译后32位lua资源路径
        public const string LUA_32_PATH_TEMPLATE = "Assets/Actions/Resources/{0}/Lua32";
        //编译后64位lua资源路径
        public const string LUA_64_PATH_TEMPLATE = "Assets/Actions/Resources/{0}/Lua64";
        //二进制资源路径
        public const string BIN_PATH_TEMPLATE = "Assets/Actions/Resources/{0}/Bin";
        //临时二进制资源路径
        public const string TEMP_BIN_PATH_TEMPLATE = "Assets/Actions/Resources/{0}/TempBin";
        //AssetBundle资源输出目录路径
        public static string STREAM_ASSET_PATH = Application.streamingAssetsPath + "/Pandora";
        //打包选项保存路径
        public const string OPTIONS_FOLDER = "Assets/Pandora/Editor/Options";
        //打包选项保存文件夹
        public const string OPTIONS_PATH = OPTIONS_FOLDER + "/BuildOptions.txt";
    }
}

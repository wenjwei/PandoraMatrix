using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_com_tencent_pandora_CSharpInterface : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.constructor");
			#endif
			com.tencent.pandora.CSharpInterface o;
			o=new com.tencent.pandora.CSharpInterface();
			pushValue(l,true);
			pushValue(l,o);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int NowMilliseconds_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.NowMilliseconds");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.NowMilliseconds();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int UnloadAssets_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.UnloadAssets");
			#endif
			UnityEngine.Object a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.UnloadAssets(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadGameObjectByResources_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadGameObjectByResources");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.LoadGameObjectByResources(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int PlaySound_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.PlaySound");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.PlaySound(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Report_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.Report");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.Int32 a2;
			checkType(l,2,out a2);
			System.Int32 a3;
			checkType(l,3,out a3);
			com.tencent.pandora.CSharpInterface.Report(a1,a2,a3);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ReportError_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ReportError");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.Int32 a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.ReportError(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ReportLuaError_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ReportLuaError");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.ReportLuaError(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CloneAndAddToParent_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.CloneAndAddToParent");
			#endif
			UnityEngine.GameObject a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			UnityEngine.GameObject a3;
			checkType(l,3,out a3);
			var ret=com.tencent.pandora.CSharpInterface.CloneAndAddToParent(a1,a2,a3);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int SetParent_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.SetParent");
			#endif
			UnityEngine.GameObject a1;
			checkType(l,1,out a1);
			UnityEngine.GameObject a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.SetParent(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int SetPanelParent_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.SetPanelParent");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			UnityEngine.GameObject a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.SetPanelParent(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int IsDebug_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.IsDebug");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.IsDebug();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetPlatformDescription_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetPlatformDescription");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.GetPlatformDescription();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetSDKVersion_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetSDKVersion");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.GetSDKVersion();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int UnloadUnusedAssets_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.UnloadUnusedAssets");
			#endif
			com.tencent.pandora.CSharpInterface.UnloadUnusedAssets();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int WriteCookie_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.WriteCookie");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			var ret=com.tencent.pandora.CSharpInterface.WriteCookie(a1,a2);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ReadCookie_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ReadCookie");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.ReadCookie(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetUserData_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetUserData");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.GetUserData();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int RefreshUserDataTokens_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.RefreshUserDataTokens");
			#endif
			com.tencent.pandora.CSharpInterface.RefreshUserDataTokens();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetRemoteConfig_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetRemoteConfig");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.GetRemoteConfig();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ShowPortrait_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ShowPortrait");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			UnityEngine.GameObject a3;
			checkType(l,3,out a3);
			System.Boolean a4;
			checkType(l,4,out a4);
			System.UInt32 a5;
			checkType(l,5,out a5);
			com.tencent.pandora.CSharpInterface.ShowPortrait(a1,a2,a3,a4,a5);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ShowImage_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ShowImage");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			UnityEngine.GameObject a3;
			checkType(l,3,out a3);
			System.Boolean a4;
			checkType(l,4,out a4);
			System.UInt32 a5;
			checkType(l,5,out a5);
			com.tencent.pandora.CSharpInterface.ShowImage(a1,a2,a3,a4,a5);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CacheImage_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.CacheImage");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.CacheImage(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int IsImageCached_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.IsImageCached");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.IsImageCached(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadText_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadText");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.UInt32 a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.LoadText(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadAssetBundle_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadAssetBundle");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.UInt32 a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.LoadAssetBundle(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadGameObject_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadGameObject");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.Boolean a2;
			checkType(l,2,out a2);
			com.tencent.pandora.LuaFunction a3;
			checkType(l,3,out a3);
			com.tencent.pandora.CSharpInterface.LoadGameObject(a1,a2,a3);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadImage_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadImage");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.Boolean a2;
			checkType(l,2,out a2);
			System.UInt32 a3;
			checkType(l,3,out a3);
			com.tencent.pandora.CSharpInterface.LoadImage(a1,a2,a3);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetAsset_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetAsset");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.GetAsset(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CacheAsset_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.CacheAsset");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.LuaFunction a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.CacheAsset(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int IsAssetCached_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.IsAssetCached");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.IsAssetCached(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DeleteCacheAsset_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.DeleteCacheAsset");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.DeleteCacheAsset(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ReleaseAsset_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ReleaseAsset");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.ReleaseAsset(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ForceDeleteZeroReferenceAsset_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ForceDeleteZeroReferenceAsset");
			#endif
			com.tencent.pandora.CSharpInterface.ForceDeleteZeroReferenceAsset();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadWww_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadWww");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			System.Boolean a3;
			checkType(l,3,out a3);
			System.UInt32 a4;
			checkType(l,4,out a4);
			com.tencent.pandora.CSharpInterface.LoadWww(a1,a2,a3,a4);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LoadWwwWithBinaryData_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LoadWwwWithBinaryData");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			System.Byte[] a3;
			checkArray(l,3,out a3);
			System.Boolean a4;
			checkType(l,4,out a4);
			System.UInt32 a5;
			checkType(l,5,out a5);
			com.tencent.pandora.CSharpInterface.LoadWwwWithBinaryData(a1,a2,a3,a4,a5);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CreatePanel_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.CreatePanel");
			#endif
			System.UInt32 a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.CreatePanel(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetPanel_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetPanel");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.GetPanel(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int HidePanel_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.HidePanel");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.HidePanel(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int HideAllPanel_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.HideAllPanel");
			#endif
			com.tencent.pandora.CSharpInterface.HideAllPanel();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DestroyPanel_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.DestroyPanel");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.DestroyPanel(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int DestroyAllPanel_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.DestroyAllPanel");
			#endif
			com.tencent.pandora.CSharpInterface.DestroyAllPanel();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetTotalSwitch_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetTotalSwitch");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.GetTotalSwitch();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetFunctionSwitch_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetFunctionSwitch");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			var ret=com.tencent.pandora.CSharpInterface.GetFunctionSwitch(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int LuaCallGame_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.LuaCallGame");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			com.tencent.pandora.CSharpInterface.LuaCallGame(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CallBroker_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.CallBroker");
			#endif
			System.UInt32 a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			System.Int32 a3;
			checkType(l,3,out a3);
			com.tencent.pandora.CSharpInterface.CallBroker(a1,a2,a3);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CallAtm_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.CallAtm");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.Int32 a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.CallAtm(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ShowItem_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ShowItem");
			#endif
			UnityEngine.GameObject a1;
			checkType(l,1,out a1);
			System.UInt32 a2;
			checkType(l,2,out a2);
			System.UInt32 a3;
			checkType(l,3,out a3);
			com.tencent.pandora.CSharpInterface.ShowItem(a1,a2,a3);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ShowItemIcon_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ShowItemIcon");
			#endif
			UnityEngine.GameObject a1;
			checkType(l,1,out a1);
			System.UInt32 a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.ShowItemIcon(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ShowItemTips_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.ShowItemTips");
			#endif
			UnityEngine.GameObject a1;
			checkType(l,1,out a1);
			System.UInt32 a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.ShowItemTips(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetCurrency_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.GetCurrency");
			#endif
			var ret=com.tencent.pandora.CSharpInterface.GetCurrency();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Jump_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.Jump");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			com.tencent.pandora.CSharpInterface.Jump(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_IsIOSPlatform(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.IsIOSPlatform");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.CSharpInterface.IsIOSPlatform);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_PauseDownloading(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.PauseDownloading");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.CSharpInterface.PauseDownloading);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_PauseDownloading(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.PauseDownloading");
			#endif
			bool v;
			checkType(l,2,out v);
			com.tencent.pandora.CSharpInterface.PauseDownloading=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int get_PauseSocketSending(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.PauseSocketSending");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.CSharpInterface.PauseSocketSending);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int set_PauseSocketSending(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.CSharpInterface.PauseSocketSending");
			#endif
			bool v;
			checkType(l,2,out v);
			com.tencent.pandora.CSharpInterface.PauseSocketSending=v;
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if PANDORA_PROFILE
		finally {
			endSample();
		}
		#endif
	}
	static public void reg(IntPtr l) {
		getTypeTable(l,"com.tencent.pandora.CSharpInterface");
		addMember(l,NowMilliseconds_s);
		addMember(l,UnloadAssets_s);
		addMember(l,LoadGameObjectByResources_s);
		addMember(l,PlaySound_s);
		addMember(l,Report_s);
		addMember(l,ReportError_s);
		addMember(l,ReportLuaError_s);
		addMember(l,CloneAndAddToParent_s);
		addMember(l,SetParent_s);
		addMember(l,SetPanelParent_s);
		addMember(l,IsDebug_s);
		addMember(l,GetPlatformDescription_s);
		addMember(l,GetSDKVersion_s);
		addMember(l,UnloadUnusedAssets_s);
		addMember(l,WriteCookie_s);
		addMember(l,ReadCookie_s);
		addMember(l,GetUserData_s);
		addMember(l,RefreshUserDataTokens_s);
		addMember(l,GetRemoteConfig_s);
		addMember(l,ShowPortrait_s);
		addMember(l,ShowImage_s);
		addMember(l,CacheImage_s);
		addMember(l,IsImageCached_s);
		addMember(l,LoadText_s);
		addMember(l,LoadAssetBundle_s);
		addMember(l,LoadGameObject_s);
		addMember(l,LoadImage_s);
		addMember(l,GetAsset_s);
		addMember(l,CacheAsset_s);
		addMember(l,IsAssetCached_s);
		addMember(l,DeleteCacheAsset_s);
		addMember(l,ReleaseAsset_s);
		addMember(l,ForceDeleteZeroReferenceAsset_s);
		addMember(l,LoadWww_s);
		addMember(l,LoadWwwWithBinaryData_s);
		addMember(l,CreatePanel_s);
		addMember(l,GetPanel_s);
		addMember(l,HidePanel_s);
		addMember(l,HideAllPanel_s);
		addMember(l,DestroyPanel_s);
		addMember(l,DestroyAllPanel_s);
		addMember(l,GetTotalSwitch_s);
		addMember(l,GetFunctionSwitch_s);
		addMember(l,LuaCallGame_s);
		addMember(l,CallBroker_s);
		addMember(l,CallAtm_s);
		addMember(l,ShowItem_s);
		addMember(l,ShowItemIcon_s);
		addMember(l,ShowItemTips_s);
		addMember(l,GetCurrency_s);
		addMember(l,Jump_s);
		addMember(l,"IsIOSPlatform",get_IsIOSPlatform,null,false);
		addMember(l,"PauseDownloading",get_PauseDownloading,set_PauseDownloading,false);
		addMember(l,"PauseSocketSending",get_PauseSocketSending,set_PauseSocketSending,false);
		createTypeMetatable(l,constructor, typeof(com.tencent.pandora.CSharpInterface));
	}
}

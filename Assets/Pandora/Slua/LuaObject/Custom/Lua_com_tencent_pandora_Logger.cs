using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_com_tencent_pandora_Logger : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Log_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.Log");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			com.tencent.pandora.Logger.Log(a1,a2);
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
	static public int LogInfo_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.LogInfo");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			com.tencent.pandora.Logger.LogInfo(a1,a2);
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
	static public int LogWarning_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.LogWarning");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			com.tencent.pandora.Logger.LogWarning(a1,a2);
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
	static public int LogError_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.LogError");
			#endif
			System.String a1;
			checkType(l,1,out a1);
			System.String a2;
			checkType(l,2,out a2);
			com.tencent.pandora.Logger.LogError(a1,a2);
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
	static public int SetLogConfig_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.SetLogConfig");
			#endif
			System.Collections.Generic.Dictionary<System.String,com.tencent.pandora.LogConfig> a1;
			checkType(l,1,out a1);
			System.Boolean a2;
			checkType(l,2,out a2);
			System.String a3;
			checkType(l,3,out a3);
			System.Boolean a4;
			checkType(l,4,out a4);
			System.Int32 a5;
			checkType(l,5,out a5);
			System.Boolean a6;
			checkType(l,6,out a6);
			System.Int32 a7;
			checkType(l,7,out a7);
			com.tencent.pandora.Logger.SetLogConfig(a1,a2,a3,a4,a5,a6,a7);
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
	static public int Dispose_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.Dispose");
			#endif
			com.tencent.pandora.Logger.Dispose();
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
	static public int get_ERROR(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.ERROR");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.Logger.ERROR);
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
	static public int get_WARNING(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.WARNING");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.Logger.WARNING);
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
	static public int get_INFO(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.INFO");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.Logger.INFO);
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
	static public int get_DEBUG(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.DEBUG");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.Logger.DEBUG);
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
	static public int set_HandleLog(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.HandleLog");
			#endif
			System.Action<System.String,System.String,System.Int32> v;
			int op=LuaDelegation.checkDelegate(l,2,out v);
			if(op==0) com.tencent.pandora.Logger.HandleLog=v;
			else if(op==1) com.tencent.pandora.Logger.HandleLog+=v;
			else if(op==2) com.tencent.pandora.Logger.HandleLog-=v;
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
	static public int get_Enable(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.Enable");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.Logger.Enable);
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
	static public int set_Enable(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.Enable");
			#endif
			System.Boolean v;
			checkType(l,2,out v);
			com.tencent.pandora.Logger.Enable=v;
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
	static public int get_LogLevel(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.LogLevel");
			#endif
			pushValue(l,true);
			pushValue(l,com.tencent.pandora.Logger.LogLevel);
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
	static public int set_LogLevel(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.Logger.LogLevel");
			#endif
			System.Int32 v;
			checkType(l,2,out v);
			com.tencent.pandora.Logger.LogLevel=v;
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
		getTypeTable(l,"com.tencent.pandora.Logger");
		addMember(l,Log_s);
		addMember(l,LogInfo_s);
		addMember(l,LogWarning_s);
		addMember(l,LogError_s);
		addMember(l,SetLogConfig_s);
		addMember(l,Dispose_s);
		addMember(l,"ERROR",get_ERROR,null,false);
		addMember(l,"WARNING",get_WARNING,null,false);
		addMember(l,"INFO",get_INFO,null,false);
		addMember(l,"DEBUG",get_DEBUG,null,false);
		addMember(l,"HandleLog",null,set_HandleLog,false);
		addMember(l,"Enable",get_Enable,set_Enable,false);
		addMember(l,"LogLevel",get_LogLevel,set_LogLevel,false);
		createTypeMetatable(l,null, typeof(com.tencent.pandora.Logger),typeof(UnityEngine.MonoBehaviour));
	}
}

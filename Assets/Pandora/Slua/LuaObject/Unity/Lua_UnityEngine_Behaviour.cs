using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_Behaviour : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Behaviour.constructor");
			#endif
			UnityEngine.Behaviour o;
			o=new UnityEngine.Behaviour();
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
	static public int get_enabled(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Behaviour.enabled");
			#endif
			UnityEngine.Behaviour self=(UnityEngine.Behaviour)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.enabled);
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
	static public int set_enabled(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Behaviour.enabled");
			#endif
			UnityEngine.Behaviour self=(UnityEngine.Behaviour)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.enabled=v;
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
	static public int get_isActiveAndEnabled(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Behaviour.isActiveAndEnabled");
			#endif
			UnityEngine.Behaviour self=(UnityEngine.Behaviour)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.isActiveAndEnabled);
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
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Behaviour");
		addMember(l,"enabled",get_enabled,set_enabled,true);
		addMember(l,"isActiveAndEnabled",get_isActiveAndEnabled,null,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Behaviour),typeof(UnityEngine.Component));
	}
}

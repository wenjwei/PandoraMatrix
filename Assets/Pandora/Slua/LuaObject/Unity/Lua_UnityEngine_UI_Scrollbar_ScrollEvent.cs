using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_Scrollbar_ScrollEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.Scrollbar.ScrollEvent.constructor");
			#endif
			UnityEngine.UI.Scrollbar.ScrollEvent o;
			o=new UnityEngine.UI.Scrollbar.ScrollEvent();
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
	static public void reg(IntPtr l) {
		LuaUnityEvent_float.reg(l);
		getTypeTable(l,"UnityEngine.UI.Scrollbar.ScrollEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.Scrollbar.ScrollEvent),typeof(LuaUnityEvent_float));
	}
}

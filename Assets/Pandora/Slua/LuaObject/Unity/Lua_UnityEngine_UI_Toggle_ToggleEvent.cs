using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_Toggle_ToggleEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.Toggle.ToggleEvent.constructor");
			#endif
			UnityEngine.UI.Toggle.ToggleEvent o;
			o=new UnityEngine.UI.Toggle.ToggleEvent();
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
		LuaUnityEvent_bool.reg(l);
		getTypeTable(l,"UnityEngine.UI.Toggle.ToggleEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.Toggle.ToggleEvent),typeof(LuaUnityEvent_bool));
	}
}

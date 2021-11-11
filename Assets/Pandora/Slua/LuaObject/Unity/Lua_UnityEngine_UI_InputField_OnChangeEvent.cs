using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_InputField_OnChangeEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.InputField.OnChangeEvent.constructor");
			#endif
			UnityEngine.UI.InputField.OnChangeEvent o;
			o=new UnityEngine.UI.InputField.OnChangeEvent();
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
		LuaUnityEvent_string.reg(l);
		getTypeTable(l,"UnityEngine.UI.InputField.OnChangeEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.InputField.OnChangeEvent),typeof(LuaUnityEvent_string));
	}
}

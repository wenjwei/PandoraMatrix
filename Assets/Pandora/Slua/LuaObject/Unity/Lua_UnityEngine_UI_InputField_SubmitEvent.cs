using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_InputField_SubmitEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.InputField.SubmitEvent.constructor");
			#endif
			UnityEngine.UI.InputField.SubmitEvent o;
			o=new UnityEngine.UI.InputField.SubmitEvent();
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
		getTypeTable(l,"UnityEngine.UI.InputField.SubmitEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.InputField.SubmitEvent),typeof(LuaUnityEvent_string));
	}
}

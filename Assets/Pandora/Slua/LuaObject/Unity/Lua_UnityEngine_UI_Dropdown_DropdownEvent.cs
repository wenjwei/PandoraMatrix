using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_Dropdown_DropdownEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.Dropdown.DropdownEvent.constructor");
			#endif
			UnityEngine.UI.Dropdown.DropdownEvent o;
			o=new UnityEngine.UI.Dropdown.DropdownEvent();
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
		LuaUnityEvent_int.reg(l);
		getTypeTable(l,"UnityEngine.UI.Dropdown.DropdownEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.Dropdown.DropdownEvent),typeof(LuaUnityEvent_int));
	}
}

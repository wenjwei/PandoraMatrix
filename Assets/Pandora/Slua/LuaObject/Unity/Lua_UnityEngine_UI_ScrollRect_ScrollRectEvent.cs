using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_ScrollRect_ScrollRectEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ScrollRect.ScrollRectEvent.constructor");
			#endif
			UnityEngine.UI.ScrollRect.ScrollRectEvent o;
			o=new UnityEngine.UI.ScrollRect.ScrollRectEvent();
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
		LuaUnityEvent_UnityEngine_Vector2.reg(l);
		getTypeTable(l,"UnityEngine.UI.ScrollRect.ScrollRectEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.ScrollRect.ScrollRectEvent),typeof(LuaUnityEvent_UnityEngine_Vector2));
	}
}

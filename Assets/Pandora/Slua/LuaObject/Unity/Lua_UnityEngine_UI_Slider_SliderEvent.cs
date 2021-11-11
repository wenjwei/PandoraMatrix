using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_Slider_SliderEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.Slider.SliderEvent.constructor");
			#endif
			UnityEngine.UI.Slider.SliderEvent o;
			o=new UnityEngine.UI.Slider.SliderEvent();
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
		getTypeTable(l,"UnityEngine.UI.Slider.SliderEvent");
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.Slider.SliderEvent),typeof(LuaUnityEvent_float));
	}
}

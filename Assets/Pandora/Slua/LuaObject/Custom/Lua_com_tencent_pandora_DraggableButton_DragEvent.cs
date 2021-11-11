using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_com_tencent_pandora_DraggableButton_DragEvent : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.DragEvent.constructor");
			#endif
			com.tencent.pandora.DraggableButton.DragEvent o;
			o=new com.tencent.pandora.DraggableButton.DragEvent();
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
		getTypeTable(l,"com.tencent.pandora.DraggableButton.DragEvent");
		createTypeMetatable(l,constructor, typeof(com.tencent.pandora.DraggableButton.DragEvent),typeof(LuaUnityEvent_UnityEngine_Vector2));
	}
}

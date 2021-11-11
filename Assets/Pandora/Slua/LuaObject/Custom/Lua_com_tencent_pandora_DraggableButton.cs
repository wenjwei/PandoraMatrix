using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_com_tencent_pandora_DraggableButton : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.constructor");
			#endif
			com.tencent.pandora.DraggableButton o;
			o=new com.tencent.pandora.DraggableButton();
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
	static public int OnBeginDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.OnBeginDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnBeginDrag(a1);
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
	static public int OnDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.OnDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnDrag(a1);
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
	static public int OnEndDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.OnEndDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnEndDrag(a1);
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
	static public int OnPointerClick(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.OnPointerClick");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			UnityEngine.EventSystems.PointerEventData a1;
			checkType(l,2,out a1);
			self.OnPointerClick(a1);
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
	static public int get_onDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.onDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.onDrag);
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
	static public int set_onDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.onDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			com.tencent.pandora.DraggableButton.DragEvent v;
			checkType(l,2,out v);
			self.onDrag=v;
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
	static public int get_onBeginDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.onBeginDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.onBeginDrag);
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
	static public int set_onBeginDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.onBeginDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			com.tencent.pandora.DraggableButton.DragEvent v;
			checkType(l,2,out v);
			self.onBeginDrag=v;
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
	static public int get_onEndDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.onEndDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.onEndDrag);
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
	static public int set_onEndDrag(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.DraggableButton.onEndDrag");
			#endif
			com.tencent.pandora.DraggableButton self=(com.tencent.pandora.DraggableButton)checkSelf(l);
			com.tencent.pandora.DraggableButton.DragEvent v;
			checkType(l,2,out v);
			self.onEndDrag=v;
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
		getTypeTable(l,"com.tencent.pandora.DraggableButton");
		addMember(l,OnBeginDrag);
		addMember(l,OnDrag);
		addMember(l,OnEndDrag);
		addMember(l,OnPointerClick);
		addMember(l,"onDrag",get_onDrag,set_onDrag,true);
		addMember(l,"onBeginDrag",get_onBeginDrag,set_onBeginDrag,true);
		addMember(l,"onEndDrag",get_onEndDrag,set_onEndDrag,true);
		createTypeMetatable(l,constructor, typeof(com.tencent.pandora.DraggableButton),typeof(UnityEngine.UI.Button));
	}
}

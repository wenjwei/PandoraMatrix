using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_VerticalLayoutGroup : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CalculateLayoutInputHorizontal(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.VerticalLayoutGroup.CalculateLayoutInputHorizontal");
			#endif
			UnityEngine.UI.VerticalLayoutGroup self=(UnityEngine.UI.VerticalLayoutGroup)checkSelf(l);
			self.CalculateLayoutInputHorizontal();
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
	static public int CalculateLayoutInputVertical(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.VerticalLayoutGroup.CalculateLayoutInputVertical");
			#endif
			UnityEngine.UI.VerticalLayoutGroup self=(UnityEngine.UI.VerticalLayoutGroup)checkSelf(l);
			self.CalculateLayoutInputVertical();
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
	static public int SetLayoutHorizontal(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.VerticalLayoutGroup.SetLayoutHorizontal");
			#endif
			UnityEngine.UI.VerticalLayoutGroup self=(UnityEngine.UI.VerticalLayoutGroup)checkSelf(l);
			self.SetLayoutHorizontal();
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
	static public int SetLayoutVertical(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.VerticalLayoutGroup.SetLayoutVertical");
			#endif
			UnityEngine.UI.VerticalLayoutGroup self=(UnityEngine.UI.VerticalLayoutGroup)checkSelf(l);
			self.SetLayoutVertical();
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
		getTypeTable(l,"UnityEngine.UI.VerticalLayoutGroup");
		addMember(l,CalculateLayoutInputHorizontal);
		addMember(l,CalculateLayoutInputVertical);
		addMember(l,SetLayoutHorizontal);
		addMember(l,SetLayoutVertical);
		createTypeMetatable(l,null, typeof(UnityEngine.UI.VerticalLayoutGroup),typeof(UnityEngine.UI.HorizontalOrVerticalLayoutGroup));
	}
}

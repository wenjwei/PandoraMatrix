using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_HorizontalLayoutGroup : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int CalculateLayoutInputHorizontal(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.HorizontalLayoutGroup.CalculateLayoutInputHorizontal");
			#endif
			UnityEngine.UI.HorizontalLayoutGroup self=(UnityEngine.UI.HorizontalLayoutGroup)checkSelf(l);
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
			beginSample("[C#]UnityEngine.UI.HorizontalLayoutGroup.CalculateLayoutInputVertical");
			#endif
			UnityEngine.UI.HorizontalLayoutGroup self=(UnityEngine.UI.HorizontalLayoutGroup)checkSelf(l);
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
			beginSample("[C#]UnityEngine.UI.HorizontalLayoutGroup.SetLayoutHorizontal");
			#endif
			UnityEngine.UI.HorizontalLayoutGroup self=(UnityEngine.UI.HorizontalLayoutGroup)checkSelf(l);
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
			beginSample("[C#]UnityEngine.UI.HorizontalLayoutGroup.SetLayoutVertical");
			#endif
			UnityEngine.UI.HorizontalLayoutGroup self=(UnityEngine.UI.HorizontalLayoutGroup)checkSelf(l);
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
		getTypeTable(l,"UnityEngine.UI.HorizontalLayoutGroup");
		addMember(l,CalculateLayoutInputHorizontal);
		addMember(l,CalculateLayoutInputVertical);
		addMember(l,SetLayoutHorizontal);
		addMember(l,SetLayoutVertical);
		createTypeMetatable(l,null, typeof(UnityEngine.UI.HorizontalLayoutGroup),typeof(UnityEngine.UI.HorizontalOrVerticalLayoutGroup));
	}
}

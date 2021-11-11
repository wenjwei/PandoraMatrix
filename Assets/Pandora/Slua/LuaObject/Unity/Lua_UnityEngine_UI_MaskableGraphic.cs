using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_MaskableGraphic : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetModifiedMaterial(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.GetModifiedMaterial");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			UnityEngine.Material a1;
			checkType(l,2,out a1);
			var ret=self.GetModifiedMaterial(a1);
			pushValue(l,true);
			pushValue(l,ret);
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
	static public int Cull(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.Cull");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			UnityEngine.Rect a1;
			checkValueType(l,2,out a1);
			System.Boolean a2;
			checkType(l,3,out a2);
			self.Cull(a1,a2);
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
	static public int SetClipRect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.SetClipRect");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			UnityEngine.Rect a1;
			checkValueType(l,2,out a1);
			System.Boolean a2;
			checkType(l,3,out a2);
			self.SetClipRect(a1,a2);
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
	static public int RecalculateClipping(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.RecalculateClipping");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			self.RecalculateClipping();
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
	static public int RecalculateMasking(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.RecalculateMasking");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			self.RecalculateMasking();
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
	static public int get_onCullStateChanged(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.onCullStateChanged");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.onCullStateChanged);
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
	static public int set_onCullStateChanged(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.onCullStateChanged");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			UnityEngine.UI.MaskableGraphic.CullStateChangedEvent v;
			checkType(l,2,out v);
			self.onCullStateChanged=v;
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
	static public int get_maskable(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.maskable");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.maskable);
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
	static public int set_maskable(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.MaskableGraphic.maskable");
			#endif
			UnityEngine.UI.MaskableGraphic self=(UnityEngine.UI.MaskableGraphic)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.maskable=v;
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
		getTypeTable(l,"UnityEngine.UI.MaskableGraphic");
		addMember(l,GetModifiedMaterial);
		addMember(l,Cull);
		addMember(l,SetClipRect);
		addMember(l,RecalculateClipping);
		addMember(l,RecalculateMasking);
		addMember(l,"onCullStateChanged",get_onCullStateChanged,set_onCullStateChanged,true);
		addMember(l,"maskable",get_maskable,set_maskable,true);
		createTypeMetatable(l,null, typeof(UnityEngine.UI.MaskableGraphic),typeof(UnityEngine.UI.Graphic));
	}
}

using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_RawImage : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int SetNativeSize(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.RawImage.SetNativeSize");
			#endif
			UnityEngine.UI.RawImage self=(UnityEngine.UI.RawImage)checkSelf(l);
			self.SetNativeSize();
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
	static public int get_mainTexture(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.RawImage.mainTexture");
			#endif
			UnityEngine.UI.RawImage self=(UnityEngine.UI.RawImage)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.mainTexture);
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
	static public int get_texture(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.RawImage.texture");
			#endif
			UnityEngine.UI.RawImage self=(UnityEngine.UI.RawImage)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.texture);
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
	static public int set_texture(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.RawImage.texture");
			#endif
			UnityEngine.UI.RawImage self=(UnityEngine.UI.RawImage)checkSelf(l);
			UnityEngine.Texture v;
			checkType(l,2,out v);
			self.texture=v;
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
	static public int get_uvRect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.RawImage.uvRect");
			#endif
			UnityEngine.UI.RawImage self=(UnityEngine.UI.RawImage)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.uvRect);
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
	static public int set_uvRect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.RawImage.uvRect");
			#endif
			UnityEngine.UI.RawImage self=(UnityEngine.UI.RawImage)checkSelf(l);
			UnityEngine.Rect v;
			checkValueType(l,2,out v);
			self.uvRect=v;
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
		getTypeTable(l,"UnityEngine.UI.RawImage");
		addMember(l,SetNativeSize);
		addMember(l,"mainTexture",get_mainTexture,null,true);
		addMember(l,"texture",get_texture,set_texture,true);
		addMember(l,"uvRect",get_uvRect,set_uvRect,true);
		createTypeMetatable(l,null, typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.MaskableGraphic));
	}
}

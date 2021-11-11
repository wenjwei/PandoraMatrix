using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_com_tencent_pandora_TextureHelper : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Show(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.TextureHelper.Show");
			#endif
			com.tencent.pandora.TextureHelper self=(com.tencent.pandora.TextureHelper)checkSelf(l);
			UnityEngine.Texture a1;
			checkType(l,2,out a1);
			self.Show(a1);
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
	static public int Clear(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.TextureHelper.Clear");
			#endif
			com.tencent.pandora.TextureHelper self=(com.tencent.pandora.TextureHelper)checkSelf(l);
			self.Clear();
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
	static public int GC_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]com.tencent.pandora.TextureHelper.GC");
			#endif
			com.tencent.pandora.TextureHelper.GC();
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
		getTypeTable(l,"com.tencent.pandora.TextureHelper");
		addMember(l,Show);
		addMember(l,Clear);
		addMember(l,GC_s);
		createTypeMetatable(l,null, typeof(com.tencent.pandora.TextureHelper),typeof(UnityEngine.MonoBehaviour));
	}
}

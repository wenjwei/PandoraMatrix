using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_LayoutRebuilder : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.constructor");
			#endif
			UnityEngine.UI.LayoutRebuilder o;
			o=new UnityEngine.UI.LayoutRebuilder();
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
	static public int IsDestroyed(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.IsDestroyed");
			#endif
			UnityEngine.UI.LayoutRebuilder self=(UnityEngine.UI.LayoutRebuilder)checkSelf(l);
			var ret=self.IsDestroyed();
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
	static public int Rebuild(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.Rebuild");
			#endif
			UnityEngine.UI.LayoutRebuilder self=(UnityEngine.UI.LayoutRebuilder)checkSelf(l);
			UnityEngine.UI.CanvasUpdate a1;
			checkEnum(l,2,out a1);
			self.Rebuild(a1);
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
	static public int LayoutComplete(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.LayoutComplete");
			#endif
			UnityEngine.UI.LayoutRebuilder self=(UnityEngine.UI.LayoutRebuilder)checkSelf(l);
			self.LayoutComplete();
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
	static public int GraphicUpdateComplete(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.GraphicUpdateComplete");
			#endif
			UnityEngine.UI.LayoutRebuilder self=(UnityEngine.UI.LayoutRebuilder)checkSelf(l);
			self.GraphicUpdateComplete();
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
	static public int ForceRebuildLayoutImmediate_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate");
			#endif
			UnityEngine.RectTransform a1;
			checkType(l,1,out a1);
			UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(a1);
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
	static public int MarkLayoutForRebuild_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild");
			#endif
			UnityEngine.RectTransform a1;
			checkType(l,1,out a1);
			UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(a1);
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
	static public int get_transform(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.LayoutRebuilder.transform");
			#endif
			UnityEngine.UI.LayoutRebuilder self=(UnityEngine.UI.LayoutRebuilder)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.transform);
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
		getTypeTable(l,"UnityEngine.UI.LayoutRebuilder");
		addMember(l,IsDestroyed);
		addMember(l,Rebuild);
		addMember(l,LayoutComplete);
		addMember(l,GraphicUpdateComplete);
		addMember(l,ForceRebuildLayoutImmediate_s);
		addMember(l,MarkLayoutForRebuild_s);
		addMember(l,"transform",get_transform,null,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.LayoutRebuilder));
	}
}

using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_Canvas : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.constructor");
			#endif
			UnityEngine.Canvas o;
			o=new UnityEngine.Canvas();
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
	static public int GetDefaultCanvasMaterial_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.GetDefaultCanvasMaterial");
			#endif
			var ret=UnityEngine.Canvas.GetDefaultCanvasMaterial();
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
	static public int GetETC1SupportedCanvasMaterial_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.GetETC1SupportedCanvasMaterial");
			#endif
			var ret=UnityEngine.Canvas.GetETC1SupportedCanvasMaterial();
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
	static public int ForceUpdateCanvases_s(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.ForceUpdateCanvases");
			#endif
			UnityEngine.Canvas.ForceUpdateCanvases();
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
	static public int get_renderMode(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.renderMode");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushEnum(l,(int)self.renderMode);
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
	static public int set_renderMode(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.renderMode");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			UnityEngine.RenderMode v;
			checkEnum(l,2,out v);
			self.renderMode=v;
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
	static public int get_isRootCanvas(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.isRootCanvas");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.isRootCanvas);
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
	static public int get_pixelRect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.pixelRect");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.pixelRect);
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
	static public int get_scaleFactor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.scaleFactor");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.scaleFactor);
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
	static public int set_scaleFactor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.scaleFactor");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			float v;
			checkType(l,2,out v);
			self.scaleFactor=v;
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
	static public int get_referencePixelsPerUnit(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.referencePixelsPerUnit");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.referencePixelsPerUnit);
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
	static public int set_referencePixelsPerUnit(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.referencePixelsPerUnit");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			float v;
			checkType(l,2,out v);
			self.referencePixelsPerUnit=v;
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
	static public int get_overridePixelPerfect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.overridePixelPerfect");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.overridePixelPerfect);
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
	static public int set_overridePixelPerfect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.overridePixelPerfect");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.overridePixelPerfect=v;
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
	static public int get_pixelPerfect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.pixelPerfect");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.pixelPerfect);
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
	static public int set_pixelPerfect(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.pixelPerfect");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.pixelPerfect=v;
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
	static public int get_planeDistance(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.planeDistance");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.planeDistance);
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
	static public int set_planeDistance(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.planeDistance");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			float v;
			checkType(l,2,out v);
			self.planeDistance=v;
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
	static public int get_renderOrder(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.renderOrder");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.renderOrder);
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
	static public int get_overrideSorting(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.overrideSorting");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.overrideSorting);
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
	static public int set_overrideSorting(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.overrideSorting");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.overrideSorting=v;
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
	static public int get_sortingOrder(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.sortingOrder");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.sortingOrder);
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
	static public int set_sortingOrder(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.sortingOrder");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			int v;
			checkType(l,2,out v);
			self.sortingOrder=v;
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
	static public int get_targetDisplay(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.targetDisplay");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.targetDisplay);
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
	static public int set_targetDisplay(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.targetDisplay");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			int v;
			checkType(l,2,out v);
			self.targetDisplay=v;
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
	static public int get_sortingLayerID(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.sortingLayerID");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.sortingLayerID);
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
	static public int set_sortingLayerID(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.sortingLayerID");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			int v;
			checkType(l,2,out v);
			self.sortingLayerID=v;
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
	static public int get_cachedSortingLayerValue(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.cachedSortingLayerValue");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.cachedSortingLayerValue);
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
	static public int get_additionalShaderChannels(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.additionalShaderChannels");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushEnum(l,(int)self.additionalShaderChannels);
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
	static public int set_additionalShaderChannels(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.additionalShaderChannels");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			UnityEngine.AdditionalCanvasShaderChannels v;
			checkEnum(l,2,out v);
			self.additionalShaderChannels=v;
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
	static public int get_sortingLayerName(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.sortingLayerName");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.sortingLayerName);
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
	static public int set_sortingLayerName(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.sortingLayerName");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			string v;
			checkType(l,2,out v);
			self.sortingLayerName=v;
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
	static public int get_rootCanvas(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.rootCanvas");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.rootCanvas);
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
	static public int get_worldCamera(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.worldCamera");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.worldCamera);
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
	static public int set_worldCamera(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.worldCamera");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			UnityEngine.Camera v;
			checkType(l,2,out v);
			self.worldCamera=v;
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
	static public int get_normalizedSortingGridSize(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.normalizedSortingGridSize");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.normalizedSortingGridSize);
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
	static public int set_normalizedSortingGridSize(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.Canvas.normalizedSortingGridSize");
			#endif
			UnityEngine.Canvas self=(UnityEngine.Canvas)checkSelf(l);
			float v;
			checkType(l,2,out v);
			self.normalizedSortingGridSize=v;
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
		getTypeTable(l,"UnityEngine.Canvas");
		addMember(l,GetDefaultCanvasMaterial_s);
		addMember(l,GetETC1SupportedCanvasMaterial_s);
		addMember(l,ForceUpdateCanvases_s);
		addMember(l,"renderMode",get_renderMode,set_renderMode,true);
		addMember(l,"isRootCanvas",get_isRootCanvas,null,true);
		addMember(l,"pixelRect",get_pixelRect,null,true);
		addMember(l,"scaleFactor",get_scaleFactor,set_scaleFactor,true);
		addMember(l,"referencePixelsPerUnit",get_referencePixelsPerUnit,set_referencePixelsPerUnit,true);
		addMember(l,"overridePixelPerfect",get_overridePixelPerfect,set_overridePixelPerfect,true);
		addMember(l,"pixelPerfect",get_pixelPerfect,set_pixelPerfect,true);
		addMember(l,"planeDistance",get_planeDistance,set_planeDistance,true);
		addMember(l,"renderOrder",get_renderOrder,null,true);
		addMember(l,"overrideSorting",get_overrideSorting,set_overrideSorting,true);
		addMember(l,"sortingOrder",get_sortingOrder,set_sortingOrder,true);
		addMember(l,"targetDisplay",get_targetDisplay,set_targetDisplay,true);
		addMember(l,"sortingLayerID",get_sortingLayerID,set_sortingLayerID,true);
		addMember(l,"cachedSortingLayerValue",get_cachedSortingLayerValue,null,true);
		addMember(l,"additionalShaderChannels",get_additionalShaderChannels,set_additionalShaderChannels,true);
		addMember(l,"sortingLayerName",get_sortingLayerName,set_sortingLayerName,true);
		addMember(l,"rootCanvas",get_rootCanvas,null,true);
		addMember(l,"worldCamera",get_worldCamera,set_worldCamera,true);
		addMember(l,"normalizedSortingGridSize",get_normalizedSortingGridSize,set_normalizedSortingGridSize,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Canvas),typeof(UnityEngine.Behaviour));
	}
}

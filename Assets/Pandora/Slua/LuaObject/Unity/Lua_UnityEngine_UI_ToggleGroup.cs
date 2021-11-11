using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_ToggleGroup : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int NotifyToggleOn(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.NotifyToggleOn");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			UnityEngine.UI.Toggle a1;
			checkType(l,2,out a1);
			self.NotifyToggleOn(a1);
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
	static public int UnregisterToggle(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.UnregisterToggle");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			UnityEngine.UI.Toggle a1;
			checkType(l,2,out a1);
			self.UnregisterToggle(a1);
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
	static public int RegisterToggle(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.RegisterToggle");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			UnityEngine.UI.Toggle a1;
			checkType(l,2,out a1);
			self.RegisterToggle(a1);
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
	static public int AnyTogglesOn(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.AnyTogglesOn");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			var ret=self.AnyTogglesOn();
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
	static public int ActiveToggles(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.ActiveToggles");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			var ret=self.ActiveToggles();
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
	static public int SetAllTogglesOff(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.SetAllTogglesOff");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			self.SetAllTogglesOff();
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
	static public int get_allowSwitchOff(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.allowSwitchOff");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.allowSwitchOff);
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
	static public int set_allowSwitchOff(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.ToggleGroup.allowSwitchOff");
			#endif
			UnityEngine.UI.ToggleGroup self=(UnityEngine.UI.ToggleGroup)checkSelf(l);
			bool v;
			checkType(l,2,out v);
			self.allowSwitchOff=v;
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
		getTypeTable(l,"UnityEngine.UI.ToggleGroup");
		addMember(l,NotifyToggleOn);
		addMember(l,UnregisterToggle);
		addMember(l,RegisterToggle);
		addMember(l,AnyTogglesOn);
		addMember(l,ActiveToggles);
		addMember(l,SetAllTogglesOff);
		addMember(l,"allowSwitchOff",get_allowSwitchOff,set_allowSwitchOff,true);
		createTypeMetatable(l,null, typeof(UnityEngine.UI.ToggleGroup),typeof(UnityEngine.EventSystems.UIBehaviour));
	}
}

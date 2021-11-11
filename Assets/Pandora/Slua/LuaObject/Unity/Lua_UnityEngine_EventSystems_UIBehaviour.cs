using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_EventSystems_UIBehaviour : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int IsActive(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.EventSystems.UIBehaviour.IsActive");
			#endif
			UnityEngine.EventSystems.UIBehaviour self=(UnityEngine.EventSystems.UIBehaviour)checkSelf(l);
			var ret=self.IsActive();
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
	static public int IsDestroyed(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.EventSystems.UIBehaviour.IsDestroyed");
			#endif
			UnityEngine.EventSystems.UIBehaviour self=(UnityEngine.EventSystems.UIBehaviour)checkSelf(l);
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
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.EventSystems.UIBehaviour");
		addMember(l,IsActive);
		addMember(l,IsDestroyed);
		createTypeMetatable(l,null, typeof(UnityEngine.EventSystems.UIBehaviour),typeof(UnityEngine.MonoBehaviour));
	}
}

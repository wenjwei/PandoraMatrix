using System;
using System.Collections.Generic;
namespace com.tencent.pandora {
	[LuaBinder(0)]
	public class BindUnity {
		public static Action<IntPtr>[] GetBindList() {
			Action<IntPtr>[] list= {
				Lua_UnityEngine_Application.reg,
				Lua_UnityEngine_Object.reg,
				Lua_UnityEngine_Component.reg,
				Lua_UnityEngine_Behaviour.reg,
				Lua_UnityEngine_Color.reg,
				Lua_UnityEngine_GameObject.reg,
				Lua_UnityEngine_Renderer.reg,
				Lua_UnityEngine_Vector3.reg,
				Lua_UnityEngine_Quaternion.reg,
				Lua_UnityEngine_MonoBehaviour.reg,
				Lua_UnityEngine_Resources.reg,
				Lua_UnityEngine_SystemInfo.reg,
				Lua_UnityEngine_Events_UnityEventBase.reg,
				Lua_UnityEngine_Events_UnityEvent.reg,
				Lua_UnityEngine_Vector2.reg,
				Lua_UnityEngine_Vector4.reg,
				Lua_UnityEngine_Transform.reg,
				Lua_UnityEngine_RectTransform.reg,
				Lua_UnityEngine_RectTransform_Axis.reg,
				Lua_UnityEngine_RectTransform_Edge.reg,
				Lua_UnityEngine_Sprite.reg,
				Lua_UnityEngine_Event.reg,
				Lua_UnityEngine_Canvas.reg,
				Lua_UnityEngine_CanvasGroup.reg,
			};
			return list;
		}
	}
}

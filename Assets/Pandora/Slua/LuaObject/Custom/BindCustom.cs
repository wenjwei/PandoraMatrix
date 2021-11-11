using System;
using System.Collections.Generic;
namespace com.tencent.pandora {
	[LuaBinder(3)]
	public class BindCustom {
		public static Action<IntPtr>[] GetBindList() {
			Action<IntPtr>[] list= {
				Lua_com_tencent_pandora_TextureHelper.reg,
				Lua_com_tencent_pandora_DraggableButton.reg,
				Lua_com_tencent_pandora_DraggableButton_DragEvent.reg,
				Lua_System_Object.reg,
				Lua_System_Collections_Generic_Dictionary_2_string_string.reg,
				Lua_System_Collections_Generic_List_1_string.reg,
				Lua_com_tencent_pandora_CSharpInterface.reg,
				Lua_com_tencent_pandora_Logger.reg,
			};
			return list;
		}
	}
}

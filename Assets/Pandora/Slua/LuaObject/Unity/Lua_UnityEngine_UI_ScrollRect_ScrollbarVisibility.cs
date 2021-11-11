using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_ScrollRect_ScrollbarVisibility : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.UI.ScrollRect.ScrollbarVisibility");
		addMember(l,0,"Permanent");
		addMember(l,1,"AutoHide");
		addMember(l,2,"AutoHideAndExpandViewport");
		LuaDLL.pua_pop(l, 1);
	}
}

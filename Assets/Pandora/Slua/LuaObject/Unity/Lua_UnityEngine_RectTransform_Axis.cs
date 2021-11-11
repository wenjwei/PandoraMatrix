using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_RectTransform_Axis : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.RectTransform.Axis");
		addMember(l,0,"Horizontal");
		addMember(l,1,"Vertical");
		LuaDLL.pua_pop(l, 1);
	}
}

using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_GridLayoutGroup_Constraint : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"UnityEngine.UI.GridLayoutGroup.Constraint");
		addMember(l,0,"Flexible");
		addMember(l,1,"FixedColumnCount");
		addMember(l,2,"FixedRowCount");
		LuaDLL.pua_pop(l, 1);
	}
}

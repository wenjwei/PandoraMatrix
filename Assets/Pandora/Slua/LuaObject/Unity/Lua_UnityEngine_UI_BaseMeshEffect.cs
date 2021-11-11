using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_BaseMeshEffect : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ModifyMesh(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]UnityEngine.UI.BaseMeshEffect.ModifyMesh");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(matchType(l,argc,2,typeof(UnityEngine.UI.VertexHelper))){
				UnityEngine.UI.BaseMeshEffect self=(UnityEngine.UI.BaseMeshEffect)checkSelf(l);
				UnityEngine.UI.VertexHelper a1;
				checkType(l,2,out a1);
				self.ModifyMesh(a1);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.Mesh))){
				UnityEngine.UI.BaseMeshEffect self=(UnityEngine.UI.BaseMeshEffect)checkSelf(l);
				UnityEngine.Mesh a1;
				checkType(l,2,out a1);
				self.ModifyMesh(a1);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.pua_pushstring(l,"No matched override function to call");
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
		getTypeTable(l,"UnityEngine.UI.BaseMeshEffect");
		addMember(l,ModifyMesh);
		createTypeMetatable(l,null, typeof(UnityEngine.UI.BaseMeshEffect),typeof(UnityEngine.EventSystems.UIBehaviour));
	}
}

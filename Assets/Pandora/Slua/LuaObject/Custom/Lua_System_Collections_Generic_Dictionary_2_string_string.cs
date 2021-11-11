using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_System_Collections_Generic_Dictionary_2_string_string : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.constructor");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			System.Collections.Generic.Dictionary<System.String,System.String> o;
			if(argc==1){
				o=new System.Collections.Generic.Dictionary<System.String,System.String>();
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(IEqualityComparer<System.String>))){
				System.Collections.Generic.IEqualityComparer<System.String> a1;
				checkType(l,2,out a1);
				o=new System.Collections.Generic.Dictionary<System.String,System.String>(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(IDictionary<System.String,System.String>))){
				System.Collections.Generic.IDictionary<System.String,System.String> a1;
				checkType(l,2,out a1);
				o=new System.Collections.Generic.Dictionary<System.String,System.String>(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(int))){
				System.Int32 a1;
				checkType(l,2,out a1);
				o=new System.Collections.Generic.Dictionary<System.String,System.String>(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(IDictionary<System.String,System.String>),typeof(IEqualityComparer<System.String>))){
				System.Collections.Generic.IDictionary<System.String,System.String> a1;
				checkType(l,2,out a1);
				System.Collections.Generic.IEqualityComparer<System.String> a2;
				checkType(l,3,out a2);
				o=new System.Collections.Generic.Dictionary<System.String,System.String>(a1,a2);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(int),typeof(IEqualityComparer<System.String>))){
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Collections.Generic.IEqualityComparer<System.String> a2;
				checkType(l,3,out a2);
				o=new System.Collections.Generic.Dictionary<System.String,System.String>(a1,a2);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			return error(l,"New object failed.");
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
	static public int Add(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Add");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			System.String a2;
			checkType(l,3,out a2);
			self.Add(a1,a2);
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
	static public int Clear(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Clear");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			self.Clear();
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
	static public int ContainsKey(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.ContainsKey");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			var ret=self.ContainsKey(a1);
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
	static public int ContainsValue(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.ContainsValue");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			var ret=self.ContainsValue(a1);
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
	static public int GetObjectData(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.GetObjectData");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.Runtime.Serialization.SerializationInfo a1;
			checkType(l,2,out a1);
			System.Runtime.Serialization.StreamingContext a2;
			checkValueType(l,3,out a2);
			self.GetObjectData(a1,a2);
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
	static public int OnDeserialization(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.OnDeserialization");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.Object a1;
			checkType(l,2,out a1);
			self.OnDeserialization(a1);
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
	static public int Remove(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Remove");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			var ret=self.Remove(a1);
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
	static public int TryGetValue(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.TryGetValue");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			System.String a2;
			var ret=self.TryGetValue(a1,out a2);
			pushValue(l,true);
			pushValue(l,ret);
			pushValue(l,a2);
			return 3;
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
	static public int get_Count(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Count");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.Count);
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
	static public int get_Comparer(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Comparer");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.Comparer);
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
	static public int get_Keys(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Keys");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.Keys);
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
	static public int get_Values(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.Values");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.Values);
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
	static public int getItem(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.getItem");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			string v;
			checkType(l,2,out v);
			var ret = self[v];
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
	static public int setItem(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.Dictionary<System.String,System.String>.setItem");
			#endif
			System.Collections.Generic.Dictionary<System.String,System.String> self=(System.Collections.Generic.Dictionary<System.String,System.String>)checkSelf(l);
			string v;
			checkType(l,2,out v);
			string c;
			checkType(l,3,out c);
			self[v]=c;
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
		getTypeTable(l,"DictStrStr");
		addMember(l,Add);
		addMember(l,Clear);
		addMember(l,ContainsKey);
		addMember(l,ContainsValue);
		addMember(l,GetObjectData);
		addMember(l,OnDeserialization);
		addMember(l,Remove);
		addMember(l,TryGetValue);
		addMember(l,getItem);
		addMember(l,setItem);
		addMember(l,"Count",get_Count,null,true);
		addMember(l,"Comparer",get_Comparer,null,true);
		addMember(l,"Keys",get_Keys,null,true);
		addMember(l,"Values",get_Values,null,true);
		createTypeMetatable(l,constructor, typeof(System.Collections.Generic.Dictionary<System.String,System.String>));
	}
}

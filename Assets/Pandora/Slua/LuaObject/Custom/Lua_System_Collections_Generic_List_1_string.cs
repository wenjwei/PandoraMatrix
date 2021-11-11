using System;
using com.tencent.pandora;
using System.Collections.Generic;
public class Lua_System_Collections_Generic_List_1_string : LuaObject {
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.constructor");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			System.Collections.Generic.List<System.String> o;
			if(argc==1){
				o=new System.Collections.Generic.List<System.String>();
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(IEnumerable<System.String>))){
				System.Collections.Generic.IEnumerable<System.String> a1;
				checkType(l,2,out a1);
				o=new System.Collections.Generic.List<System.String>(a1);
				pushValue(l,true);
				pushValue(l,o);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(int))){
				System.Int32 a1;
				checkType(l,2,out a1);
				o=new System.Collections.Generic.List<System.String>(a1);
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
			beginSample("[C#]System.Collections.Generic.List<System.String>.Add");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			self.Add(a1);
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
	static public int AddRange(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.AddRange");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Collections.Generic.IEnumerable<System.String> a1;
			checkType(l,2,out a1);
			self.AddRange(a1);
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
	static public int AsReadOnly(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.AsReadOnly");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			var ret=self.AsReadOnly();
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
	static public int BinarySearch(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.BinarySearch");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==2){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				var ret=self.BinarySearch(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Collections.Generic.IComparer<System.String> a2;
				checkType(l,3,out a2);
				var ret=self.BinarySearch(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==5){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.String a3;
				checkType(l,4,out a3);
				System.Collections.Generic.IComparer<System.String> a4;
				checkType(l,5,out a4);
				var ret=self.BinarySearch(a1,a2,a3,a4);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Clear(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Clear");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
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
	static public int Contains(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Contains");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			var ret=self.Contains(a1);
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
	static public int Exists(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Exists");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Predicate<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			var ret=self.Exists(a1);
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
	static public int Find(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Find");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Predicate<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			var ret=self.Find(a1);
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
	static public int FindAll(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.FindAll");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Predicate<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			var ret=self.FindAll(a1);
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
	static public int FindIndex(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.FindIndex");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==2){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Predicate<System.String> a1;
				LuaDelegation.checkDelegate(l,2,out a1);
				var ret=self.FindIndex(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Predicate<System.String> a2;
				LuaDelegation.checkDelegate(l,3,out a2);
				var ret=self.FindIndex(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==4){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Predicate<System.String> a3;
				LuaDelegation.checkDelegate(l,4,out a3);
				var ret=self.FindIndex(a1,a2,a3);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int FindLast(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.FindLast");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Predicate<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			var ret=self.FindLast(a1);
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
	static public int FindLastIndex(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.FindLastIndex");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==2){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Predicate<System.String> a1;
				LuaDelegation.checkDelegate(l,2,out a1);
				var ret=self.FindLastIndex(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Predicate<System.String> a2;
				LuaDelegation.checkDelegate(l,3,out a2);
				var ret=self.FindLastIndex(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==4){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Predicate<System.String> a3;
				LuaDelegation.checkDelegate(l,4,out a3);
				var ret=self.FindLastIndex(a1,a2,a3);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ForEach(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.ForEach");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Action<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			self.ForEach(a1);
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
	static public int GetRange(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.GetRange");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			System.Int32 a2;
			checkType(l,3,out a2);
			var ret=self.GetRange(a1,a2);
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
	static public int IndexOf(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.IndexOf");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==2){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				var ret=self.IndexOf(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				var ret=self.IndexOf(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==4){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				var ret=self.IndexOf(a1,a2,a3);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Insert(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Insert");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			System.String a2;
			checkType(l,3,out a2);
			self.Insert(a1,a2);
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
	static public int InsertRange(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.InsertRange");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			System.Collections.Generic.IEnumerable<System.String> a2;
			checkType(l,3,out a2);
			self.InsertRange(a1,a2);
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
	static public int LastIndexOf(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.LastIndexOf");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==2){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				var ret=self.LastIndexOf(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				var ret=self.LastIndexOf(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==4){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Int32 a3;
				checkType(l,4,out a3);
				var ret=self.LastIndexOf(a1,a2,a3);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Remove(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Remove");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
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
	static public int RemoveAll(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.RemoveAll");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Predicate<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			var ret=self.RemoveAll(a1);
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
	static public int RemoveAt(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.RemoveAt");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			self.RemoveAt(a1);
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
	static public int RemoveRange(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.RemoveRange");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			System.Int32 a2;
			checkType(l,3,out a2);
			self.RemoveRange(a1,a2);
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
	static public int Reverse(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Reverse");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==1){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				self.Reverse();
				pushValue(l,true);
				return 1;
			}
			else if(argc==3){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				self.Reverse(a1,a2);
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Sort(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Sort");
			#endif
			int argc = LuaDLL.pua_gettop(l);
			if(argc==1){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				self.Sort();
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(System.Comparison<System.String>))){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Comparison<System.String> a1;
				LuaDelegation.checkDelegate(l,2,out a1);
				self.Sort(a1);
				pushValue(l,true);
				return 1;
			}
			else if(matchType(l,argc,2,typeof(IComparer<System.String>))){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Collections.Generic.IComparer<System.String> a1;
				checkType(l,2,out a1);
				self.Sort(a1);
				pushValue(l,true);
				return 1;
			}
			else if(argc==4){
				System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
				System.Int32 a1;
				checkType(l,2,out a1);
				System.Int32 a2;
				checkType(l,3,out a2);
				System.Collections.Generic.IComparer<System.String> a3;
				checkType(l,4,out a3);
				self.Sort(a1,a2,a3);
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
	[com.tencent.pandora.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int ToArray(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.ToArray");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			var ret=self.ToArray();
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
	static public int TrimExcess(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.TrimExcess");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			self.TrimExcess();
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
	static public int TrueForAll(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.TrueForAll");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			System.Predicate<System.String> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			var ret=self.TrueForAll(a1);
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
	static public int get_Capacity(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Capacity");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.Capacity);
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
	static public int set_Capacity(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Capacity");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			int v;
			checkType(l,2,out v);
			self.Capacity=v;
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
	static public int get_Count(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.Count");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
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
	static public int getItem(IntPtr l) {
		try {
			#if PANDORA_PROFILE
			beginSample("[C#]System.Collections.Generic.List<System.String>.getItem");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			int v;
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
			beginSample("[C#]System.Collections.Generic.List<System.String>.setItem");
			#endif
			System.Collections.Generic.List<System.String> self=(System.Collections.Generic.List<System.String>)checkSelf(l);
			int v;
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
		getTypeTable(l,"ListStr");
		addMember(l,Add);
		addMember(l,AddRange);
		addMember(l,AsReadOnly);
		addMember(l,BinarySearch);
		addMember(l,Clear);
		addMember(l,Contains);
		addMember(l,Exists);
		addMember(l,Find);
		addMember(l,FindAll);
		addMember(l,FindIndex);
		addMember(l,FindLast);
		addMember(l,FindLastIndex);
		addMember(l,ForEach);
		addMember(l,GetRange);
		addMember(l,IndexOf);
		addMember(l,Insert);
		addMember(l,InsertRange);
		addMember(l,LastIndexOf);
		addMember(l,Remove);
		addMember(l,RemoveAll);
		addMember(l,RemoveAt);
		addMember(l,RemoveRange);
		addMember(l,Reverse);
		addMember(l,Sort);
		addMember(l,ToArray);
		addMember(l,TrimExcess);
		addMember(l,TrueForAll);
		addMember(l,getItem);
		addMember(l,setItem);
		addMember(l,"Capacity",get_Capacity,set_Capacity,true);
		addMember(l,"Count",get_Count,null,true);
		createTypeMetatable(l,constructor, typeof(System.Collections.Generic.List<System.String>));
	}
}

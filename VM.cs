using System;
using System.Collections.Generic;

namespace LuaV.VM {
	public enum LuaType {
		Nil,
		Boolean,
		Number,
		String,
		Table,
		Function,
		Userdata,
		Thread,
		None,
	}

	public class LuaObject : IEquatable<LuaObject> {
		public LuaType Type;

		public object Value;

		public static LuaObject String(string str) {
			return new LuaObject(LuaType.String, str);
		}

		public static LuaObject Number(float num) {
			return new LuaObject(LuaType.Number, num);
		}

		public static LuaObject Nil() {
			return new LuaObject(LuaType.Nil, null);
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode();
		}

		public float ToFloat() {
			return (float) Value;
		}

		public LuaObject(LuaType type, object value) {
			Type = type;

			Value = value;
		}

		public override string ToString()
		{
			if (Type == LuaType.String) {
				return string.Format("\"{0}\"", Value);
			}

			if (Value == null) {
				return "nil";
			}

			return Value.ToString();
		}

		public bool Equals(LuaObject o) {
			if (o == null || Type != o.Type)
				return false;
			
			if (Type == LuaType.Table)
				return ((LuaTable) this).Hash == ((LuaTable) o).Hash;
			
			if (Type == LuaType.String)
				return ((string) Value) == ((string) o.Value);

			return Value.Equals(o.Value);
		}
	}

	public class LuaTable : LuaObject {
		public List<LuaObject> Keys = new List<LuaObject>();

		public List<LuaObject> Values = new List<LuaObject>();

		public string Hash;

		public LuaTable(LuaTable table) : base(LuaType.Table, null) {
			Keys = table.Keys;

			Values = table.Values;

			Hash = Guid.NewGuid().ToString();
		}

		public LuaTable() : base(LuaType.Table, null) {
			Hash = Guid.NewGuid().ToString();
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append("{");

			for (int i = 0; i < Keys.Count; i++) {
				sb.Append(Keys[i]);
				sb.Append("=");
				sb.Append(Values[i]);
				sb.Append(", ");
			}

			sb.Append("}");

			return sb.ToString();
		}

		public LuaObject this[LuaObject key] {
			get {
				if (Keys.Contains(key)) {
					return Values[Keys.IndexOf(key)];
				}

				return new LuaObject(LuaType.Nil, null);
			}

			set {
				if (Keys.Contains(key)) {
					Values[Keys.IndexOf(key)] = value;
				}
				else {
					Keys.Add(key);

					Values.Add(value);
				}
			}
		}
	}

	public class LuaFunction : LuaObject {
		public Dictionary<int, LuaObject> Upvalues = new Dictionary<int, LuaObject>();

		public List<LuaObject> Args = new List<LuaObject>();

		public Expr Func;
		
		public LuaFunction() : base(LuaType.Function, null) {

		}
	}

	public class LuaStack {
		public List<LuaObject> Stack = new List<LuaObject>();

		public void Push(LuaObject obj) {
			Stack.Add(obj);
		}

		public LuaObject Pop(int i = -1) {
			LuaObject obj = Stack[Stack.Count + i];

			Stack.RemoveAt(Stack.Count + i);

			return obj;
		}

		public LuaObject Peek(int i = -1) {
			return Stack[Stack.Count + i];
		}
	}

	public class LuaVM {
		public LuaStack Stack = new LuaStack();

		public void Push(LuaObject obj) {
			Stack.Push(obj);
		}

		public void GetField(int idx, string key) {
			LuaObject obj = Stack.Peek(idx);

			if (obj.Type == LuaType.Table) {
				LuaTable table = (LuaTable) obj;

				if (table == null) {
					throw new Exception("Table is null");
				}

				LuaObject value = table[LuaObject.String(key)];

				Stack.Push(value);
			}
			else {
				throw new Exception("Attempt to get field of non-table");
			}
		}

		public void SetField(int idx, string key) {
			LuaObject obj = Stack.Peek(idx);

			if (obj.Type == LuaType.Table) {
				LuaTable table = (LuaTable) obj;

				LuaObject value = Stack.Pop();

				table[LuaObject.String(key)] = value;
			}
			else {
				throw new Exception("Attempt to set field of non-table");
			}
		}

		public void SetTable(int idx) {
			LuaObject obj = Stack.Peek(idx);

			if (obj.Type == LuaType.Table) {
				LuaTable table = (LuaTable) obj;

				LuaObject value = Stack.Pop();

				LuaObject key = Stack.Pop();

				table[key] = value;
			}
			else {
				throw new Exception("Attempt to set field of non-table");
			}
		}

		public LuaObject GetTop() {
			return Stack.Peek();
		}

		public LuaObject Pop(int i = -1) {
			return Stack.Pop(i);
		}
	}
}
using System;
using System.Collections.Generic;
using LuaV.VM;

namespace LuaV {
	public class LuaInterpreter {
		public LuaParser Parser;

		public LuaVM VM = new LuaVM();

		public LuaObject Expression(Expr expr) {
			if (expr.Type == "function") {
				LuaFunction value = new LuaFunction();

				for (int i = 0; i < expr.DefinedArgs.Count; i++) {
					value.Upvalues[i] = new LuaObject(LuaType.Nil, null);
				}

				value.Func = expr;

				return value;
			}

			return Math(expr);
		}

		public LuaObject Math(Expr expr) {

			if (expr.Type == "binary") {
				LuaObject left = Math(expr.Left);

				LuaObject right = Math(expr.Right);
				
				if (expr.Operator == "+") {
					if (left.Type == LuaType.Number) {
						if (right.Type == LuaType.Number) {
							return LuaObject.Number(left.ToFloat() + right.ToFloat());
						}
					}
				}

				return null;
			}

			return Literal(expr);
		}

		public LuaObject Literal(Expr expr) {
			if (expr.Type == "number") {
				return LuaObject.Number(float.Parse(expr.Value.Value));
			}

			if (expr.Type == "string") {
				return LuaObject.String(expr.Value.Value);
			}

			if (expr.Type == "boolean") {
				// return LuaObject.Boolean(expr.Value.Value == "true");
			}

			if (expr.Type == "nil") {
				return LuaObject.Nil();
			}

			if (expr.Type == "group") {
				return Expression(expr.Left);
			}

			if (expr.Type == "table") {
				LuaTable table = new LuaTable();

				VM.Push(table);

				for (int i = 0; i < expr.TKeys.Count; i++) {
					VM.Push(Math(expr.TKeys[i]));

					VM.Push(Math(expr.TValues[i]));

					VM.SetTable(-3);
				}

				return VM.Pop();
			}

			if (expr.Type == "call") {
				LuaFunction func = Expression(expr.Left) as LuaFunction;

				if (func == null) {
					throw new Exception("Attempt to call non-function");
				}

				Console.WriteLine("Call " + func.Func.Name);
			}

			return null;
		}
	}
}
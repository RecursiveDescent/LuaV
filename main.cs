using System;
using LuaV.VM;

namespace LuaV
{
    public class Program {
        public static void Main(string[] args) {
			LuaParser parser = new LuaParser(new Lexer("(function abc() { [15 + 5] = 2, [2] = 15 } end)()"));

			LuaInterpreter intrp = new LuaInterpreter();

			intrp.Expression(parser.Function());

			/*LuaTable tbl = (LuaTable) intrp.Literal(parser.Literal());

			Console.WriteLine(tbl[LuaObject.Number(20)]);

			Console.WriteLine(tbl[LuaObject.Number(2)]);*/
        }
    }
}

using System;
using System.Collections.Generic;

namespace LuaV {
	public class Expr {
		public string Type;

		public string Name;

		public List<string> DefinedArgs = new List<string>();

		public List<Expr> Body = new List<Expr>();

		public List<Expr> Args = new List<Expr>();

		public Expr Left;

		public string Operator;
		
		public Expr Right;

		public bool Dictionary = false;

		public List<Expr> TKeys = new List<Expr>();

		public List<Expr> TValues = new List<Expr>();

		public Token Value;

		public override string ToString()
		{
			if (Type == "binary") {
				return string.Format("({0} {1} {2})", Left, Operator, Right);
			}

			return Value.Value;
		}

		public Expr(Expr left, string op, Expr right) {
			Type = "binary";

			Left = left;

			Operator = op;

			Right = right;
		}

		public Expr(string type, Token value) {
			Type = type;

			Value = value;
		}

		public Expr(string type) {
			Type = type;
		}
	}

	public class Operator {
		public string Op;

		public int Precedence;

		public bool LeftAssociative = true;

		public Operator(string op, int precedence) {
			Op = op;

			Precedence = precedence;
		}
	}

	public class LuaParser {
		public Lexer Lex;

		private Stack<Expr> Operands = new Stack<Expr>();

		private Stack<Operator> Operators = new Stack<Operator>();

		// Lua operators
		public Dictionary<string, Operator> OperatorDefs = new Dictionary<string, Operator>() {
			{ "+", new Operator("+", 1) },
			{ "-", new Operator("-", 1) },
			{ "*", new Operator("*", 2) },
			{ "/", new Operator("/", 3) },
		};

		public Expr Function() {
			if (Lex.PeekToken().Value == "function") {
				Lex.GetToken();

				Expr expr = new Expr("function");

				if (Lex.PeekToken().Type == "ID") {
					expr.Name = Lex.GetToken().Value;
				}

				if (Lex.GetToken().Type != "LPAREN") {
					throw new Exception("Expected '('");
				}

				while (Lex.PeekToken().Type != "RPAREN") {
					expr.DefinedArgs.Add(Lex.GetToken().Value);

					if (Lex.PeekToken().Type != "RPAREN") {
						if (Lex.PeekToken().Type != "COMMA") {
							throw new Exception("Expected ','");
						}

						Lex.GetToken();
					}
				}

				Lex.GetToken();

				while (Lex.PeekToken().Value != "end") {
					expr.Body.Add(Function());
				}

				Lex.GetToken();

				return expr;
			}

			return Math();
		}

		public Expr Math() {
			Operands.Push(Primary());

			while (Lex.PeekToken().Type == "OP" && OperatorDefs.ContainsKey(Lex.PeekToken().Value)) {
				Operator op = OperatorDefs[Lex.PeekToken().Value];

				if (Operators.Count > 0) {
					Operator next = Operators.Peek();

					while (Operators.Count > 0 && (next.Precedence > op.Precedence || (op.Precedence == next.Precedence && Operators.Peek().LeftAssociative))) {
						Expr right = Operands.Pop();

						Operands.Push(new Expr(Operands.Pop(), Operators.Pop().Op, right));
					}
				}

				Lex.GetToken();

				Operators.Push(op);

				Operands.Push(Primary());
			}

			while (Operators.Count > 0) {
				Expr right = Operands.Pop();

				Operands.Push(new Expr(Operands.Pop(), Operators.Pop().Op, right));
			}

			return Operands.Pop();
		}

		public Expr Primary() {
			return Literal();
		}

		public Expr Table() {
			if (Lex.PeekToken().Type == "LBRACE") {
				Token open = Lex.GetToken();

				Expr table = new Expr("table");

				bool dict = false;

				bool first = true;

				while (Lex.PeekToken().Type != "RBRACE") {
					if (first && Lex.PeekToken().Type == "LBRACKET") {
						dict = true;

						table.Dictionary = true;
					}

					first = false;

					if (dict) {
						if (Lex.GetToken().Type != "LBRACKET") {
							throw new Exception("Expected '['");
						}

						table.TKeys.Add(Math());

						if (Lex.PeekToken().Type != "RBRACKET") {
							throw new Exception($"']' expected near '{Lex.PeekToken().Value}'");
						}

						Lex.GetToken();

						if (Lex.GetToken().Value != "=") {
							throw new Exception("Expected '=' near ']'");
						}

						table.TValues.Add(Math());
					}
					else {
						table.TValues.Add(Math());
					}

					if (Lex.PeekToken().Type == "COMMA") {
						Lex.GetToken();
					}
					else if (Lex.PeekToken().Type != "RBRACE") {
						throw new Exception($"'}}' expected (to close '{{' at line {open.Line}) near '{Lex.PeekToken().Value}'");
					}
				}

				Lex.GetToken();

				return table;
			}

			return Literal();
		}

		public Expr Literal() {
			if (Lex.PeekToken().Type == "LBRACE")
				return Table();
			
			Token tok = Lex.GetToken();

			if (tok.Type == "NUM")
				return new Expr("number", tok);
			
			if (tok.Type == "STR")
				return new Expr("string", tok);

			Expr callable = null;
			
			if (tok.Type == "ID")
				callable = new Expr("id", tok);

			if (tok.Type == "LPAREN") {
				callable = new Expr("group");

				callable.Left = Function();

				if (Lex.GetToken().Type != "RPAREN") {
					throw new Exception($"Expected ')' near '{tok.Value}'");
				}
			}

			if (Lex.PeekToken().Type == "LPAREN") {
				Expr expr = new Expr("call");

				expr.Left = callable;

				Token open = Lex.GetToken();

				// Add argument parsing later

				if (Lex.GetToken().Type != "RPAREN") {
					throw new Exception($"Expected ')' near '{open.Value}'");
				}

				return expr;
			}
			
			throw new Exception($"Unexpected symbol near '{tok.Value}'");
		}

		public LuaParser(Lexer lex) {
			Lex = lex;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Common {
	public enum TokenClass {
		Value, Function, Operator, LeftParentheses, RightParentheses, Negative
	};

	public enum Associativity {
		Left, Right
	};

	public enum AttributeProperty {
		Health, Attack, Speed, Merge, Split, AttackCooldown, Invalid
	};

	public class MathParser {
		public bool debugFlag;

		private static Regex Regex = new Regex(@"([\d]+[.][\d]+)|([\d]+)|([\+\-\*\/\^]+)|([\(\)])?");
		private static List<string> BinaryInfixOperators = new List<string>() { "+", "-", "*", "/", "^" };

		//Shunting yard algorithm
		public static double ProcessEquation(string equation, AttributeProperty property, int level) {
			if (equation.Equals("")) {
				throw new ArgumentException("Equation is empty.");
			}
			List<string> result = new List<string>(Regex.Split(equation.ToLower().Trim()));
			for (int i = result.Count - 1; i >= 0; i--) {
				if (result[i].Equals("") || result[i].Equals(" ")) {
					result.RemoveAt(i);
				}
			}
			Queue<string> queue = new Queue<string>();
			Stack<string> stack = new Stack<string>();

			for (int i = 0; i < result.Count; i++) {
				if (result[i].Equals("x")) {
					result[i] = level.ToString();
				}
				else if (result[i].Equals("r")) {
					float value = UnityEngine.Random.Range(1f, 1000f);
					value /= 1000f;
					result[i] = value.ToString();
				}
			}

			TokenClass previousTokenClass = TokenClass.Value;
			for (int i = 0; i < result.Count; i++) {
				string element = result[i];

				if (element.Equals("y") || element.Equals("=")) {
					continue;
				}

				TokenClass tokenClass = GetTokenClass(element);
				switch (tokenClass) {
					case TokenClass.Value:
						queue.Enqueue(element);
						break;
					case TokenClass.LeftParentheses:
						stack.Push(element);
						break;
					case TokenClass.RightParentheses:
						while (!stack.Peek().Equals("(")) {
							queue.Enqueue(stack.Pop());
						}
						stack.Pop();
						break;
					case TokenClass.Operator:
						if (element.Equals("-") && (previousTokenClass == TokenClass.Operator || previousTokenClass == TokenClass.LeftParentheses) && (stack.Count == 0 || result[i - 1].Equals("("))) {
							//Push unary operator "Negative" to stack.
							stack.Push("NEG");
							break;
						}
						if (stack.Count > 0) {
							string stackTopToken = stack.Peek();
							if (GetTokenClass(stackTopToken) == TokenClass.Operator) {
								Associativity tokenAssociativity = GetAssociativity(stackTopToken);
								int tokenPrecedence = GetPrecedence(element);
								int stackTopPrecedence = GetPrecedence(stackTopToken);
								if ((tokenAssociativity == Associativity.Left && tokenPrecedence <= stackTopPrecedence) || (tokenAssociativity == Associativity.Right && tokenPrecedence < stackTopPrecedence)) {
									queue.Enqueue(stack.Pop());
								}
							}
						}
						stack.Push(element);
						break;
				}

				if (tokenClass == TokenClass.Value || tokenClass == TokenClass.RightParentheses) {
					if (i < result.Count - 1) {
						string nextToken = result[i + 1];
						TokenClass nextTokenClass = GetTokenClass(nextToken);
						if (nextTokenClass != TokenClass.Operator && nextTokenClass != TokenClass.RightParentheses) {
							result.Insert(i + 1, "*");
						}
					}
				}

				previousTokenClass = tokenClass;
			}

			while (stack.Count > 0) {
				string operand = stack.Pop();
				if (operand.Equals("(") || operand.Equals(")")) {
					throw new ArgumentException("Mismatched parentheses.");
				}
				queue.Enqueue(operand);
			}

			Stack<string> expressionStack = new Stack<string>();
			while (queue.Count > 0) {
				string token = queue.Dequeue();
				TokenClass tokenClass = GetTokenClass(token);
				if (tokenClass == TokenClass.Value) {
					expressionStack.Push(token);
				}
				else {
					double answer = 0f;
					if (tokenClass == TokenClass.Operator) {
						string rightOperand = expressionStack.Pop();
						string leftOperand = expressionStack.Pop();
						if (token.Equals("+")) {
							answer = double.Parse(leftOperand);
							answer += double.Parse(rightOperand);
						}
						else if (token.Equals("-")) {
							answer = double.Parse(leftOperand);
							answer -= double.Parse(rightOperand);
						}
						else if (token.Equals("*")) {
							answer = double.Parse(leftOperand);
							answer *= double.Parse(rightOperand);
						}
						else if (token.Equals("/")) {
							answer = double.Parse(leftOperand);
							answer /= double.Parse(rightOperand);
						}
						else if (token.Equals("^")) {
							double baseValue = double.Parse(leftOperand);
							double exponent = double.Parse(rightOperand);
							answer = Math.Pow(baseValue, exponent);
						}
					}
					else if (tokenClass == TokenClass.Negative) {
						string operand = expressionStack.Pop();
						answer = double.Parse(operand) * -1f;
					}
					expressionStack.Push(answer.ToString());
				}
			}

			if (expressionStack.Count != 1) {
				throw new ArgumentException("Invalid equation.");
			}

			double finalAnswer = double.Parse(expressionStack.Pop());

			return finalAnswer;
		}


		private static TokenClass GetTokenClass(string token) {
			double tempDouble;
			if (double.TryParse(token, out tempDouble)) {
				return TokenClass.Value;
			}
			else if (token.Equals("(")) {
				return TokenClass.LeftParentheses;
			}
			else if (token.Equals(")")) {
				return TokenClass.RightParentheses;
			}
			else if (BinaryInfixOperators.Contains(token)) {
				return TokenClass.Operator;
			}
			else if (token.Equals("NEG")) {
				return TokenClass.Negative;
			}
			else {
				throw new ArgumentException("Invalid token.");
			}
		}

		private static Associativity GetAssociativity(string token) {
			if (token.Equals("^")) {
				return Associativity.Right;
			}
			return Associativity.Left;
		}

		private static int GetPrecedence(string token) {
			if (token.Equals("+") || token.Equals("-")) {
				return 1;
			}
			else if (token.Equals("*") || token.Equals("/")) {
				return 2;
			}
			else if (token.Equals("^")) {
				return 3;
			}
			else {
				throw new ArgumentException("Invalid token");
			}
		}
	}
}
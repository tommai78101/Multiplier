using UnityEngine;
using UnityEngine.UI;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public enum TokenClass {
	Value, Function, Operator, LeftParentheses, RightParentheses, Negative
};

public enum Associativity {
	Left, Right
};

public class Attributes : MonoBehaviour {
	public GameObject panelPrefab;
	public InputField equationInputField;
	public float inputLag;

	private Regex regex = new Regex(@"([\+\-\*\(\)\^\/\ ])");
	private List<string> binaryInfixOperators = new List<string>() { "+", "-", "*", "/" };

	public void Start() {
		if (this.panelPrefab == null) {
			Debug.LogError("Panels prefab has not been set.");
			return;
		}
		if (this.equationInputField == null) {
			Debug.LogError("Input field has not been set.");
			return;
		}
		this.inputLag = 0f;

		string[] attributesList = new string[] {
			"Health", "Attack", "Speed", "Merge", "Split"
		};

		for (int i = 0; i < attributesList.Length; i++) {
			GameObject obj = MonoBehaviour.Instantiate<GameObject>(this.panelPrefab);
			obj.transform.SetParent(this.transform);

			Title title = obj.GetComponentInChildren<Title>();
			if (title != null) {
				title.titleText.text = attributesList[i];
			}

			Number number = obj.GetComponentInChildren<Number>();
			if (number != null) {
				number.numberText.text = (1234.001f).ToString();
			}
		} 
	}

	public void Update() {
		if (!this.equationInputField.text.Equals("") && Input.GetKey(KeyCode.Return) && this.inputLag < 0.01f) {
			Debug.Log("Pressed enter.");
			InputText inputText = this.equationInputField.GetComponentInChildren<InputText>();
			if (inputText != null) {
				this.equationInputField.text = inputText.inputText.text;
				Debug.Log("Setting the equation.");
				try {
					ProcessEquation(inputText.inputText.text);
					Debug.Log("Done calculating");
				}
				catch (Exception e) {
					Debug.LogError(e.Message.ToString());
				}
				this.inputLag = 1.0f;
			}
			else {
				Debug.Log("This is null.");
			}
		}

		if (this.inputLag > 0f) {
			this.inputLag -= Time.deltaTime;
		}
	}

	public void ProcessEquation(string equation) {
		List<string> result = this.regex.Split(equation).Select(t => t.Trim()).Where(t => t != "").ToList();

		Queue<string> queue = new Queue<string>();
		Stack<string> stack = new Stack<string>();
		Debug.Log("Start calculating");

		for (int i = 0; i < result.Count; i++) {
			string element = result[i];
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
					if (element.Equals("-") && (stack.Count == 0 || result[i - 1].Equals("("))) {
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
		}

		Debug.Log("Second half");
		while (stack.Count > 0) {
			string operand = stack.Pop();
			if (operand.Equals("(") || operand.Equals(")")) {
				throw new ArgumentException("Mismatched parentheses.");
			}
			queue.Enqueue(operand);
		}

		Debug.Log("Third half");
		Stack<string> expressionStack = new Stack<string>();
		while (queue.Count > 0) {
			string token = queue.Dequeue();
			TokenClass tokenClass = GetTokenClass(token);
			if (tokenClass == TokenClass.Value) {
				expressionStack.Push(token);
			}
			else {
				float answer = 0f;
				if (tokenClass == TokenClass.Operator) {
					string rightOperand = expressionStack.Pop();
					string leftOperand = expressionStack.Pop();
					if (token.Equals("+")) {
						answer = float.Parse(leftOperand);
						answer += float.Parse(rightOperand);
					}
					else if (token.Equals("-")) {
						answer = float.Parse(leftOperand);
						answer -= float.Parse(rightOperand);
					}
					else if (token.Equals("*")) {
						answer = float.Parse(leftOperand);
						answer *= float.Parse(rightOperand);
					}
					else if (token.Equals("/")) {
						answer = float.Parse(leftOperand);
						answer /= float.Parse(rightOperand);
					}
				}
				else if (tokenClass == TokenClass.Negative) {
					string operand = expressionStack.Pop();
					answer = float.Parse(operand) * -1f;
				}
				expressionStack.Push(answer.ToString());
			}
		}

		Debug.Log("Fourth half");

		if (expressionStack.Count != 1) {
			throw new ArgumentException("Invalid equation.");
		}

		float finalAnswer = float.Parse(expressionStack.Pop());
		Debug.Log("Answer is: " + finalAnswer);
	}


	public TokenClass GetTokenClass(string token) {
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
		else if (binaryInfixOperators.Contains(token)) {
			return TokenClass.Operator;
		}
		else if (token.Equals("NEG")) {
			return TokenClass.Negative;
		}
		else {
			throw new ArgumentException("Invalid token.");
		}
	}

	public Associativity GetAssociativity(string token) {
		if (token.Equals("^")) {
			return Associativity.Right;
		}
		return Associativity.Left;
	}

	public int GetPrecedence(string token) {
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

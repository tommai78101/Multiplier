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

public enum AttributeProperty {
	Health, Attack, Speed, Merge, Split, Invalid
};

public class Attributes : MonoBehaviour {
	public GameObject panelPrefab;
	public InputField equationInputField;
	public ToggleGroup toggleGroup;
	public float inputLag;
	public bool debugFlag;
	public List<GameObject> prefabList;
	public Toggle healthToggle;
	public Toggle attackToggle;
	public Toggle speedToggle;
	public Toggle mergeToggle;
	public Toggle splitToggle;
	public AttributeProperty oldProperty;
	public AttributeProperty newProperty;
	public const int MAX_NUM_OF_LEVELS = 10;
	public UnitAttributes unitAttributes;

	private Regex regex = new Regex(@"([\+\-\*\(\)\^\/\ \D])");
	private List<string> binaryInfixOperators = new List<string>() { "+", "-", "*", "/", "^" };

	public void Start() {
		if (this.panelPrefab == null) {
			Debug.LogError("Panels prefab has not been set.");
			return;
		}
		if (this.equationInputField == null) {
			Debug.LogError("Input field has not been set.");
			return;
		}
		if (this.toggleGroup == null) {
			Debug.LogError("Toggle group has not been set.");
			return;
		}
		if (this.healthToggle == null || this.attackToggle == null || this.speedToggle == null || this.mergeToggle == null || this.splitToggle == null) {
			Debug.LogError("Toggle has not been set. Please check.");
			return;
		}
		this.oldProperty = this.newProperty = AttributeProperty.Health;
		this.inputLag = 0f;
		this.prefabList = new List<GameObject>();

		//string[] attributesList = new string[] {
		//	"Health", "Attack", "Speed", "Merge", "Split"
		//};

		//For each level, instantiate a prefab and place it in the Content of the ScrollView.
		//This allows the Attributes to show consistently the progression of the attributes for each level.
		for (int i = 0; i < MAX_NUM_OF_LEVELS; i++) {
			GameObject obj = MonoBehaviour.Instantiate<GameObject>(this.panelPrefab);
			obj.transform.SetParent(this.transform);
			this.prefabList.Add(obj);

			Title title = obj.GetComponentInChildren<Title>();
			if (title != null) {
				title.titleText.text = "Level " + (i + 1).ToString();
			}

			Number number = obj.GetComponentInChildren<Number>();
			if (number != null) {
				number.numberText.text = (0f).ToString();
			}
		}
	}

	public void Update() {
		if (!this.equationInputField.text.Equals("") && Input.GetKey(KeyCode.Return) && this.inputLag < 0.01f) {
			InputText inputText = this.equationInputField.GetComponentInChildren<InputText>();
			if (inputText != null) {
				this.equationInputField.text = inputText.inputText.text;
				AttributeProperty property = ProcessToggle();
				if (property == AttributeProperty.Invalid) {
					Debug.LogError("Toggle setup is incorrect. Please check.");
				}

				try {
					for (int level = 0; level < MAX_NUM_OF_LEVELS; level++) {
						float answer = ProcessEquation(inputText.inputText.text, property, level + 1);
						GameObject panel = this.prefabList[level];
						Title titlePanel = panel.GetComponentInChildren<Title>();
						if (titlePanel != null) {
							titlePanel.titleText.text = "Level " + (level + 1).ToString();
						}
						Number numberPanel = panel.GetComponentInChildren<Number>();
						if (numberPanel != null) {
							switch (property) {
								default:
								case AttributeProperty.Health:
									this.unitAttributes.healthPrefabList[level] = answer;
									break;
								case AttributeProperty.Attack:
									this.unitAttributes.attackPrefabList[level] = answer;
									break;
								case AttributeProperty.Speed:
									this.unitAttributes.speedPrefabList[level] = answer;
									break;
								case AttributeProperty.Merge:
									this.unitAttributes.mergePrefabList[level] = answer;
									break;
								case AttributeProperty.Split:
									this.unitAttributes.splitPrefabList[level] = answer;
									break;
							}
							numberPanel.numberText.text = answer.ToString();
						}
					}
				}
				catch (Exception e) {
					Debug.LogError(e.Message.ToString());
					this.equationInputField.text = this.equationInputField.text + " [" + e.Message.ToString() + "]";
					for (int i = 0; i < MAX_NUM_OF_LEVELS; i++) {
						GameObject obj = this.prefabList[i];
						Title title = obj.GetComponentInChildren<Title>();
						if (title != null) {
							title.titleText.text = "Level " + (i + 1).ToString();
						}
						Number number = obj.GetComponentInChildren<Number>();
						if (number != null) {
							number.numberText.text = (0f).ToString();
						}
					}
				}
				this.inputLag = 1.0f;
			}
			else {
				Debug.LogError("This is null.");
			}
		}

		if (this.inputLag > 0f) {
			this.inputLag -= Time.deltaTime;
		}

		if (this.oldProperty != this.newProperty) {
			List<float> propertyList;
			switch (this.newProperty) {
				default:
				case AttributeProperty.Health:
					propertyList = this.unitAttributes.healthPrefabList;
					break;
				case AttributeProperty.Attack:
					propertyList = this.unitAttributes.attackPrefabList;
					break;
				case AttributeProperty.Speed:
					propertyList = this.unitAttributes.speedPrefabList;
					break;
				case AttributeProperty.Merge:
					propertyList = this.unitAttributes.mergePrefabList;
					break;
				case AttributeProperty.Split:
					propertyList = this.unitAttributes.splitPrefabList;
					break;
			}
			for (int i = 0; i < MAX_NUM_OF_LEVELS; i++) {
				Number number = this.prefabList[i].GetComponentInChildren<Number>();
				if (number != null) {
					number.numberText.text = propertyList[i].ToString();
				}
			}
			this.oldProperty = this.newProperty;
		}
	}

	//Shunting yard algorithm
	public float ProcessEquation(string equation, AttributeProperty property, int level) {
		List<string> result = this.regex.Split(equation).Select(t => t.Trim().ToLower()).Where(t => t != "").ToList();
		Queue<string> queue = new Queue<string>();
		Stack<string> stack = new Stack<string>();

		if (this.debugFlag) {
			Debug.Log("DEBUG 1");
		}

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

		if (this.debugFlag) {
			Debug.Log("DEBUG 2");
		}

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

		if (this.debugFlag) {
			Debug.Log("DEBUG 3");
		}

		while (stack.Count > 0) {
			string operand = stack.Pop();
			if (operand.Equals("(") || operand.Equals(")")) {
				throw new ArgumentException("Mismatched parentheses.");
			}
			queue.Enqueue(operand);
		}

		if (this.debugFlag) {
			Debug.Log("DEBUG 4");
		}

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
					else if (token.Equals("^")) {
						float baseValue = float.Parse(leftOperand);
						float exponent = float.Parse(rightOperand);
						answer = Mathf.Pow(baseValue, exponent);
					}
				}
				else if (tokenClass == TokenClass.Negative) {
					string operand = expressionStack.Pop();
					answer = float.Parse(operand) * -1f;
				}
				expressionStack.Push(answer.ToString());
			}
		}

		if (this.debugFlag) {
			Debug.Log("DEBUG 5");
		}

		if (expressionStack.Count != 1) {
			throw new ArgumentException("Invalid equation.");
		}

		if (this.debugFlag) {
			Debug.Log("DEBUG 6");
		}

		float finalAnswer = float.Parse(expressionStack.Pop());
		return finalAnswer;
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

	public AttributeProperty ProcessToggle() {
		foreach (Toggle toggle in this.toggleGroup.ActiveToggles()) {
			if (toggle.isOn) {
				Text text = toggle.GetComponentInChildren<Text>();
				if (text != null) {
					string label = text.text;
					if (label.Equals("Health")) {
						return AttributeProperty.Health;
					}
					else if (label.Equals("Attack")) {
						return AttributeProperty.Attack;
					}
					else if (label.Equals("Speed")) {
						return AttributeProperty.Speed;
					}
					else if (label.Equals("Merge")) {
						return AttributeProperty.Merge;
					}
					else if (label.Equals("Split")) {
						return AttributeProperty.Split;
					}
				}
			}
		}
		return AttributeProperty.Invalid;
	}

	public void ChangeProperty() {
		this.newProperty = ProcessToggle();
	}
}

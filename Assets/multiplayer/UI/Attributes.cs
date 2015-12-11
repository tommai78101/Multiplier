using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MultiPlayer;
using Common;

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
	public Toggle attackCooldownToggle;
	public Toggle splitToggle;
	public AttributeProperty oldProperty;
	public AttributeProperty newProperty;
	public const int MAX_NUM_OF_LEVELS = 10;
	public UnitAttributes unitAttributes;

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
		if (this.healthToggle == null || this.attackToggle == null || this.speedToggle == null || this.mergeToggle == null || this.attackCooldownToggle == null || this.splitToggle == null) {
			Debug.LogError("Toggle has not been set. Please check.");
			return;
		}
		this.oldProperty = this.newProperty = AttributeProperty.Health;
		this.inputLag = 0f;
		this.prefabList = new List<GameObject>();

		//For each level, instantiate a prefab and place it in the Content of the ScrollView.
		//This allows the Attributes to show consistently the progression of the attributes for each level.
		for (int i = 0; i < MAX_NUM_OF_LEVELS; i++) {
			GameObject obj = MonoBehaviour.Instantiate<GameObject>(this.panelPrefab);
			obj.transform.SetParent(this.transform);
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			if (rectTransform != null) {
				rectTransform.localScale = Vector3.one;
			}
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
						float answer = (float) MathParser.ProcessEquation(inputText.inputText.text, property, level + 1);

						if (this.debugFlag) {
							Debug.Log("DEBUG 8");
						}

						GameObject panel = this.prefabList[level];
						Title titlePanel = panel.GetComponentInChildren<Title>();

						if (this.debugFlag) {
							Debug.Log("DEBUG 9");
						}

						if (titlePanel != null) {
							titlePanel.titleText.text = "Level " + (level + 1).ToString();
						}

						if (this.debugFlag) {
							Debug.Log("DEBUG 10");
						}

						int propertyValue = 0;
						Number numberPanel = panel.GetComponentInChildren<Number>();
						if (numberPanel != null) {
							if (this.unitAttributes != null) {
								switch (property) {
									case AttributeProperty.Health:
										this.unitAttributes.healthPrefabList[level] = answer;
										numberPanel.numberText.text = answer.ToString();
										propertyValue = 0;
										break;
									case AttributeProperty.Attack:
										this.unitAttributes.attackPrefabList[level] = answer;
										numberPanel.numberText.text = answer.ToString();
										propertyValue = 1;
										break;
									case AttributeProperty.Speed:
										this.unitAttributes.speedPrefabList[level] = answer;
										numberPanel.numberText.text = answer.ToString();
										propertyValue = 2;
										break;
									case AttributeProperty.Merge:
										this.unitAttributes.mergePrefabList[level] = answer;
										numberPanel.numberText.text = answer.ToString();
										propertyValue = 3;
										break;
									case AttributeProperty.AttackCooldown:
										this.unitAttributes.attackCooldownPrefabList[level] = answer;
										numberPanel.numberText.text = answer.ToString();
										propertyValue = 4;
										break;
									case AttributeProperty.Split:
										if (level <= 0) {
											this.unitAttributes.splitPrefabFactor = answer;
											numberPanel.numberText.text = answer.ToString();
										}
										else {
											numberPanel.numberText.text = "N/A";
										}
										level = 10;
										propertyValue = 5;
										break;
									default:
									case AttributeProperty.Invalid:
										throw new ArgumentException("Attribute property is invalid.");
								}
							}
						}

						if (this.debugFlag) {
							Debug.Log("DEBUG 11");
						}

						this.unitAttributes.CmdUpdateAnswer(answer, level, propertyValue);

						if (this.debugFlag) {
							Debug.Log("DEBUG 12");
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
			for (int i = 0; i < MAX_NUM_OF_LEVELS; i++) {
				Number number = this.prefabList[i].GetComponentInChildren<Number>();
				if (number != null) {
					switch (this.newProperty) {
						default:
						case AttributeProperty.Health:
							number.numberText.text = this.unitAttributes.healthPrefabList[i].ToString();
							break;
						case AttributeProperty.Attack:
							number.numberText.text = this.unitAttributes.attackPrefabList[i].ToString();
							break;
						case AttributeProperty.Speed:
							number.numberText.text = this.unitAttributes.speedPrefabList[i].ToString();
							break;
						case AttributeProperty.Merge:
							number.numberText.text = this.unitAttributes.mergePrefabList[i].ToString();
							break;
						case AttributeProperty.AttackCooldown:
							number.numberText.text = this.unitAttributes.attackCooldownPrefabList[i].ToString();
							break;
						case AttributeProperty.Split:
							if (i <= 0) {
								number.numberText.text = this.unitAttributes.splitPrefabFactor.ToString();
							}
							else {
								number.numberText.text = "N/A";
							}
							break;
					}
				}
			}
			//this.unitAttributes.UpdateValues();
			this.oldProperty = this.newProperty;
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
					else if (label.Equals("AtkCool")) {
						return AttributeProperty.AttackCooldown;
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

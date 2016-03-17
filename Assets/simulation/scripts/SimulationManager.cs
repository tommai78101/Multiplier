using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Extension;
using Common;
using MultiPlayer;
using SinglePlayer;

namespace Simulation {
	public class SimulationManager : MonoBehaviour {
		//Constants
		public const byte INVALID = 0xFF;
		public const byte LARGER = 0xF0;
		public const byte APPROXIMATE = 0x00;
		public const byte SMALLER = 0x0F;

		//Definite flag to let user modify either Yellow or Blue.
		public bool isEditingYellowTeam; //TRUE: Yellow Team. FALSE: Blue Team. Default value is Yellow.

		//Toggle Groups
		public ToggleGroup editTeamToggleGroup;
		public ToggleGroup unitAttributeToggleGroup;

		//Equation Editor stuffs
		public InputField equationInputField;
		public RectTransform contentPane;
		public GameObject levelInfoPrefab;

		//AI Attribute Managers
		public AIAttributeManager yellowTeamAttributes;
		public AIAttributeManager blueTeamAttributes;

		public void Start() {
			Initialization();
		}

		public void FixedUpdate() {
			CheckTeamToggle();
		}

		public void UpdateEquation() {
			string equation = this.equationInputField.text;
			Debug.Log("Equation is: " + equation);

			AIAttributeManager AIManager = null;
			if (this.isEditingYellowTeam) {
				AIManager = this.yellowTeamAttributes;
			}
			else {
				AIManager = this.blueTeamAttributes;
			}

			Toggle attributeToggle = this.unitAttributeToggleGroup.GetSingleActiveToggle();
			if (attributeToggle != null) {
				EnumToggle toggle = attributeToggle.GetComponent<EnumToggle>();
				if (toggle != null) {
					switch (toggle.value) {
						default:
							Debug.LogError("Wrong toggle value: " + toggle.value + ". Please check.");
							return;
						case 0:
							AIManager.SetDirectHealthAttribute(equation);
							break;
						case 1:
							AIManager.SetDirectAttackAttribute(equation);
							break;
						case 2:
							AIManager.SetDirectSpeedAttribute(equation);
							break;
						case 3:
							AIManager.SetDirectSplitAttribute(equation);
							break;
						case 4:
							AIManager.SetDirectMergeAttribute(equation);
							break;
						case 5:
							AIManager.SetDirectAttackCooldownAttribute(equation);
							break;
					}

					float previousAnswer = 0f;
					float answer = 0f;
					for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
						switch (toggle.value) {
							default:
								Debug.LogError("Invalid toggle value: " + toggle.value + ". Please check.");
								return;
							case 0:
								answer = (float) MathParser.ProcessEquation(equation, AttributeProperty.Health, i, i - 1, previousAnswer);
								break;
							case 1:
								answer = (float) MathParser.ProcessEquation(equation, AttributeProperty.Attack, i, i - 1, previousAnswer);
								break;
							case 2:
								answer = (float) MathParser.ProcessEquation(equation, AttributeProperty.Speed, i, i - 1, previousAnswer);
								break;
							case 3:
								answer = (float) MathParser.ProcessEquation(equation, AttributeProperty.Split, i, i - 1, previousAnswer);
								break;
							case 4:
								answer = (float) MathParser.ProcessEquation(equation, AttributeProperty.Merge, i, i - 1, previousAnswer);
								break;
							case 5:
								answer = (float) MathParser.ProcessEquation(equation, AttributeProperty.AttackCooldown, i, i - 1, previousAnswer);
								break;
						}


						GameObject obj = null;
						if (this.contentPane.transform.childCount < Attributes.MAX_NUM_OF_LEVELS) {
							obj = MonoBehaviour.Instantiate(this.levelInfoPrefab);
						}
						else {
							obj = this.contentPane.GetChild(i).gameObject;
						}
						LevelInfo levelInfo = obj.GetComponent<LevelInfo>();
						levelInfo.level = i + 1;
						levelInfo.rate = answer;
						if (levelInfo.level == 1) {
							levelInfo.comparisonFlag = INVALID;
						}
						else {
							if (answer < previousAnswer) {
								levelInfo.comparisonFlag = SMALLER;
							}
							else if (Mathf.Abs(previousAnswer - answer) <= float.Epsilon) {
								levelInfo.comparisonFlag = APPROXIMATE;
							}
							else if (answer > previousAnswer) {
								levelInfo.comparisonFlag = LARGER;
							}
						}

						levelInfo.UpdateText();

						levelInfo.transform.SetParent(this.contentPane.transform);
						levelInfo.transform.position = this.contentPane.transform.position;
						RectTransform rectTransform = levelInfo.GetComponent<RectTransform>();
						rectTransform.localScale = Vector3.one;
						rectTransform.localRotation = this.contentPane.localRotation;

						previousAnswer = answer;
					}
				}
			}
		}

		//-----------------  PRIVATE METHODS  ----------------------------

		private void Initialization() {
			//GameObjects null checking. Yes, you can do bitwise operations.
			bool flag = this.editTeamToggleGroup == null;
			flag |= this.unitAttributeToggleGroup == null;
			flag |= this.equationInputField == null;
			flag |= this.levelInfoPrefab == null;
			if (flag) {
				Debug.LogError("One of the game objects is null. Please check.");
			}

			//Boolean flags
			this.isEditingYellowTeam = true;
		}

		private void CheckTeamToggle() {
			Toggle editTeamToggle = this.editTeamToggleGroup.GetSingleActiveToggle();
			if (editTeamToggle != null) {
				EnumToggle toggle = editTeamToggle.GetComponent<EnumToggle>();
				if (toggle != null) {
					this.isEditingYellowTeam = toggle.value == 0 ? true : false;
				}
			}
		}
	}
}

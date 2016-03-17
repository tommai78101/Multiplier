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
							AIManager.SetHealthAttribute(equation);
							break;
						case 1:
							AIManager.SetAttackAttribute(equation);
							break;
						case 2:
							AIManager.SetSpeedAttribute(equation);
							break;
						case 3:
							AIManager.SetSplitAttribute(equation);
							break;
						case 4:
							AIManager.SetMergeAttribute(equation);
							break;
						case 5:
							AIManager.SetAttackCooldownAttribute(equation);
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

						LevelInfo levelInfo = new LevelInfo();
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

						levelInfo.transform.SetParent(this.contentPane.transform);
						RectTransform rectTransform = levelInfo.GetComponent<RectTransform>();
						rectTransform.localScale = Vector3.one;

						levelInfo.UpdateText();

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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SinglePlayer.UI;

namespace SinglePlayer {
	public class AIPreset : MonoBehaviour {
		public Dropdown dropdown;
		public AIAttributeManager aiAttributeManager;
		public AttributePanelUI aiAttributePanelUI;
		public UIEnableDisable aiEnableDisableUI;
		public CategoryHandler aiCategoryHandler;

		public void SetAIAttributes() {
			if (this.aiAttributeManager == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("AIAttributeManager");
				if (obj != null) {
					this.aiAttributeManager = obj.GetComponent<AIAttributeManager>();
				}
			}
			if (this.aiAttributeManager != null && this.dropdown != null) {
				int itemValue = this.dropdown.value;
				string attackCooldownExpression = "y=0.9*x";
				switch (itemValue) {
					default:
					case 0:
						break;
					case 1:
					case 2:
					case 3:
						Debug.Log("Setting expressions for AI: " + this.dropdown.options[itemValue].text);
						string expression = this.dropdown.options[itemValue].text;
						this.aiAttributeManager.SetHealthAttribute(expression);
						this.aiAttributeManager.SetAttackAttribute(expression);
						this.aiAttributeManager.SetAttackCooldownAttribute(attackCooldownExpression);
						this.aiAttributeManager.SetSpeedAttribute(expression);
						this.aiAttributeManager.SetSplitAttribute(expression);
						this.aiAttributeManager.SetMergeAttribute(expression);
						this.aiEnableDisableUI.DisableCustomEquations();
						break;
					case 4:
						this.aiAttributeManager.SetHealthAttribute("y=2*x");
						string otherExpression = "y=1.414*x";
						this.aiAttributeManager.SetAttackAttribute(otherExpression);
						this.aiAttributeManager.SetSpeedAttribute(otherExpression);
						this.aiAttributeManager.SetSplitAttribute(otherExpression);
						this.aiAttributeManager.SetMergeAttribute(otherExpression);
						this.aiAttributeManager.SetAttackCooldownAttribute(attackCooldownExpression);
						this.aiEnableDisableUI.DisableCustomEquations();
						break;
					case 5:
						string zero = "y=0";
						this.aiAttributeManager.SetHealthAttribute(zero);
						this.aiAttributeManager.SetAttackAttribute(zero);
						this.aiAttributeManager.SetAttackCooldownAttribute(zero);
						this.aiAttributeManager.SetSpeedAttribute(zero);
						this.aiAttributeManager.SetSplitAttribute(zero);
						this.aiAttributeManager.SetMergeAttribute(zero);
						this.aiEnableDisableUI.EnableCustomEquations();
						break;
				}
				this.aiAttributePanelUI.aiRefreshAttributes(this.aiAttributeManager);
			}
			else {
				Debug.Log("Cannot obtain AI attribute manager. Please check.");
			}
		}
	}
}

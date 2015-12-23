using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SinglePlayer.UI;

namespace SinglePlayer {
	public class AIPreset : MonoBehaviour {
		public Dropdown dropdown;
		public AIAttributeManager aiAttributeManager;
		public UIEnableDisable aiAttributePanelUI;
		public AICategoryHandler aiCategoryHandler;

		public void SetAIAttributes() {
			if (this.aiAttributeManager == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("AIAttributeManager");
				if (obj != null) {
					this.aiAttributeManager = obj.GetComponent<AIAttributeManager>();
				}
			}
			if (this.aiAttributeManager != null && this.dropdown != null) {
				int itemValue = this.dropdown.value;
				switch (itemValue) {
					default:
					case 0:
					case 1:
					case 2:
						Debug.Log("Setting expressions for AI: " + this.dropdown.options[itemValue].text);
						string expression = this.dropdown.options[itemValue].text;
						this.aiAttributeManager.SetHealthAttribute(expression);
						this.aiAttributeManager.SetAttackAttribute(expression);
						this.aiAttributeManager.SetAttackCooldownAttribute(expression);
						this.aiAttributeManager.SetSpeedAttribute(expression);
						this.aiAttributeManager.SetSplitAttribute(expression);
						this.aiAttributeManager.SetMergeAttribute(expression);
						this.aiAttributePanelUI.DisableCustomEquations();
						break;
					case 3:
						this.aiAttributeManager.SetHealthAttribute("y=2*x");
						string otherExpression = "y=1.414*x";
						this.aiAttributeManager.SetAttackAttribute(otherExpression);
						this.aiAttributeManager.SetSpeedAttribute(otherExpression);
						this.aiAttributeManager.SetSplitAttribute(otherExpression);
						this.aiAttributeManager.SetMergeAttribute(otherExpression);
						this.aiAttributeManager.SetAttackCooldownAttribute(otherExpression);
						this.aiAttributePanelUI.DisableCustomEquations();
						break;
					case 4:
						string zero = "y=0";
						this.aiAttributeManager.SetHealthAttribute(zero);
						this.aiAttributeManager.SetAttackAttribute(zero);
						this.aiAttributeManager.SetAttackCooldownAttribute(zero);
						this.aiAttributeManager.SetSpeedAttribute(zero);
						this.aiAttributeManager.SetSplitAttribute(zero);
						this.aiAttributeManager.SetMergeAttribute(zero);
						this.aiAttributePanelUI.EnableCustomEquations();
						break;
				}
				this.aiAttributeManager.aiLevelRateHandler.UpdateAllPanelItems(this.aiCategoryHandler.selectedToggle);
			}
			else {
				Debug.Log("Cannot obtain AI attribute manager. Please check.");
			}
		}
	}
}

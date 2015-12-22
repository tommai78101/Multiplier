using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SinglePlayer.UI;

namespace SinglePlayer {
	public class AIPreset : MonoBehaviour {
		public Dropdown dropdown;
		public AIAttributeManager attributeManager;
		public UIEnableDisable aiAttributePanelUI;
		public AICategoryHandler aiCategoryHandler;

		public void SetAIAttributes() {
			if (this.attributeManager == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("AIAttributeManager");
				if (obj != null) {
					this.attributeManager = obj.GetComponent<AIAttributeManager>();
				}
			}
			if (this.attributeManager != null && this.dropdown != null) {
				int itemValue = this.dropdown.value;
				switch (itemValue) {
					default:
					case 0:
					case 1:
					case 2:
						Debug.Log("Setting expressions for AI: " + this.dropdown.options[itemValue].text);
						string expression = this.dropdown.options[itemValue].text;
						this.attributeManager.SetHealthAttribute(expression);
						this.attributeManager.SetAttackAttribute(expression);
						this.attributeManager.SetAttackCooldownAttribute(expression);
						this.attributeManager.SetSpeedAttribute(expression);
						this.attributeManager.SetSplitAttribute(expression);
						this.attributeManager.SetMergeAttribute(expression);
						this.aiAttributePanelUI.DisableCustomEquations();
						break;
					case 3:
						this.attributeManager.SetHealthAttribute("y=2*x");
						string otherExpression = "y=1.414*x";
						this.attributeManager.SetAttackAttribute(otherExpression);
						this.attributeManager.SetSpeedAttribute(otherExpression);
						this.attributeManager.SetSplitAttribute(otherExpression);
						this.attributeManager.SetMergeAttribute(otherExpression);
						this.attributeManager.SetAttackCooldownAttribute(otherExpression);
						this.aiAttributePanelUI.DisableCustomEquations();
						break;
					case 4:
						string zero = "y=0";
						this.attributeManager.SetHealthAttribute(zero);
						this.attributeManager.SetAttackAttribute(zero);
						this.attributeManager.SetAttackCooldownAttribute(zero);
						this.attributeManager.SetSpeedAttribute(zero);
						this.attributeManager.SetSplitAttribute(zero);
						this.attributeManager.SetMergeAttribute(zero);
						this.aiAttributePanelUI.EnableCustomEquations();
						break;
				}
				this.aiCategoryHandler.Refresh();
			}
			else {
				Debug.Log("Cannot obtain AI attribute manager. Please check.");
			}
		}
	}
}

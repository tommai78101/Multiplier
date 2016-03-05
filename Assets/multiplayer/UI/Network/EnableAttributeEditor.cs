using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnableAttributeEditor : MonoBehaviour {
	public CanvasGroup canvasGroupUnitAttributeEditor;
	public bool isCustomOptionSelected;

	public void Start() {
		if (this.canvasGroupUnitAttributeEditor == null) {
			Debug.LogError("Unassigned unit attribute editor.");
		}
		this.TurnOffCanvasGroup();
	}

	public void OnCustomOptionSelected(Dropdown dropdown) {
		if (dropdown.value == 3) {
			TurnOnCanvasGroup();
		}
		else {
			TurnOffCanvasGroup();
		}
	}

	public void TurnOnCanvasGroup() {
		this.isCustomOptionSelected = true;
		this.canvasGroupUnitAttributeEditor.alpha = 1f;
		this.canvasGroupUnitAttributeEditor.interactable = true;
		this.canvasGroupUnitAttributeEditor.blocksRaycasts = true;
	}

	public void TurnOffCanvasGroup() {
		this.isCustomOptionSelected = false;
		this.canvasGroupUnitAttributeEditor.alpha = 0f;
		this.canvasGroupUnitAttributeEditor.interactable = false;
		this.canvasGroupUnitAttributeEditor.blocksRaycasts = false;
	}
}

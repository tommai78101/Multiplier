using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnableAttributeEditor : MonoBehaviour {
	public GameObject unitAttributesEditor;

	public void Start() {
		if (this.unitAttributesEditor == null) {
			Debug.LogError("Unassigned unit attribute editor.");
		}
		this.unitAttributesEditor.SetActive(false);
	}

	public void OnCustomOptionSelected(Dropdown dropdown) {
		if (dropdown.value == 3) {
			this.unitAttributesEditor.SetActive(true);
		}
		else {
			this.unitAttributesEditor.SetActive(false);
		}
	}
}

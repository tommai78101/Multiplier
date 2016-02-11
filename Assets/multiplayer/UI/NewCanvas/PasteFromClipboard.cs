using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class PasteFromClipboard : MonoBehaviour {
	public InputField inputField;

	public void OnClick() {
		if (inputField != null) {
			PasteIPAddress(EditorGUIUtility.systemCopyBuffer);
		}
	}


	public void PasteIPAddress(string buffer) {
		this.inputField.text = buffer;
		Text text = this.inputField.placeholder.GetComponent<Text>();
		if (text != null) {
			text.text = buffer;
		}
		text = this.inputField.GetComponent<Text>();
		if (text != null) {
			text.text = buffer;
		}
	}
}

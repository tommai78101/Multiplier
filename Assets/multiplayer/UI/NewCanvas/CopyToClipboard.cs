using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class CopyToClipboard : MonoBehaviour {
	public InputField inputField;

	public void OnClick() {
		Text text = this.inputField.GetComponentInChildren<Text>();
		if (text != null) {
			EditorGUIUtility.systemCopyBuffer = text.text;
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#else
#endif
using System.Collections;

public class PasteFromClipboard : MonoBehaviour {
	public InputField inputField;


#if UNITY_EDITOR
	public void OnClick() {
		if (inputField != null) {
			PasteIPAddress(EditorGUIUtility.systemCopyBuffer);
		}
	}
#else
	public void Start(){
		this.gameObject.SetActive(false);
	}
#endif

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

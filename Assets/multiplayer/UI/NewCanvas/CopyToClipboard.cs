using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR 
using UnityEditor;
#else
#endif
using System.Collections;

public class CopyToClipboard : MonoBehaviour {
	public InputField inputField;

#if UNITY_EDITOR
	public void OnClick() {
		Text text = this.inputField.GetComponentInChildren<Text>();
		if (text != null) {
			EditorGUIUtility.systemCopyBuffer = text.text;
		}
	}
#else
	public void Start(){
		this.gameObject.SetActive(false);
	}
#endif
}

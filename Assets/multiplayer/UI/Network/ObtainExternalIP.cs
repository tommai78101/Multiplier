using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObtainExternalIP : MonoBehaviour {
	public InputField textField;

	public void Start() {
		if (this.textField == null) {
			Debug.LogError("Text field is not set.");
		}
	}
}


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Taskbar : MonoBehaviour {
	public static Taskbar Instance;

	public Text taskbarText;

	public void Start() {
		if (this.taskbarText == null) {
			this.taskbarText = this.GetComponentInChildren<Text>();
			if (this.taskbarText == null) {
				Debug.LogError("Cannot find Text UI component.");
			}
		}
		Taskbar.Instance = this;
		this.ShowTaskbar(false);
	}

	public void ShowTaskbar(bool flag) {
		this.gameObject.SetActive(flag);
	}
}

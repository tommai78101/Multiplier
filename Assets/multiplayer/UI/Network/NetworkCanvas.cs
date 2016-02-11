using UnityEngine;
using System.Collections;

public class NetworkCanvas : MonoBehaviour {
	public RectTransform firstMenu;

	public void DisableMenu() {
		this.firstMenu.gameObject.SetActive(false);
	}

	public void EnableMenu() {
		this.firstMenu.gameObject.SetActive(true);
	}
}

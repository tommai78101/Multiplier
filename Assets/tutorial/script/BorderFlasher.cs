using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BorderFlasher : MonoBehaviour {
	public bool showBorder;
	public Image panelBorderImage;

	public void Start() {
		this.showBorder = false;
		this.panelBorderImage = this.GetComponent<Image>();
		if (this.panelBorderImage == null) {
			Debug.LogError("Check to make sure UI Image is attached to game object, the image type is \'Sliced\', and \'Fill Center\' is false.");
		}
	}

	public void Update() {
		Color color = this.panelBorderImage.color;
		if (this.showBorder) {
			color.a += Time.deltaTime * 2f;
		}
		else {
			color.a -= Time.deltaTime * 2f;
		}
		this.panelBorderImage.color = color;

		if (color.a >= 1f) {
			this.showBorder = false;
		}
		else if (color.a < 0f) {
			this.showBorder = true;
		}
	}
}

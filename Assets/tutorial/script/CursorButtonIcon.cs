using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tutorial {
	[RequireComponent(typeof(Image))]
	public class CursorButtonIcon : MonoBehaviour {
		public Sprite leftClickSprite;
		public Sprite rightClickSprite;
		public Sprite nothing;
		public Image buttonClickImage;

		public void Start() {
			this.buttonClickImage = this.GetComponent<Image>();
			if (this.buttonClickImage == null) {
				Debug.LogError("Cannot find Image component. Please check.");
			}
			this.buttonClickImage.sprite = this.nothing;
		}

		public void SetButton(CursorButton button) {
			switch (button) {
				case CursorButton.Left_Click:
					this.buttonClickImage.sprite = this.leftClickSprite;
					break;
				case CursorButton.Right_Click:
					this.buttonClickImage.sprite = this.rightClickSprite;
					break;
				default:
					this.buttonClickImage.sprite = this.nothing;
					break;
			}
		}
	}
}

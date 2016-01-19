using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class ObtainStartingPosition : MonoBehaviour {
		public Cursor parentCursor;
		public RectTransform rectTransform;

		public void Start() {
			this.rectTransform = this.GetComponent<RectTransform>();
			if (this.rectTransform == null) {
				Debug.LogError("Cannot obtain RectTransform. Please check.");
			}
		}

		public void Update() {
			this.parentCursor.startingPosition = this.rectTransform.localPosition;
	    }
	}
}

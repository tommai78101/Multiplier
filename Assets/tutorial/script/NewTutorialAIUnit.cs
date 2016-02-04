using UnityEngine;
using System.Collections;
using Common;

namespace Tutorial {
	public class NewTutorialAIUnit : MonoBehaviour {
		public MeshRenderer selectionRingRenderer;

		public void Awake() {
			this.gameObject.SetActive(false);
			if (this.selectionRingRenderer != null) {
				this.selectionRingRenderer.enabled = false;
			}
		}

		public void ToggleSelectionRing(bool value) {
			this.selectionRingRenderer.enabled = value;
		}
	}
}

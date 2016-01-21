using UnityEngine;
using System.Collections;
using Common;

namespace SinglePlayer {
	public class TurnOnPostRenderer : MonoBehaviour {
		public PostRenderer postRenderer;
		public bool isInitialized;

		public void Start() {
			this.isInitialized = false;
			this.postRenderer = this.GetComponent<PostRenderer>();
			if (this.postRenderer == null) {
				Debug.Log("Can't initialize post renderer. Moving to Update() to check.");
			}
		}

		// Update is called once per frame
		public void Update() {
			if (!this.isInitialized) {
				if (this.postRenderer == null) {
					this.postRenderer = this.GetComponent<PostRenderer>();
					if (this.postRenderer != null) {
						this.postRenderer.enabled = false;
					}
				}
			}
		}

		public void TurnOnSelectionBox() {
			if (this.postRenderer != null) {
				this.postRenderer.enabled = true;
				this.isInitialized = true;
				this.enabled = false;
			}
		}
	}
}

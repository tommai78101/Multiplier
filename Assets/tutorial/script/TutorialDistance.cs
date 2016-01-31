using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class TutorialDistance : MonoBehaviour {
		public Transform level1_distanceUnits;
		public Transform level2_distanceUnits;
		public GameObject level1Unit;
		public GameObject level2Unit;

		[SerializeField]
		private int iterator;
		[SerializeField]
		private int childCount;
		[SerializeField]
		private float elapsedTimeCounter;

		public void Start() {
			bool flag = (this.level1_distanceUnits == null) || (this.level2_distanceUnits == null) || (this.level1Unit == null) || (this.level2Unit == null);
			if (flag) {
				Debug.LogError("Transform for game objects to show distance from owner unit is missing.");
			}
			this.childCount = this.level1_distanceUnits.childCount;
			this.elapsedTimeCounter = 0f;
			this.level1Unit.SetActive(true);
			this.level2Unit.SetActive(true);
			Renderer renderer = this.level1Unit.GetComponent<Renderer>();
			if (renderer != null) {
				Material material = renderer.material;
				material.SetColor("_TeamColor", Color.red);
			}
			renderer = this.level2Unit.GetComponent<Renderer>();
			if (renderer != null) {
				Material material = renderer.material;
				material.SetColor("_TeamColor", Color.red);
			}

			this.HideDistance();
		}

		public void Update() {
			if (this.elapsedTimeCounter > 1f) {
				this.elapsedTimeCounter = 0f;

				if (this.iterator < this.childCount) {
					ShowDistance(this.iterator);
					this.iterator++;
				}
				else {
					this.iterator = 0;
					HideDistance();
				}
			}
			else {
				this.elapsedTimeCounter += Time.deltaTime;
			}
		}


		private void ShowDistance(int iterator) {
			Transform child = this.level1_distanceUnits.GetChild(iterator);
			child.gameObject.SetActive(true);
			child = this.level2_distanceUnits.GetChild(iterator);
			child.gameObject.SetActive(true);
		}

		private void HideDistance() {
			foreach (Transform child in this.level1_distanceUnits) {
				child.gameObject.SetActive(false);
			}
			foreach (Transform child in this.level2_distanceUnits) {
				child.gameObject.SetActive(false);
			}
		}
	}
}

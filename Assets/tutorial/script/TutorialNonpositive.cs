using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class TutorialNonpositive : MonoBehaviour {
		public Transform level1_StartPoint;
		public Transform level1_EndPoint;
		public Transform level2_StartPoint;
		public Transform level2_EndPoint;
		public Transform level1_Unit;
		public Transform level2_Unit;

		[SerializeField]
		private float level1_elapsedTime;
		[SerializeField]
		private float level2_elapsedTime;
		[SerializeField]
		private bool level1_negativeFlag;
		[SerializeField]
		private bool level2_negativeFlag;
		[SerializeField]
		private float level1_speedFactor;
		[SerializeField]
		private float level2_speedFactor;

		public void Start() {
			bool flag = (this.level1_StartPoint == null) || (this.level1_EndPoint == null) || (this.level2_StartPoint == null) || (this.level2_EndPoint == null);
			if (flag) {
				Debug.LogError("Vector points are not initialized.");
			}

			if (this.level1_Unit == null || this.level2_Unit == null) {
				Debug.LogError("The game objects are not set.");
			}

			this.level1_elapsedTime = 0f;
			this.level1_negativeFlag = false;
			this.level1_speedFactor = 1f;
			this.level1_Unit.gameObject.SetActive(true);

			this.level2_elapsedTime = 0f;
			this.level2_negativeFlag = false;
			this.level2_speedFactor = 2f;
			this.level2_Unit.gameObject.SetActive(true);
		}

		public void Update() {
			if (this.level1_negativeFlag) {
				this.level1_elapsedTime -= Time.deltaTime / this.level1_speedFactor;
			}
			else {
				this.level1_elapsedTime += Time.deltaTime / this.level1_speedFactor;
			}

			if (this.level2_negativeFlag) {
				this.level2_elapsedTime -= Time.deltaTime / this.level2_speedFactor;
			}
			else {
				this.level2_elapsedTime += Time.deltaTime / this.level2_speedFactor;
			}

			if (this.level1_elapsedTime > 1f) {
				this.level1_negativeFlag = true;
			}
			else if(this.level1_elapsedTime < 0f) {
				this.level1_negativeFlag = false;
			}
			if (this.level2_elapsedTime > 1f) {
				this.level2_negativeFlag = true;
			}
			else if (this.level2_elapsedTime < 0f) {
				this.level2_negativeFlag = false;
			}

			this.level1_Unit.transform.position = Vector3.Lerp(this.level1_StartPoint.position, this.level1_EndPoint.position, this.level1_elapsedTime);
			this.level2_Unit.transform.position = Vector3.Lerp(this.level2_StartPoint.position, this.level2_EndPoint.position, this.level2_elapsedTime);
		}
	}
}

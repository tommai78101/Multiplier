using UnityEngine;
using System.Collections;

public class SlowSpin : MonoBehaviour {
	public float angle;
	[Range(0.1f, 90f)]
	public float rotationSpeed;
	public void Update() {
		this.angle = (this.angle + Time.deltaTime * this.rotationSpeed) % 360f;
		this.transform.localRotation = Quaternion.Euler(new Vector3(0f, angle, 0f));
	}
}

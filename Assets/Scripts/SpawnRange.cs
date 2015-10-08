using UnityEngine;
using System.Collections;

public class SpawnRange : MonoBehaviour {
	public float radius;
	public SphereCollider spawnRangeTrigger;
	 
	private void Start() { 
		if (this.spawnRangeTrigger == null) {
			Debug.LogError("Spawn range trigger is not set. Please check.");
		}

		this.radius = this.spawnRangeTrigger.radius / 2f;
	}

	public void SetRadius(float value) {
		this.spawnRangeTrigger.radius = value;
		this.radius = value;
	}
}

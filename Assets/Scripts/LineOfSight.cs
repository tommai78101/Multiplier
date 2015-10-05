using UnityEngine;
using System.Collections.Generic;

public class LineOfSight : MonoBehaviour {
	public bool enemySpotted;
	public List<GameObject> enemiesInRange;

	public void OnTriggerEnter(Collider other) {
		//if (other.tag.Equals("Unit")) {
		//	if (this.enemiesInRange.Count <= 0) {
		//		this.enemySpotted = true;
		//	}
		//	GameUnit otherUnit = other.GetComponent<GameUnit>();
		//	if (!this.enemiesInRange.Contains(other.gameObject) && !otherUnit.hasAuthority) {
		//		this.enemiesInRange.Add(other.gameObject);
		//	}
		//}
	}

	public void OnTriggerExit(Collider other) {
		//if (other.tag.Equals("Unit")) {
		//	GameUnit otherUnit = other.GetComponent<GameUnit>();
		//	if (this.enemiesInRange.Contains(other.gameObject) && !otherUnit.hasAuthority) {
		//		this.enemiesInRange.Remove(other.gameObject);
		//	}
		//	if (this.enemiesInRange.Count <= 0) {
		//		this.enemySpotted = false;
		//	}
		//}
	}
}

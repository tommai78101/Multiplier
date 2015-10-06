using UnityEngine;
using System.Collections.Generic;

public class LineOfSight : MonoBehaviour {
	public List<GameUnit> enemiesInRange = new List<GameUnit>();

	//To use OnTrigger...() methods, you need to attach this script with a game object that have a Collider and a RigidBody.
	//And remember to untick (uncheck) the "Use Gravity" in RigidBody.

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		GameUnit myself = this.GetComponentInParent<GameUnit>();
		if (unit != null && myself != null && (unit != myself) && !unit.hasAuthority && !this.enemiesInRange.Contains(unit)) {
			this.enemiesInRange.Add(unit);
		}
	}

	public void OnTriggerExit(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		GameUnit myself = this.GetComponentInParent<GameUnit>();
		if (unit != null && myself != null && (unit != myself) && !unit.hasAuthority && this.enemiesInRange.Contains(unit)) {
			this.enemiesInRange.Remove(unit);
		}
	}
}

using UnityEngine;
using System.Collections.Generic;

public class AttackArea : MonoBehaviour {
	//Same design pattern as LineOfSight class.
	public List<GameUnit> enemies = new List<GameUnit>();

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && !this.enemies.Contains(unit)) {
			this.enemies.Add(unit);
		}
	}

	public void OnTriggerExit(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && this.enemies.Contains(unit)) {
			this.enemies.Remove(unit);
		}
	}
}

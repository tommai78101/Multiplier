using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AttackArea : MonoBehaviour {
	//Same design pattern as LineOfSight class.

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		GameUnit owner = this.GetComponentInParent<GameUnit>();
		if (unit != null && owner != null && !unit.hasAuthority && !owner.enemies.Contains(unit) && !unit.Equals(this.GetComponentInParent<GameUnit>())) {
			owner.enemies.Add(unit);
		}
	}

	public void OnTriggerExit(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		GameUnit owner = this.GetComponentInParent<GameUnit>();
		if (unit != null && owner != null && !unit.hasAuthority && owner.enemies.Contains(unit) && !unit.Equals(this.GetComponentInParent<GameUnit>())) {
			owner.enemies.Remove(unit);
		}
	}
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AttackArea : MonoBehaviour {
	//Same design pattern as LineOfSight class.
	public List<GameUnit> enemiesInAttackRange;
	public List<GameUnit> removeList;
	public GameUnit parent;

	public void Start() {
        this.enemiesInAttackRange = new List<GameUnit>();
		this.removeList = new List<GameUnit>();
		this.parent = this.GetComponentInParent<GameUnit>();
	}

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) {
			this.enemiesInAttackRange.Add(unit);
		}
	}

	public void OnTriggerExit(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) {
			this.enemiesInAttackRange.Remove(unit);
		}
	}

	public void OnTriggerStay(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) {
			this.enemiesInAttackRange.Add(unit);
		}
	}

	public void Update() {
		if (this.enemiesInAttackRange.Count > 0) {
			foreach (GameUnit unit in this.enemiesInAttackRange) {
				if (unit == null) {
					this.removeList.Add(unit);
				}
			}
		}

		if (this.removeList.Count > 0) {
			foreach (GameUnit unit in this.removeList) {
				if (this.enemiesInAttackRange.Contains(unit))
				this.enemiesInAttackRange.Remove(unit);
			}
			this.removeList.Clear();
		}
	}
}

using UnityEngine;
using System.Collections.Generic;

public class LineOfSight : MonoBehaviour {
	public List<GameUnit> enemiesInRange = new List<GameUnit>();
	public List<GameUnit> removeList = new List<GameUnit>();
	public float radius;
	public GameUnit parent;

	public void Start() {
		SphereCollider collider = this.GetComponent<SphereCollider>();
		if (collider != null) {
			this.radius = collider.radius;
		}
		this.parent = this.GetComponentInParent<GameUnit>();
		if (parent == null) {
			Debug.LogError("There's something wrong with this parent GameUnit object.");
		}
	}


	//To use OnTrigger...() methods, you need to attach this script with a game object that have a Collider and a RigidBody.
	//And remember to untick (uncheck) the "Use Gravity" in RigidBody.
	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		GameUnit myself = this.GetComponentInParent<GameUnit>();
		if (unit != null && myself != null && (unit != myself) && unit.CheckIfVisible() && !unit.hasAuthority && !this.enemiesInRange.Contains(unit)) {
			this.enemiesInRange.Add(unit);
		}
	}

	public void OnTriggerExit(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		GameUnit myself = this.GetComponentInParent<GameUnit>();
		if ((unit != null && myself != null && (unit != myself) && !unit.hasAuthority && this.enemiesInRange.Contains(unit)) || (!unit.CheckIfVisible() && this.enemiesInRange.Contains(unit))) {
			this.enemiesInRange.Remove(unit);
		}
	}

	public void OnTriggerStay(Collider other) {
		GameUnit unit = other.GetComponentInParent<GameUnit>();
		if (unit != null && !unit.hasAuthority && unit.CheckIfVisible() && !(unit.Equals(parent)) && !this.enemiesInRange.Contains(unit)) {
			this.enemiesInRange.Add(unit);
		}
	}

	public void FixedUpdate() {
		if (this.enemiesInRange.Count > 0) {
			foreach (GameUnit unit in this.enemiesInRange) {
				if (unit == null) {
					this.removeList.Add(unit);
				}
			}
		}

		if (this.removeList.Count > 0) {
			foreach (GameUnit unit in this.removeList) {
				if (this.enemiesInRange.Contains(unit)) {
					this.enemiesInRange.Remove(unit);
				}
			}
			this.removeList.Clear();
		}
	}

	public bool Recheck() {
		bool result = false;
		Collider[] objs = Physics.OverlapSphere(this.transform.position, this.radius / 2f);
		foreach (Collider col in objs) {
			GameUnit unit = col.gameObject.GetComponentInParent<GameUnit>();
			if (unit != null && !unit.Equals(this.parent) && !unit.hasAuthority && !this.enemiesInRange.Contains(unit)) {
				this.enemiesInRange.Add(unit);
				result = true;
			}
		}
		return result;
	}
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UnitCleaner : NetworkBehaviour {
	public SelectionManager selectionManager;
	public UnitAttributes unitAttributes;
	public int counter;

	public void Start() {
		counter = 10;
	}

	public void FixedUpdate() {
		if (!this.hasAuthority) {
			return;
		}

		this.StartCoroutine(CR_Clean());
	}

	public IEnumerator CR_Clean() {
		while (this.counter > 0) {
			this.counter--;
			yield return null;
		}
		CmdClean();
		this.counter = 20;
	}

	[Command]
	public void CmdClean() {
		if (this.selectionManager.selectedObjects.Count > 0) {
			foreach (GameObject obj in this.selectionManager.selectedObjects) {
				GameUnit unit = obj.GetComponent<GameUnit>();
				if (unit != null) {
					unit.speed = 1;
					unit.maxHealth = 1;
					unit.attackCooldown = 1;
					unit.attackPower = 1;
					int level = unit.level;
					for (int i = 1; i <= level; i++) {
						unit.speed *= this.unitAttributes.speedPrefabList[i];
						unit.maxHealth *= Mathf.FloorToInt(this.unitAttributes.healthPrefabList[i]);
						unit.attackCooldown *= this.unitAttributes.attackCooldownPrefabList[i];
						unit.attackPower *= this.unitAttributes.attackPrefabList[i];
					}
				}
				NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
				if (agent != null && unit != null) {
					agent.speed = unit.speed;
				}
			}
		}
	}
}


using UnityEngine;
using System.Collections.Generic;
using SinglePlayer;

public class Starter : MonoBehaviour {
	public GameObject redTeam;
	public Transform redTeamUnits;
	public Transform redTeamStartPosiiton;
	public GameObject blueTeam;
	public Transform blueTeamUnits;
	public Transform blueTeamStartPosition;
	public GameObject AIUnitPrefab;

	private AIManager redTeamAI;
	private AIManager blueTeamAI;
	private bool gameMatchStart;
	private float timePauseCounter;

	public void Start() {
		this.timePauseCounter = 1f;

		if (this.redTeamUnits != null) {
			int childs = this.redTeamUnits.transform.childCount;
			for (int i = childs; i > 0; i--) {
				MonoBehaviour.Destroy(this.redTeamUnits.transform.GetChild(i-1).gameObject);
			}
		}

		if (this.blueTeamUnits != null) {
			int childs = this.blueTeamUnits.transform.childCount;
			for (int i = childs; i > 0; i--) {
				MonoBehaviour.Destroy(this.blueTeamUnits.transform.GetChild(i-1).gameObject);
			}
		}

		if (this.redTeam != null) {
			//AI Unit spawning.
			GameObject obj = MonoBehaviour.Instantiate(this.AIUnitPrefab) as GameObject;
			obj.transform.SetParent(this.redTeamUnits.transform);
			obj.transform.position = this.redTeamStartPosiiton.position;
			AIUnit unit = obj.GetComponent<AIUnit>();

			//AI manager spawning.
			AIManager AImanager = this.redTeam.GetComponentInChildren<AIManager>();
			if (AImanager != null) {
				this.redTeamAI = AImanager;
				unit.unitManager = AImanager;
				unit.teamFaction = AImanager.teamFaction;
				unit.SetTeamColor(0);
				AImanager.Activate();
			}
		}

		if (this.blueTeam != null) {
			//AI Unit spawning.
			GameObject obj = MonoBehaviour.Instantiate(this.AIUnitPrefab) as GameObject;
			obj.transform.SetParent(this.blueTeamUnits.transform);
			obj.transform.position = this.blueTeamUnits.position;
			AIUnit unit = obj.GetComponent<AIUnit>();
			unit.SetTeamColor(1);

			//AI manager spawning.
			AIManager AImanager = this.blueTeam.GetComponentInChildren<AIManager>();
			if (AImanager != null) {
				this.blueTeamAI = AImanager;
				unit.unitManager = AImanager;
				unit.teamFaction = AImanager.teamFaction;
				unit.SetTeamColor(1);
				AImanager.Activate();
			}
		}

		this.gameMatchStart = true;
	}

	public void FixedUpdate() {
		if (this.gameMatchStart) {
			if (this.redTeamAI != null) {
				if (this.redTeamUnits.transform.childCount > 0) {
					this.redTeamAI.Activate();
				}
			}
			if (this.blueTeamAI != null) {
				if (this.blueTeamUnits.transform.childCount > 0) {
					this.blueTeamAI.Activate();
				}
			}

			if (this.redTeamUnits.transform.childCount <= 0 || this.blueTeamUnits.transform.childCount <= 0) {
				this.redTeamAI.Deactivate();
				this.blueTeamAI.Deactivate();

				int childs = this.redTeamUnits.transform.childCount;
				for (int i = childs; i > 0; i--) {
					MonoBehaviour.Destroy(this.redTeamUnits.transform.GetChild(i-1).gameObject);
				}

				childs = this.blueTeamUnits.transform.childCount;
				for (int i = childs; i > 0; i--) {
					MonoBehaviour.Destroy(this.blueTeamUnits.transform.GetChild(i-1).gameObject);
				}

				this.gameMatchStart = false;
			}
		}
		else {
			if (this.timePauseCounter > 0f) {
				this.timePauseCounter -= Time.deltaTime;
			}
			else {
				this.Start();
			}
		}
	}
}

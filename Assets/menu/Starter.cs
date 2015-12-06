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
			unit.SetTeamColor(0);

			//AI manager spawning.
			AIManager AIManager = this.redTeam.GetComponentInChildren<AIManager>();
			if (AIManager != null) {
				this.redTeamAI = AIManager;
				unit.unitManager = AIManager;
				AIManager.teamFaction = EnumTeam.Player;
				AIManager.Activate();
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
			AIManager AIManager = this.blueTeam.GetComponentInChildren<AIManager>();
			if (AIManager != null) {
				this.blueTeamAI = AIManager;
				unit.unitManager = AIManager;
				AIManager.teamFaction = EnumTeam.Computer;
				AIManager.Activate();
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

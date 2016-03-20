using UnityEngine;
using System.Collections.Generic;
using SinglePlayer;

public class Starter : MonoBehaviour {
	public GameObject yellowTeam;
	public Transform yellowTeamUnits;
	public Transform yellowTeamStartPosiiton;
	public GameObject blueTeam;
	public Transform blueTeamUnits;
	public Transform blueTeamStartPosition;
	public GameObject AIUnitPrefab;

	private AIManager yellowTeamAI;
	private AIManager blueTeamAI;
	private bool gameMatchStart;
	private float timePauseCounter;

	public const int YELLOW_TEAM_INDEX = 0;
	public const int BLUE_TEAM_INDEX = 1;

	public void Start() {
		this.timePauseCounter = 1f;

		if (this.yellowTeamUnits != null) {
			int childs = this.yellowTeamUnits.transform.childCount;
			for (int i = childs; i > 0; i--) {
				MonoBehaviour.Destroy(this.yellowTeamUnits.transform.GetChild(i-1).gameObject);
			}
		}

		if (this.blueTeamUnits != null) {
			int childs = this.blueTeamUnits.transform.childCount;
			for (int i = childs; i > 0; i--) {
				MonoBehaviour.Destroy(this.blueTeamUnits.transform.GetChild(i-1).gameObject);
			}
		}

		if (this.yellowTeam != null) {
			//AI Unit spawning.
			GameObject obj = MonoBehaviour.Instantiate(this.AIUnitPrefab) as GameObject;
			obj.transform.SetParent(this.yellowTeamUnits.transform);
			obj.transform.position = this.yellowTeamStartPosiiton.position;
			AIUnit unit = obj.GetComponent<AIUnit>();

			//AI manager spawning.
			AIManager AImanager = this.yellowTeam.GetComponentInChildren<AIManager>();
			if (AImanager != null) {
				this.yellowTeamAI = AImanager;
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
			if (this.yellowTeamAI != null) {
				if (this.yellowTeamUnits.transform.childCount > 0) {
					this.yellowTeamAI.Activate();
				}
			}
			if (this.blueTeamAI != null) {
				if (this.blueTeamUnits.transform.childCount > 0) {
					this.blueTeamAI.Activate();
				}
			}

			if (this.yellowTeamUnits.transform.childCount <= 0 || this.blueTeamUnits.transform.childCount <= 0) {
				this.yellowTeamAI.Deactivate();
				this.blueTeamAI.Deactivate();

				int childs = this.yellowTeamUnits.transform.childCount;
				for (int i = childs; i > 0; i--) {
					MonoBehaviour.Destroy(this.yellowTeamUnits.transform.GetChild(i-1).gameObject);
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

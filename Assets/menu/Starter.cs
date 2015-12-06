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

	public void Start() {
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
				AIManager.startAIFlag = true;
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
				AIManager.startAIFlag = true;
			}
		}
	}

	public void FixedUpdate() {
		if (this.redTeamAI != null) {
			this.redTeamAI.Activate();
		}
		if (this.blueTeamAI != null) {
			this.blueTeamAI.Activate();
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SinglePlayer;

namespace Simulation {
	public class SimulationStarter : MonoBehaviour {
		public Transform yellowTeamUnits;
		public Transform yellowTeamStartPosiiton;
		public Transform blueTeamUnits;
		public Transform blueTeamStartPosition;
		public GameObject yellowTeam;
		public GameObject blueTeam;
		public GameObject AIUnitPrefab;
		public Text sessionNumberText;

		[SerializeField]
		private bool gameMatchStart;
		[SerializeField]
		private bool gamePaused;
		[SerializeField]
		private int sessionNumber;
		[SerializeField]
		private float timePauseCounter;
		private AIManager yellowTeamAI;
		private AIManager blueTeamAI;

		public const int YELLOW_TEAM_INDEX = 0;
		public const int BLUE_TEAM_INDEX = 1;

		public void Start() {
			this.gameMatchStart = false;
			this.sessionNumber = 0;
			this.timePauseCounter = 0f;
			InitializeSimulation();
		}

		public void InitializeSimulation() {
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
					unit.SetTeamColor(YELLOW_TEAM_INDEX);
					AImanager.Deactivate();
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
					unit.SetTeamColor(BLUE_TEAM_INDEX);
					AImanager.Deactivate();
				}
			}
		}

		public void ClearSimulation() {
			if (this.yellowTeamUnits != null) {
				int childs = this.yellowTeamUnits.transform.childCount;
				for (int i = childs; i > 0; i--) {
					MonoBehaviour.Destroy(this.yellowTeamUnits.transform.GetChild(i - 1).gameObject);
				}
			}

			if (this.blueTeamUnits != null) {
				int childs = this.blueTeamUnits.transform.childCount;
				for (int i = childs; i > 0; i--) {
					MonoBehaviour.Destroy(this.blueTeamUnits.transform.GetChild(i - 1).gameObject);
				}
			}
		}

		public void StartSimulation() {
			this.gameMatchStart = true;
			this.sessionNumber++;
			Debug.Log("Session Number has increased - INITIAL PHASE.");
		}

		public void StopSimulation() {
			this.gameMatchStart = false;
			this.sessionNumber = 0;
			this.ClearSimulation();
			this.InitializeSimulation();
		}

		public void ContinueSimulation() {
			this.timePauseCounter = 1f;
			this.sessionNumber++;
			this.sessionNumberText.text = this.sessionNumber.ToString();
			Debug.Log("Session Number has increased.");
			this.ClearSimulation();
			this.gameMatchStart = true;
			//this.gamePaused = true;
		}

		public void PauseSimulation() {
			this.gamePaused = !this.gamePaused;
		}

		public void FixedUpdate() {
			if (this.sessionNumberText != null) {
				this.sessionNumberText.text = this.sessionNumber.ToString();
			}

			if (!this.gameMatchStart) {
				return;
			}

			if (this.gamePaused) {
				this.timePauseCounter = 1f;
				this.yellowTeamAI.Deactivate();
				this.blueTeamAI.Deactivate();
				return;
			}

			if (this.timePauseCounter > 0f) {
				this.timePauseCounter -= Time.deltaTime;
				return;
			}
			else {
				if (this.yellowTeamUnits.transform.childCount <= 0 && this.blueTeamUnits.transform.childCount <= 0) {
					InitializeSimulation();
					this.yellowTeamAI.Activate();
					this.blueTeamAI.Activate();
					return;
				}
			}

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
				Debug.Log("TEST");
				this.yellowTeamAI.Deactivate();
				this.blueTeamAI.Deactivate();
				ContinueSimulation();
			}
		}
	}
}
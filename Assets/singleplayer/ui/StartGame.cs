using UnityEngine;
using System.Collections.Generic;
using SinglePlayer;

public class StartGame : MonoBehaviour {
	public CanvasGroup attributePanelCanvasGroup;
	public Transform playerStartingPosition;
	public Transform computerStartingPosition;
	public GameObject gameUnitPrefab;
	public SelectionManager playerSelectionManager;
	public SelectionManager computerSelectionManager;


	public void StartButtonAction() {
		InitializationCheck();
		ClosePanel();
		LoadGame();
	}

	private void InitializationCheck() {
		bool flag = (this.attributePanelCanvasGroup == null) || (this.playerStartingPosition == null) || (this.computerStartingPosition == null) || (this.gameUnitPrefab == null)
			|| (this.playerSelectionManager == null) || (this.computerSelectionManager == null);
		if (flag) {
			Debug.LogError("There's something wrong with the Start Game menu button. Check again.");
		}
		Debug.Assert(!flag);
	}

	private void ClosePanel() {
		if (this.attributePanelCanvasGroup != null) {
			this.attributePanelCanvasGroup.blocksRaycasts = false;
			this.attributePanelCanvasGroup.alpha = 0f;
			this.attributePanelCanvasGroup.interactable = false;
		}
	}

	private void LoadGame() {
		GameObject playerUnit = MonoBehaviour.Instantiate(this.gameUnitPrefab) as GameObject;
		Transform unitTransform = playerUnit.transform;
		unitTransform.position = this.playerStartingPosition.transform.position;
		unitTransform.SetParent(this.playerSelectionManager.transform);
		GameUnit unit = playerUnit.GetComponent<GameUnit>();
		if (unit != null) {
			unit.teamFaction = this.playerSelectionManager.teamFaction;
			this.playerSelectionManager.allObjects.Add(unit);
		}

		GameObject computerUnit = MonoBehaviour.Instantiate(this.gameUnitPrefab) as GameObject;
		NavMeshAgent agent = computerUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.SetDestination(this.computerStartingPosition.position);
		}
		unitTransform = computerUnit.transform;
		unitTransform.position = this.computerStartingPosition.transform.position;
		unitTransform.SetParent(this.computerSelectionManager.transform);
		unit = computerUnit.GetComponent<GameUnit>();
		if (unit != null) {
			unit.teamFaction = this.computerSelectionManager.teamFaction;
			this.computerSelectionManager.allObjects.Add(unit);
		}
	}
}

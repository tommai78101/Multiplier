using UnityEngine;
using System.Collections.Generic;

public class StartGame : MonoBehaviour {
	public CanvasGroup attributePanelCanvasGroup;
	public Transform playerStartingPosition;
	public Transform computerStartingPosition;
	public GameObject gameUnitPrefab;
	public GameObject playerUnitCategory;
	public GameObject computerUnitCategory;


	public void StartButtonAction() {
		InitializationCheck();
		ClosePanel();
		LoadGame();
	}

	private void InitializationCheck() {
		bool flag = (this.attributePanelCanvasGroup == null) || (this.playerStartingPosition == null) || (this.computerStartingPosition == null) || (this.gameUnitPrefab == null)
			|| (this.playerUnitCategory == null) || (this.computerUnitCategory == null);
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
		unitTransform.SetParent(this.playerUnitCategory.transform);

		GameObject computerUnit = MonoBehaviour.Instantiate(this.gameUnitPrefab) as GameObject;
		NavMeshAgent agent = computerUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.SetDestination(this.computerStartingPosition.position);
		}
		unitTransform = computerUnit.transform;
		unitTransform.position = this.computerStartingPosition.transform.position;
		unitTransform.SetParent(this.computerUnitCategory.transform);
	}
}

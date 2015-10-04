using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SelectionManager : NetworkBehaviour {
	public List<GameObject> selectedObjects;
	public List<GameObject> allObjects;
	public List<GameObject> removeList;

	public Rect selectionBox;
	public Vector3 initialClick;
	public NetworkConnection authorityOwner;

	public bool isSelecting;

	void Start() {
		//If you need to use a different design instead of checking for hasAuthority, then it means
		//you will have to figure out how to do what you need to do, and this example will not
		//be sufficient enough to teach you more than given.
		if (!this.hasAuthority) {
			return;
		}

		this.InitializeList();
		this.selectionBox = new Rect();

		GameObject[] selectionManagers = GameObject.FindGameObjectsWithTag("SelectionManager");
		foreach (GameObject manager in selectionManagers) {
			SelectionManager selectManager = manager.GetComponent<SelectionManager>();
			if (selectManager == null || !selectManager.hasAuthority) {
				continue;
			}

			GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
			foreach (GameObject unit in units) {
				GameUnit gameUnit = unit.GetComponent<GameUnit>();
				if (gameUnit != null && !gameUnit.hasAuthority) {
					continue;
				}
				selectManager.allObjects.Add(unit);
			}
		}
	}

	void Update() {
		if (!this.hasAuthority) {
			return;
		}

		//This handles all the input actions the player has done to box select in the game.
		//Currently, it doesn't handle clicking to select.
		if (Input.GetMouseButtonDown(0)) {
			this.isSelecting = true;
			this.selectedObjects.Clear();
			this.initialClick = Input.mousePosition;

		}
		else if (Input.GetMouseButtonUp(0)) {
			this.SelectObjectsInRect();
			this.isSelecting = false;
			this.initialClick = -Vector3.one;
		}

		if (this.isSelecting && Input.GetMouseButton(0)) {
			this.selectionBox.Set(this.initialClick.x, Screen.height - this.initialClick.y, Input.mousePosition.x - this.initialClick.x, (Screen.height - Input.mousePosition.y) - (Screen.height - this.initialClick.y));
			if (this.selectionBox.width < 0) {
				this.selectionBox.x += this.selectionBox.width;
				this.selectionBox.width *= -1f;
			}
			if (this.selectionBox.height < 0) {
				this.selectionBox.y += this.selectionBox.height;
				this.selectionBox.height *= -1f;
			}
			this.TempSelectObjects();
		}

		if (this.removeList.Count > 0) {
			foreach (GameObject obj in this.removeList) {
				if (this.allObjects.Contains(obj)) {
					this.allObjects.Remove(obj);
				}
			}
			this.removeList.Clear();
		}
	}

	public void InitializeList() {
		if (this.selectedObjects == null) {
			this.selectedObjects = new List<GameObject>();
		}

		if (this.allObjects == null) {
			this.allObjects = new List<GameObject>();
		}
	}

	public void AddToRemoveList(GameObject obj) {
		if (!this.removeList.Contains(obj)) {
			this.removeList.Add(obj);
		}
	}

	private void TempSelectObjects() {
		foreach (GameObject obj in this.allObjects) {
			if (obj == null) {
				this.removeList.Add(obj);
				continue;
			}
			Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
			projectedPosition.y = Screen.height - projectedPosition.y;
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (this.selectionBox.Contains(projectedPosition)) {
				unit.isSelected = true;
			}
			else {
				unit.isSelected = false;
			}
		}
	}

	private void SelectObjectsInRect() {
		foreach (GameObject obj in this.allObjects) {
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (unit.isSelected) {
				if (!this.selectedObjects.Contains(obj)) {
					this.selectedObjects.Add(obj);
				}
			}
			else {
				if (this.selectedObjects.Contains(obj)) {
					this.selectedObjects.Remove(obj);
				}
			}
		}
	}
}

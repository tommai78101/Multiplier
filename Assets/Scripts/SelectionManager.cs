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
	public bool isBoxSelecting;

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
			if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
				ClearSelectObjects();
			}
			this.isSelecting = true;
			this.initialClick = Input.mousePosition;
		}
		else if (Input.GetMouseButtonUp(0)) {
			if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
				ClearSelectObjects();
			}
			SelectObjectAtPoint();
			SelectObjectsInRect();
			SelectObjects();
			this.isSelecting = false;
			this.isBoxSelecting = false;
			this.initialClick = -Vector3.one;
		}

		if (this.isSelecting && Input.GetMouseButton(0)) {
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
				this.isBoxSelecting = true;
			}
			this.selectionBox.Set(this.initialClick.x, Screen.height - this.initialClick.y, Input.mousePosition.x - this.initialClick.x, (Screen.height - Input.mousePosition.y) - (Screen.height - this.initialClick.y));
			if (this.selectionBox.width < 0) {
				this.selectionBox.x += this.selectionBox.width;
				this.selectionBox.width *= -1f;
			}
			if (this.selectionBox.height < 0) {
				this.selectionBox.y += this.selectionBox.height;
				this.selectionBox.height *= -1f;
			}
			TempRectSelectObjects();
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


	//-----------   Private class methods may all need refactoring   --------------------

	private void TempRectSelectObjects() {
		foreach (GameObject obj in this.allObjects) {
			if (obj == null) {
				//Because merging units will actually destroy units (as a resource), we now added a check to make sure
				//we don't call on NULL referenced objects, and remove them from the list.
				this.removeList.Add(obj);
				continue;
			}
			Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
			projectedPosition.y = Screen.height - projectedPosition.y;
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (this.selectionBox.Contains(projectedPosition)) {
				unit.isSelected = true;
			}
		}
	}

	private void SelectObjects() {
		foreach (GameObject obj in this.allObjects) {
			if (obj == null) {
				this.removeList.Add(obj);
				continue;
			}
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (unit != null) {
				if (this.selectedObjects.Contains(obj)) {
					unit.isSelected = true;
				}
			}
		}
	}

	private void SelectObjectsInRect() {
		foreach (GameObject obj in this.allObjects) {
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (this.isBoxSelecting) {
				Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
				projectedPosition.y = Screen.height - projectedPosition.y;
				if (this.selectionBox.Contains(projectedPosition)) {
					if (this.selectedObjects.Contains(obj)) {
						unit.isSelected = false;
						this.selectedObjects.Remove(obj);
					}
					else {
						unit.isSelected = true;
						this.selectedObjects.Add(obj);
					}
				}
			}
			else {
				if (unit.isSelected) {
					if (!this.selectedObjects.Contains(obj)) {
						this.selectedObjects.Add(obj);
					}
				}
			}
		}
	}

	private void ClearSelectObjects() {
		foreach (GameObject obj in this.selectedObjects) {
			if (obj == null) {
				continue;
			}
			GameUnit unit = obj.GetComponent<GameUnit>();
			unit.isSelected = false;
		}
		this.selectedObjects.Clear();
	}

	private void SelectObjectAtPoint() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray);
		foreach (RaycastHit hit in hits) {
			GameObject obj = hit.collider.gameObject;
			if (obj.tag.Equals("Unit")) {
				GameUnit unit = obj.GetComponent<GameUnit>();
				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
					if (this.allObjects.Contains(obj)) {
						if (!this.selectedObjects.Contains(obj)) {
							unit.isSelected = true;
							this.selectedObjects.Add(obj);
						}
						else if (this.selectedObjects.Contains(obj)) {
							unit.isSelected = false;
							this.selectedObjects.Remove(obj);
						}
					}
				}
				else {
					unit.isSelected = true;
					this.selectedObjects.Add(obj);
				}
			}
		}
	}
}

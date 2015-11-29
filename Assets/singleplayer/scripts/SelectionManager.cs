using UnityEngine;
using System.Collections.Generic;

namespace SinglePlayer {
	public class SelectionManager : MonoBehaviour {
		public List<GameUnit> singleSelectedObjects;
		public List<GameUnit> singleAllObjects;
		public List<GameUnit> singleRemoveList;
		public EnumTeam teamFaction;

		public Rect selectionBox;
		public Vector3 initialClick;
		public Camera minimapCamera;
		public Vector3 screenPoint;

		public bool isSelecting;
		public bool isBoxSelecting;

		void Start() {
			//If you need to use a different design instead of checking for hasAuthority, then it means
			//you will have to figure out how to do what you need to do, and this example will not
			//be sufficient enough to teach you more than given.
			this.InitializeList();
		}

		void Update() { 
			if (this.minimapCamera == null) {
				return;
			}

			//This handles all the input actions the player has done in the minimap.
			this.screenPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
			if (this.minimapCamera.rect.Contains(this.screenPoint) && Input.GetMouseButtonDown(1)) {
				if (this.singleSelectedObjects.Count > 0) {
					float mainX = (this.screenPoint.x - this.minimapCamera.rect.xMin) / (1.0f - this.minimapCamera.rect.xMin);
					float mainY = (this.screenPoint.y) / (this.minimapCamera.rect.yMax);
					Vector3 minimapScreenPoint = new Vector3(mainX, mainY, 0f);
					foreach (GameUnit unit in this.singleSelectedObjects) {
						if (unit != null) {
							unit.CastRay(true, minimapScreenPoint, this.minimapCamera);
						}
					}
				}
			}
			else {
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
					this.initialClick = -Vector3.one * 9999f;
				}
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

			foreach (GameUnit unit in this.singleAllObjects) {
				if (unit == null && !this.singleRemoveList.Contains(unit)) {
					this.singleRemoveList.Add(unit);
				}
			}

			for (int i = 0; i < this.singleSelectedObjects.Count; i++) {
				if (this.singleSelectedObjects[i] == null) {
					this.singleSelectedObjects.RemoveAt(i);
				}
			}

			if (this.singleRemoveList.Count > 0) {
				foreach (GameUnit unit in this.singleRemoveList) {
					if (this.singleAllObjects.Contains(unit)) {
						this.singleAllObjects.Remove(unit);
					}
				}
				this.singleRemoveList.Clear();
			}
		}

		public void InitializeList() {
			if (this.singleSelectedObjects == null) {
				this.singleSelectedObjects = new List<GameUnit>(100);
			}

			if (this.singleAllObjects == null) {
				this.singleAllObjects = new List<GameUnit>(100);
			}
			this.selectionBox = new Rect();
		}

		//-----------   Private class methods may all need refactoring   --------------------

		protected void TempRectSelectObjects() {
			foreach (GameUnit obj in this.singleAllObjects) {
				if (obj == null) {
					//Because merging units will actually destroy units (as a resource), we now added a check to make sure
					//we don't call on NULL referenced objects, and remove them from the list.
					this.singleRemoveList.Add(obj);
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

		protected void SelectObjects() {
			foreach (GameUnit obj in this.singleAllObjects) {
				if (obj == null) {
					this.singleRemoveList.Add(obj);
					continue;
				}
				GameUnit unit = obj.GetComponent<GameUnit>();
				if (unit != null) {
					if (this.singleSelectedObjects.Contains(obj)) {
						unit.isSelected = true;
					}
				}
			}
		}

		protected void SelectObjectsInRect() {
			foreach (GameUnit obj in this.singleAllObjects) {
				if (obj == null) {
					continue;
				}
				GameUnit unit = obj.GetComponent<GameUnit>();
				if (unit != null) {
					if (this.isBoxSelecting) {
						Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
						projectedPosition.y = Screen.height - projectedPosition.y;
						if (this.selectionBox.Contains(projectedPosition)) {
							if (this.singleSelectedObjects.Contains(obj)) {
								unit.isSelected = false;
								this.singleSelectedObjects.Remove(obj);
							}
							else {
								unit.isSelected = true;
								this.singleSelectedObjects.Add(obj);
							}
						}
					}
					else {
						if (unit.isSelected) {
							if (!this.singleSelectedObjects.Contains(obj)) {
								this.singleSelectedObjects.Add(obj);
							}
						}
					}
				}
			}
		}

		protected void ClearSelectObjects() {
			foreach (GameUnit obj in this.singleSelectedObjects) {
				if (obj == null) {
					continue;
				}
				GameUnit unit = obj.GetComponent<GameUnit>();
				unit.isSelected = false;
			}
			this.singleSelectedObjects.Clear();
		}

		protected void SelectObjectAtPoint() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hits = Physics.RaycastAll(ray);
			foreach (RaycastHit hit in hits) {
				GameObject obj = hit.collider.gameObject;
				if (obj.tag.Equals("SingleUnit")) {
					GameUnit unit = obj.GetComponent<GameUnit>();
					if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
						if (this.singleAllObjects.Contains(unit)) {
							if (!this.singleSelectedObjects.Contains(unit)) {
								unit.isSelected = true;
								this.singleSelectedObjects.Add(unit);
							}
							else if (this.singleSelectedObjects.Contains(unit)) {
								unit.isSelected = false;
								this.singleSelectedObjects.Remove(unit);
							}
						}
					}
					else {
						unit.isSelected = true;
						this.singleSelectedObjects.Add(unit);
					}
				}
			}
		}
	}
}
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace MultiPlayer {
	public class NewSelector : NetworkBehaviour {
		public UnitsSyncList selectedObjects;
		public NewSpawner spawner;

		public Rect selectionBox;
		public Vector3 initialClick;
		public NetworkConnection authorityOwner;
		public Camera minimapCamera;
		public Vector3 screenPoint;

		public bool isSelecting;
		public bool isBoxSelecting;

		public void Start() {
			//If you need to use a different design instead of checking for hasAuthority, then it means
			//you will have to figure out how to do what you need to do, and this example will not
			//be sufficient enough to teach you more than given.
			if (!this.hasAuthority) {
				return;
			}

			if (this.spawner == null) {
				this.spawner = this.GetComponent<NewSpawner>();
				if (this.spawner == null) {
					Debug.LogError("Unable to obtain NewSpawner object. Please check if NewSpawner component exists.");
				}
			}

			if (this.minimapCamera == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("Minimap");
				if (obj != null) {
					this.minimapCamera = obj.GetComponent<Camera>();
					if (this.minimapCamera == null) {
						Debug.LogError("Failure to obtain minimap camera.");
					}
				}
			}

			this.selectionBox = new Rect();
		}

		void Update() {
			if (!this.hasAuthority) {
				return;
			}
			if (this.minimapCamera == null) {
				return;
			}

			//This handles all the input actions the player has done in the minimap.
			this.screenPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
			if (this.minimapCamera.rect.Contains(this.screenPoint) && Input.GetMouseButtonDown(1)) {
				if (this.selectedObjects.Count > 0) {
					float mainX = (this.screenPoint.x - this.minimapCamera.rect.xMin) / (1.0f - this.minimapCamera.rect.xMin);
					float mainY = (this.screenPoint.y) / (this.minimapCamera.rect.yMax);
					Vector3 minimapScreenPoint = new Vector3(mainX, mainY, 0f);
					foreach (NewUnitStruct temp in this.selectedObjects) {
						NewGameUnit unit = temp.unit.GetComponent<NewGameUnit>();
						if (unit != null) {
							CastRay(unit, true, minimapScreenPoint, this.minimapCamera);
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
		}

		[Command]
		public void CmdDestroy(GameObject obj) {
			NetworkServer.Destroy(obj);
		}


		//-----------   Private class methods may all need refactoring   --------------------

		private void TempRectSelectObjects() {
			foreach (NewUnitStruct temp in this.spawner.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					//Because merging units will actually destroy units (as a resource), we now added a check to make sure
					//we don't call on NULL referenced objects, and remove them from the list.
					this.spawner.unitList.Remove(temp);
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
			foreach (NewUnitStruct temp in this.spawner.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.spawner.unitList.Remove(temp);
					continue;
				}
				if (this.selectedObjects.Contains(temp)) {
					NewGameUnit unit = obj.GetComponent<NewGameUnit>();
					if (unit != null) {
						unit.properties.isSelected = true;
					}
				}
			}
		}

		private void SelectObjectsInRect() {
			foreach (NewUnitStruct temp in this.spawner.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.spawner.unitList.Remove(temp);
					continue;
				}
				GameUnit unit = obj.GetComponent<GameUnit>();
				if (unit != null) {
					if (this.isBoxSelecting) {
						Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
						projectedPosition.y = Screen.height - projectedPosition.y;
						if (this.selectionBox.Contains(projectedPosition)) {
							if (this.selectedObjects.Contains(temp)) {
								unit.isSelected = false;
								this.selectedObjects.Remove(temp);
							}
							else {
								unit.isSelected = true;
								this.selectedObjects.Add(temp);
							}
						}
					}
					else {
						if (unit.isSelected) {
							if (!this.selectedObjects.Contains(temp)) {
								this.selectedObjects.Add(temp);
							}
						}
					}
				}
			}
		}

		private void ClearSelectObjects() {
			foreach (NewUnitStruct temp in this.spawner.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.spawner.unitList.Remove(temp);
					continue;
				}
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				unit.properties.isSelected = false;
			}
			this.selectedObjects.Clear();
		}

		private void SelectObjectAtPoint() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hits = Physics.RaycastAll(ray);
			foreach (RaycastHit hit in hits) {
				GameObject obj = hit.collider.gameObject;
				if (obj.tag.Equals("Unit")) {
					NewUnitStruct temp = new NewUnitStruct(obj);
					NewGameUnit unit = temp.unit.GetComponent<NewGameUnit>();
					if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
						if (this.spawner.unitList.Contains(temp)) {
							if (!this.selectedObjects.Contains(temp)) {
								unit.properties.isSelected = true;
								this.selectedObjects.Add(temp);
							}
							else if (this.selectedObjects.Contains(temp)) {
								unit.properties.isSelected = false;
								this.selectedObjects.Remove(temp);
							}
						}
					}
					else {
						if (unit != null) {
							unit.properties.isSelected = true;
							this.selectedObjects.Add(temp);
						}
					}
				}
			}
		}

		private void CastRay(NewGameUnit unit, bool isMinimap, Vector3 mousePosition, Camera minimapCamera) {
			Ray ray;
			if (isMinimap) {
				ray = minimapCamera.ViewportPointToRay(mousePosition);
			}
			else {
				ray = Camera.main.ScreenPointToRay(mousePosition);
			}
			RaycastHit[] hits = Physics.RaycastAll(ray, 500f);
			foreach (RaycastHit hit in hits) {
				if (hit.collider.gameObject.tag.Equals("Floor")) {
					unit.properties.targetPosition = hit.point;
					break;
				}
			}
		}
	}
}
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Common;

namespace MultiPlayer {

	public struct NewUnitStruct {
		public GameObject unit;

		public NewUnitStruct(GameObject unit) {
			this.unit = unit;
		}
	}

	public struct Split {
		public Transform owner;
		public Transform split;
		public Vector3 origin;
		public Vector3 rotationVector;
		public float elapsedTime;

		public Split(Transform owner, Transform split) {
			this.owner = owner;
			this.split = split;
			this.origin = owner.position;
			this.rotationVector = Quaternion.Euler(new Vector3(0f, Random.Range(-180f, 180f), 0f)) * (Vector3.one * 0.5f);
			this.rotationVector.y = 0f;
			this.elapsedTime = 0f;
		}

		public void Update() {
			Vector3 pos = Vector3.Lerp(this.origin, this.origin + this.rotationVector, this.elapsedTime);
			if (this.owner == null || this.owner == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.owner.position = pos;
			pos = Vector3.Lerp(this.origin, this.origin - this.rotationVector, this.elapsedTime);
			if (this.split == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.split.position = pos;

			this.elapsedTime += Time.deltaTime;
		}
	}

	public struct Merge {
		public Transform owner;
		public Transform merge;
		public Vector3 origin;
		public Vector3 ownerPosition;
		public Vector3 mergePosition;
		public Vector3 ownerScale;
		public float elapsedTime;
		public float scalingFactor;

		public Merge(Transform owner, Transform merge, float scalingFactor) {
			this.owner = owner;
			this.merge = merge;
			this.ownerPosition = owner.position;
			this.mergePosition = merge.position;
			this.origin = (this.ownerPosition + this.mergePosition) / 2f;
			this.elapsedTime = 0f;
			this.ownerScale = owner.localScale;
			this.scalingFactor = scalingFactor;
		}

		public void Update() {
			Vector3 pos = Vector3.Lerp(this.ownerPosition, this.origin, this.elapsedTime);
			if (this.owner == null || this.owner == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.owner.transform.position = pos;
			pos = Vector3.Lerp(this.mergePosition, this.origin, this.elapsedTime);
			if (this.merge == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.merge.transform.position = pos;

			//Scaling animation. Same persistent bug? It might be another mysterious bug.
			Vector3 scale = Vector3.Lerp(this.ownerScale, this.ownerScale * scalingFactor, this.elapsedTime);
			scale.y = this.ownerScale.y;
			this.owner.localScale = scale;
			this.merge.localScale = scale;

			this.elapsedTime += Time.deltaTime;
		}
	}


	public class SplitGroupSyncList : SyncListStruct<Split> {
	}

	public class MergeGroupSyncList : SyncListStruct<Merge> {
	}

	public class UnitsSyncList : SyncListStruct<NewUnitStruct> {
	}


	public class NewSpawner : NetworkBehaviour {
		public bool isPaused;
		public GameObject newGameUnitPrefab;
		public NetworkConnection owner;
		public SplitGroupSyncList splitList = new SplitGroupSyncList();
		public MergeGroupSyncList mergeList = new MergeGroupSyncList();
		public UnitsSyncList unitList = new UnitsSyncList();
		public UnitsSyncList removeList = new UnitsSyncList();
		public UnitsSyncList selectedList = new UnitsSyncList();
		public Rect selectionBox;
		public Camera minimapCamera;

		public bool isSelecting;
		public bool isBoxSelecting;
		private Vector3 initialClick;
		private Vector3 screenPoint;

		private bool moveCommandFlag;

		public void Start() {
			NetworkIdentity spawnerIdentity = this.GetComponent<NetworkIdentity>();
			this.owner = this.isServer ? spawnerIdentity.connectionToClient : spawnerIdentity.connectionToServer;
			Debug.Log("This is " + (this.isServer ? " Server." : " Client."));
			this.isPaused = false;
			this.moveCommandFlag = false;

			if (this.minimapCamera == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("Minimap");
				if (obj != null) {
					this.minimapCamera = obj.GetComponent<Camera>();
					if (this.minimapCamera == null) {
						Debug.LogError("Failure to obtain minimap camera.");
					}
				}
			}

			if (Camera.main.gameObject.GetComponent<PostRenderer>() == null) {
				PostRenderer renderer = Camera.main.gameObject.AddComponent<PostRenderer>();
				renderer.minimapCamera = this.minimapCamera;
			}

			this.selectionBox = new Rect();


			ServerInitialize();
		}

		[ServerCallback]
		public void ServerInitialize() {
			GameObject gameUnit = MonoBehaviour.Instantiate<GameObject>(this.newGameUnitPrefab);
			gameUnit.transform.SetParent(this.transform);
			gameUnit.transform.position = this.transform.position;
			NetworkIdentity unitIdentity = gameUnit.GetComponent<NetworkIdentity>();
			unitIdentity.localPlayerAuthority = true;
			NetworkServer.SpawnWithClientAuthority(gameUnit, this.owner);
			RpcOrganize();
		}

		[ClientRpc]
		public void RpcOrganize() {
			NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
			NewGameUnit[] units = GameObject.FindObjectsOfType<NewGameUnit>();
			foreach (NewSpawner spawner in spawners) {
				if (spawner.hasAuthority) {
					foreach (NewGameUnit unit in units) {
						if (unit.hasAuthority) {
							unit.transform.SetParent(spawner.transform);
							NewUnitStruct unitStruct = new NewUnitStruct();
							unitStruct.unit = unit.gameObject;
							this.unitList.Add(unitStruct);
						}
					}
				}
				else {
					foreach (NewGameUnit unit in units) {
						if (!unit.hasAuthority) {
							unit.transform.SetParent(spawner.transform);
						}
					}
				}
			}
		}

		[ClientRpc]
		public void RpcOrganizeUnit(GameObject obj) {
			NewGameUnit unit = obj.GetComponent<NewGameUnit>();
			NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
			foreach (NewSpawner spawner in spawners) {
				if (spawner.hasAuthority) {
					if (unit.hasAuthority) {
						unit.transform.SetParent(spawner.transform);
						continue;
					}
				}
				else {
					if (!unit.hasAuthority) {
						unit.transform.SetParent(spawner.transform);
						continue;
					}
				}
			}
		}

		public void Update() { 
			HandleInputs();
			ManageLists();
			HandleSelection();
		}

		[Command]
		public void CmdDestroy(GameObject obj) {
			NetworkServer.Destroy(obj);
		}

		[Command]
		public void CmdSpawn(GameObject obj) {
			NetworkServer.SpawnWithClientAuthority(obj, this.connectionToClient);
			RpcOrganizeUnit(obj);
		}

		private void HandleInputs() {
			if (Input.GetKeyUp(KeyCode.S)) {
				foreach (NewUnitStruct temp in this.unitList) {
					NewGameUnit newUnit = temp.unit.GetComponent<NewGameUnit>();
					if (!newUnit.properties.isSplitting && this.unitList.Count < 50) {
						GameObject unit = MonoBehaviour.Instantiate<GameObject>(temp.unit);
						unit.name = "NewGameUnit";
						unit.transform.SetParent(this.transform);
						CmdSpawn(unit);
						this.splitList.Add(new Split(temp.unit.transform, unit.transform));
					}
				}
			}
			//if (Input.GetKeyUp(KeyCode.L)) {
			//	Debug.Log("Damage time!");
			//	CmdTakeDamage(1);
			//}

			if (Input.GetMouseButtonUp(1)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					Debug.Log("Moving time!");
					//foreach (NewUnitStruct temp in this.unitList) {
					//	NewGameUnit unit = temp.unit.GetComponent<NewGameUnit>();
					//	unit.properties.targetPosition = hit.point;
					//}
				}
			}

			//if (this.properties.targetPosition != -9999 * Vector3.one) {
			//	NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
			//	agent.SetDestination(this.properties.targetPosition);
			//}
		}

		private void ManageLists() {
			if (this.unitList.Count > 0) {
				for (int i = 0; i < this.unitList.Count; i++) {
					NewUnitStruct temp = this.unitList[i];
					if (temp.unit == null) {
						this.unitList.RemoveAt(i);
					}
				}
			}
			if (this.splitList.Count > 0) {
				for (int i = 0; i < this.splitList.Count; i++) {
					Split splitGroup = this.splitList[i];
					if (splitGroup.owner == null || splitGroup.split == null) {
						this.splitList.RemoveAt(i);
					}
					if (splitGroup.elapsedTime > 1f) {
						this.unitList.Add(new NewUnitStruct(splitGroup.split.gameObject));
						this.splitList.RemoveAt(i);
					}
					else {
						splitGroup.Update();
						this.splitList[i] = splitGroup;
					}
				}
			}
			if (this.mergeList.Count > 0) {
				for (int i = 0; i < this.mergeList.Count; i++) {
					Merge mergeGroup = this.mergeList[i];
					if (mergeGroup.elapsedTime > 1f) {
						if (mergeGroup.owner != null) {
							Debug.LogWarning("TODO: Do something about merging.");
						}
						if (mergeGroup.merge != null) {
							NewUnitStruct temp = new NewUnitStruct();
							temp.unit = mergeGroup.merge.gameObject;
							this.removeList.Add(temp);
						}
						this.mergeList.RemoveAt(i);
					}
					else {
						mergeGroup.Update();
						this.mergeList[i] = mergeGroup;
					}
				}
			}
			if (this.removeList.Count > 0) {
				foreach (NewUnitStruct temp in this.removeList) {
					CmdDestroy(temp.unit);
				}
				this.removeList.Clear();
			}
		}

		private void HandleSelection() {
			if (this.minimapCamera == null) {
				return;
			}

			//This handles all the input actions the player has done in the minimap.
			this.screenPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
			if (this.minimapCamera.rect.Contains(this.screenPoint) && Input.GetMouseButtonDown(1)) {
				if (this.selectedList.Count > 0) {
					float mainX = (this.screenPoint.x - this.minimapCamera.rect.xMin) / (1.0f - this.minimapCamera.rect.xMin);
					float mainY = (this.screenPoint.y) / (this.minimapCamera.rect.yMax);
					Vector3 minimapScreenPoint = new Vector3(mainX, mainY, 0f);
					foreach (NewUnitStruct temp in this.selectedList) {
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

		//-----------   Private class methods may all need refactoring   --------------------

		private void TempRectSelectObjects() {
			foreach (NewUnitStruct temp in this.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					//Because merging units will actually destroy units (as a resource), we now added a check to make sure
					//we don't call on NULL referenced objects, and remove them from the list.
					this.unitList.Remove(temp);
					continue;
				}
				Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
				projectedPosition.y = Screen.height - projectedPosition.y;
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				if (this.selectionBox.Contains(projectedPosition)) {
					unit.properties.isSelected = true;
				}
			}
		}

		private void SelectObjects() {
			foreach (NewUnitStruct temp in this.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.unitList.Remove(temp);
					continue;
				}
				if (this.selectedList.Contains(temp)) {
					NewGameUnit unit = obj.GetComponent<NewGameUnit>();
					if (unit != null) {
						unit.properties.isSelected = true;
					}
				}
			}
		}

		private void SelectObjectsInRect() {
			foreach (NewUnitStruct temp in this.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.unitList.Remove(temp);
					continue;
				}
				GameUnit unit = obj.GetComponent<GameUnit>();
				if (unit != null) {
					if (this.isBoxSelecting) {
						Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
						projectedPosition.y = Screen.height - projectedPosition.y;
						if (this.selectionBox.Contains(projectedPosition)) {
							if (this.selectedList.Contains(temp)) {
								unit.isSelected = false;
								this.selectedList.Remove(temp);
							}
							else {
								unit.isSelected = true;
								this.selectedList.Add(temp);
							}
						}
					}
					else {
						if (unit.isSelected) {
							if (!this.selectedList.Contains(temp)) {
								this.selectedList.Add(temp);
							}
						}
					}
				}
			}
		}

		private void ClearSelectObjects() {
			foreach (NewUnitStruct temp in this.unitList) {
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.unitList.Remove(temp);
					continue;
				}
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				unit.properties.isSelected = false;
			}
			this.selectedList.Clear();
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
						if (this.unitList.Contains(temp)) {
							if (!this.selectedList.Contains(temp)) {
								unit.properties.isSelected = true;
								this.selectedList.Add(temp);
							}
							else if (this.selectedList.Contains(temp)) {
								unit.properties.isSelected = false;
								this.selectedList.Remove(temp);
							}
						}
					}
					else {
						if (unit != null) {
							unit.properties.isSelected = true;
							this.selectedList.Add(temp);
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

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Common;
using Analytics;

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

		public Split(Transform owner, Transform split, float angle) {
			this.owner = owner;
			this.split = split;
			this.origin = owner.position;
			this.rotationVector = Quaternion.Euler(new Vector3(0f, angle, 0f)) * (Vector3.one * 0.5f);
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
		public GameObject newGameUnitPrefab;
		public NetworkConnection owner;
		public SplitGroupSyncList splitList = new SplitGroupSyncList();
		public MergeGroupSyncList mergeList = new MergeGroupSyncList();
		public UnitsSyncList unitList = new UnitsSyncList();
		public UnitsSyncList selectedList = new UnitsSyncList();
		public Rect selectionBox;
		public Camera minimapCamera;
		public GameObject starterObjects;
		public static NewSpawner Instance;

		public bool isSelecting;
		public bool isBoxSelecting;
		public bool isGameStart = false;
		public bool isPaused;

		private Vector3 initialClick;
		private Vector3 screenPoint;
		private NewChanges changes;
		private PostRenderer selectionBoxRenderer;
		private NewStarter playerStarterObject;
		private bool isUnitListEmpty;
		private bool doNotAllowMerging;

		public static int colorCode = 0;
		public static int MAX_UNIT_LIMIT = 16;

		public void Awake() {
			NewSpawner.Instance = this;
		}

		public void Start() {
			//This is used to obtain inactive start locations. Start locations can randomly
			//be set to inactive, so I need a way to obtain these inactive game objects.
			this.starterObjects = GameObject.FindGameObjectWithTag("Respawn");
			if (this.starterObjects == null) {
				Debug.LogError("Cannot find starter object in scene.");
			}

			if (!this.hasAuthority) {
				return;
			}

			NetworkIdentity spawnerIdentity = this.GetComponent<NetworkIdentity>();
			if (!spawnerIdentity.localPlayerAuthority) {
				spawnerIdentity.localPlayerAuthority = true;
			}
			this.owner = this.isServer ? spawnerIdentity.connectionToClient : spawnerIdentity.connectionToServer;
			this.isPaused = false;

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

				//NOTE(Thompson): See NOTE in NetworkManagerActions.StartLANClient().
				renderer.enabled = false;

				//NOTE(Thompson): Setting the renderer to a variable, so I can later on check to see
				//if the renderer is disabled or not. If disabled, disallow unit selection.
				this.selectionBoxRenderer = renderer;
			}

			this.selectionBox = new Rect();
			this.initialClick = Vector3.one * -9999f;
			this.screenPoint = this.initialClick;
			this.isGameStart = false;
			this.isUnitListEmpty = true;
			//NOTE(Thompson): This means you are allowed to merge. This checks if there exists one or more LV1 game unit available.
			this.doNotAllowMerging = false;
			//NOTE(Thompson): NewSpawner needs to keep track of where the first game unit is spawned at. This is set in CmdInitialize().
			this.playerStarterObject = null;
			this.changes.Clear();

			CmdInitialize(this.gameObject);

		}

		[Command]
		public void CmdSetReadyFlag(bool value) {
			if (value) {
				RpcSetReadyFlag();
			}
		}

		[ClientRpc]
		public void RpcSetReadyFlag() {
			this.isGameStart = true;
		}

		[Command]
		public void CmdAddUnit(GameObject obj, GameObject spawner) {
			NewSpawner newSpawner = spawner.GetComponent<NewSpawner>();
			if (newSpawner != null) {
				newSpawner.unitList.Add(new NewUnitStruct(obj));
				newSpawner.isUnitListEmpty = false;
			}
		}

		[ClientRpc]
		public void RpcAdd(GameObject obj, GameObject spawner) {
			if (this.hasAuthority) {
				CmdAddUnit(obj, spawner);
			}
		}

		[Command]
		public void CmdInitialize(GameObject spawner) {
			NetworkIdentity spawnerID = spawner.GetComponent<NetworkIdentity>();

			//Only the server choose what color values to use. Client values do not matter.
			int colorValue = NewSpawner.colorCode;
			NewSpawner.colorCode = (++NewSpawner.colorCode) % 3;
			Color color;
			switch (colorValue) {
				default:
					color = Color.gray;
					break;
				case 0:
					color = Color.yellow;
					break;
				case 1:
					color = Color.blue;
					break;
				case 2:
					color = Color.green;
					break;
			}


			GameObject gameUnit = MonoBehaviour.Instantiate<GameObject>(this.newGameUnitPrefab);
			gameUnit.name = gameUnit.name.Substring(0, gameUnit.name.Length - "(Clone)".Length);
			gameUnit.transform.SetParent(this.transform);
			gameUnit.transform.position = this.transform.position;
			NewGameUnit b = gameUnit.GetComponent<NewGameUnit>();
			NewChanges changes = b.CurrentProperty();
			if (!changes.isInitialized) {
				changes.isInitialized = false;
				changes.teamColor = color;
				changes.teamFactionID = (int) (Random.value * 100f); //This is never to be changed.
			}
			b.NewProperty(changes);
			NetworkServer.SpawnWithClientAuthority(b.gameObject, spawnerID.clientAuthorityOwner);

			RpcAdd(gameUnit, spawner);
			RpcFilter(b.netId, spawnerID.netId);
		}

		[ClientRpc]
		public void RpcFilter(NetworkInstanceId unitID, NetworkInstanceId spawnerID) {
			GameObject obj = ClientScene.FindLocalObject(unitID);
			GameObject spawnerObject = ClientScene.FindLocalObject(spawnerID);
			obj.transform.SetParent(spawnerObject.transform);
			NewGameUnit unit = obj.GetComponent<NewGameUnit>();
			if (unit.hasAuthority) {
				unit.SetTeamColor(unit.properties.teamColor); //NOTE(Thompson): This has to do with triggering the SyncVar's hook.
				Vector3 pos = obj.transform.position;
				pos.y = Camera.main.transform.position.y;
				pos.z -= 5f;
				Camera.main.transform.position = pos;
			}

			NewGameUnit[] units = GameObject.FindObjectsOfType<NewGameUnit>();
			NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
			for (int i = 0; i < units.Length; i++) {
				if (!units[i].hasAuthority) {
					for (int j = 0; j < spawners.Length; j++) {
						if (!spawners[j].hasAuthority) {
							units[i].transform.SetParent(spawners[j].transform);
							units[i].SetTeamColor(units[i].properties.teamColor); //NOTE(Thompson): This has to do with triggering the SyncVar's hook.
						}
					}
				}
			}



			//NewGameUnit[] units = GameObject.FindObjectsOfType<NewGameUnit>();
			//List<NetworkStartPosition> starters = new List<NetworkStartPosition>();
			//foreach (Transform child in this.starterObjects.transform) {
			//	child.gameObject.SetActive(true);
			//	NetworkStartPosition pos = child.GetComponent<NetworkStartPosition>();
			//	if (pos != null) {
			//		starters.Add(pos);
			//	}
			//}

			//try {
			//	NewStarter enemyStarterObject = null;
			//	for (int i = 0; i < starters.Count; i++) {
			//		NewStarter starter = starters[i].GetComponent<NewStarter>();
			//		if (starter != null) {
			//			if (!starter.GetIsTakenFlag()) {
			//				this.playerStarterObject = starter;
			//				starter.SetIsTakenFlag(true);
			//				break;
			//			}
			//			else {
			//				enemyStarterObject = starter;
			//				continue;
			//			}
			//		}
			//	}
			//	Transform cameraTarget = null;
			//	for (int i = 0; i < units.Length; i++) {
			//		if (units[i].hasAuthority) {
			//			units[i].transform.SetParent(playerStarterObject.transform);
			//			units[i].SetTeamColor(units[i].properties.teamColor); //NOTE(Thompson): This has to do with triggering the SyncVar's hook.
			//			cameraTarget = units[i].transform;
			//		}
			//		else {
			//			units[i].transform.SetParent(enemyStarterObject.transform);
			//			units[i].SetTeamColor(units[i].properties.teamColor); //NOTE(Thompson): This has to do with triggering the SyncVar's hook.
			//		}
			//	}
			//	if (cameraTarget != null && this.hasAuthority) {
			//		Vector3 pos = cameraTarget.position;
			//		pos.y = Camera.main.transform.position.y;
			//		pos.z -= 5f;
			//		Camera.main.transform.position = pos;
			//	}
			//}
			//catch (System.Exception e) {
			//	Debug.LogError("Unable to obtain all start positions, or there's a missing start locations. Error message: " + e.ToString());
			//}
		}

		[ClientRpc]
		public void RpcOrganizeUnit(GameObject owner, GameObject split) {
			split.transform.SetParent(owner.transform.parent);
		}

		public void Update() {
			//NOTE(Thompson): Common codes for server and clients go here.
			if (!this.hasAuthority) {
				ManageNonAuthorityLists();
				return;
			}

			if (!this.isGameStart && this.isUnitListEmpty) {
				return;
			}

			HandleSelection();
			HandleInputs();
			ManageLists();
		}

		[Command]
		public void CmdDestroy(GameObject obj) {
			NetworkServer.Destroy(obj);
		}

		[Command]
		public void CmdSplitSpawn(GameObject owner, NetworkIdentity ownerIdentity) {
			NewGameUnit unit = owner.GetComponent<NewGameUnit>();
			NewChanges changes = unit.CurrentProperty();
			changes.isSelected = false;
			changes.isSplitting = true;
			changes.teamColor = unit.GetTeamColor();
			unit.NewProperty(changes);
			GameObject split = MonoBehaviour.Instantiate<GameObject>(owner);
			split.name = "NewGameUnit";
			split.transform.SetParent(this.transform);
			split.transform.position = unit.transform.position;
			unit = split.GetComponent<NewGameUnit>();
			unit.NewProperty(changes);
			NetworkServer.SpawnWithClientAuthority(split, ownerIdentity.clientAuthorityOwner);
			RpcAddSplit(owner, split, changes, Random.Range(-180f, 180f));
			//this.splitList.Add(new Split(temp.transform, unit.transform));
			RpcOrganizeUnit(owner, split);
		}

		[ClientRpc]
		public void RpcAddSplit(GameObject owner, GameObject split, NewChanges changes, float angle) {
			if (owner != null && split != null) {
				NewGameUnit splitUnit = split.GetComponent<NewGameUnit>();
				if (splitUnit != null) {
					splitUnit.NewProperty(changes);
				}
				else {
					Debug.LogWarning("SplitUnit does not exist.");
				}
				this.splitList.Add(new Split(owner.transform, split.transform, angle));
			}
			else {
				string value1 = (owner == null) ? " Owner is null." : "";
				string value2 = (split == null) ? " Split is null." : "";
				Debug.LogWarning(value1 + value2);
			}
		}

		[Command]
		public void CmdMergeUpdate(GameObject owner, GameObject merge, NewChanges changes) {
			RpcAddMerge(owner, merge, changes);
		}

		[ClientRpc]
		public void RpcAddMerge(GameObject owner, GameObject merge, NewChanges changes) {
			if (owner != null && merge != null) {
				NewGameUnit ownerUnit = owner.GetComponent<NewGameUnit>();
				if (ownerUnit != null) {
					this.mergeList.Add(new Merge(owner.transform, merge.transform, ownerUnit.properties.scalingFactor));
				}
			}
		}

		[Command]
		public void CmdUpdateUnitProperty(GameObject unit, NewChanges changes) {
			if (unit != null) {
				NewGameUnit gameUnit = unit.GetComponent<NewGameUnit>();
				if (gameUnit != null) {
					gameUnit.NewProperty(changes);
				}
			}
		}

		[Command]
		public void CmdRemoveUnitList(GameObject obj) {
			if (obj != null) {
				this.unitList.Remove(new NewUnitStruct(obj));
			}
			else {
				for (int i = this.unitList.Count - 1; i >= 0; i--) {
					if (this.unitList[i].unit == null) {
						this.unitList.RemoveAt(i);
					}
				}
			}
		}

		[Command]
		public void CmdShowReport() {
			RpcShowReport();
		}

		[ClientRpc]
		public void RpcShowReport() {
			NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
			if (spawners.Length > 0) {
				for (int i = 0; i < spawners.Length; i++) {
					if (spawners[i].hasAuthority) {
						spawners[i].isGameStart = false;
						GameMetricLogger.ShowPrintLog();
						break;
					}
				}
			}
		}

		//-----------   Private class methods may all need refactoring   --------------------

		private void HandleInputs() {
			if (Input.GetKeyUp(KeyCode.S)) {
				foreach (NewUnitStruct temp in this.selectedList) {
					NewGameUnit newUnit = temp.unit.GetComponent<NewGameUnit>();
					if (!newUnit.properties.isSplitting && this.unitList.Count < NewSpawner.MAX_UNIT_LIMIT && newUnit.properties.level == 1) {
						CmdSplitSpawn(temp.unit, temp.unit.GetComponent<NetworkIdentity>());
					}
					else {
						if (newUnit != null) {
							this.changes = newUnit.CurrentProperty();
							this.changes.isSelected = false;
							this.changes.isSplitting = false;
							newUnit.NewProperty(this.changes);
						}
					}
				}
				this.selectedList.Clear();
			}
			else if (Input.GetKeyUp(KeyCode.D)) {
				NewGameUnit owner = null, merge = null;
				if (this.selectedList.Count == 1) {
					owner = this.selectedList[0].unit.GetComponent<NewGameUnit>();
					this.changes = owner.CurrentProperty();
					this.changes.isSelected = false;
					owner.NewProperty(this.changes);
				}
				else {
					for (int i = this.selectedList.Count - 1; i >= 1; i--) {
						owner = this.selectedList[i].unit.GetComponent<NewGameUnit>();
						if (owner != null && !owner.properties.isMerging) {
							for (int j = i - 1; j >= 0; j--) {
								merge = this.selectedList[j].unit.GetComponent<NewGameUnit>();
								this.doNotAllowMerging = false;
								if (owner.properties.level == 1 && merge.properties.level == 1) {
									CheckAvailableResource();
								}
								if (merge != null && !merge.properties.isMerging && merge.properties.level == owner.properties.level && !this.doNotAllowMerging) {
									this.changes = owner.CurrentProperty();
									changes.isMerging = true;
									changes.isSelected = false;
									changes.newLevel++;
									CmdUpdateUnitProperty(owner.gameObject, this.changes);
									CmdUpdateUnitProperty(merge.gameObject, this.changes);
									CmdMergeUpdate(owner.gameObject, merge.gameObject, this.changes);
									this.selectedList.RemoveAt(i);
									i--;
									this.selectedList.RemoveAt(j);
									j--;
									break;
								}
								else {
									if (merge != null) {
										this.changes = merge.CurrentProperty();
										this.changes.isSelected = false;
										this.changes.isMerging = false;
										merge.NewProperty(this.changes);
										//this.selectedList.RemoveAt(j);
										//i--;
										//j--;
									}
								}
							}
							if (merge == null) {
								this.changes = owner.CurrentProperty();
								this.changes.isSelected = false;
								this.changes.isMerging = false;
								owner.NewProperty(this.changes);
								//this.selectedList.RemoveAt(i);
								//i--;
							}
						}
						else if (owner != null) {
							this.changes = owner.CurrentProperty();
							this.changes.isSelected = false;
							this.changes.isMerging = false;
							owner.NewProperty(this.changes);
							//this.selectedList.RemoveAt(i);
							//i--;
						}
					}
				}
			}

			if (Input.GetMouseButtonUp(1)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray);
				foreach (RaycastHit hit in hits) {
					if (hit.collider.gameObject.tag.Equals("Floor")) {
						foreach (NewUnitStruct temp in this.selectedList) {
							if (temp.unit == null) {
								continue;
							}
							NewGameUnit unit = temp.unit.GetComponent<NewGameUnit>();
							this.changes = unit.CurrentProperty();
							this.changes.mousePosition = hit.point;
							this.changes.isCommanded = true;
							CmdUpdateUnitProperty(temp.unit, this.changes);
						}
					}
				}
			}
		}

		private void CheckAvailableResource() {
			//NOTE(Thompson): In this game, LV1 game units are resources. Splitting is
			//only available to LV1 game units, so without LV1 game units, the player is
			//doomed to fail.

			int levelOneUnitCount = 0;
			int selectedLevelOneUnitCount = 0;
			for (int i = 0; i < this.unitList.Count; i++) {
				NewGameUnit unit = this.unitList[i].unit.GetComponent<NewGameUnit>();
				if (unit != null && unit.properties.level == 1) {
					levelOneUnitCount++;
				}
			}
			for (int i = 0; i < this.selectedList.Count; i++) {
				NewGameUnit unit = this.selectedList[i].unit.GetComponent<NewGameUnit>();
				if (unit != null && unit.properties.level == 1 && unit.properties.isSelected) {
					selectedLevelOneUnitCount++;
				}
			}
			if (selectedLevelOneUnitCount <= 2) {
				if (selectedLevelOneUnitCount == levelOneUnitCount || levelOneUnitCount <= 2) {
					this.doNotAllowMerging = true;
				}
			}
		}

		private void ManageLists() {
			if (this.splitList.Count > 0) {
				for (int i = this.splitList.Count - 1; i >= 0; i--) {
					Split splitGroup = this.splitList[i];
					if (splitGroup.owner == null || splitGroup.split == null) {
						this.splitList.Remove(splitGroup);
					}
					if (splitGroup.elapsedTime > 1f) {
						NewGameUnit unit = splitGroup.owner.GetComponent<NewGameUnit>();
						this.changes = unit.CurrentProperty();
						this.changes.isSelected = false;
						this.changes.isSplitting = false;
						CmdUpdateUnitProperty(splitGroup.owner.gameObject, this.changes);
						unit = splitGroup.split.GetComponent<NewGameUnit>();
						CmdUpdateUnitProperty(splitGroup.split.gameObject, this.changes);
						this.unitList.Add(new NewUnitStruct(splitGroup.split.gameObject));
						this.splitList.Remove(splitGroup);

						GameMetricLogger.Increment(GameMetricOptions.Splits);
					}
					else {
						splitGroup.Update();
						this.splitList[i] = splitGroup;
					}
				}
			}
			if (this.mergeList.Count > 0) {
				for (int i = this.mergeList.Count - 1; i >= 0; i--) {
					Merge mergeGroup = this.mergeList[i];
					if (mergeGroup.elapsedTime > 1f) {
						if (mergeGroup.owner != null) {
							NewGameUnit unit = mergeGroup.owner.gameObject.GetComponent<NewGameUnit>();
							this.changes = unit.CurrentProperty();
							this.changes.isMerging = false;
							this.changes.isSelected = false;
							//changes.newLevel = unit.properties.level + 1;
							CmdUpdateUnitProperty(mergeGroup.owner.gameObject, this.changes);
						}
						if (mergeGroup.merge != null) {
							NewUnitStruct temp = new NewUnitStruct();
							temp.unit = mergeGroup.merge.gameObject;
							this.unitList.Remove(temp);
							CmdDestroy(temp.unit);
						}
						this.mergeList.RemoveAt(i);

						GameMetricLogger.Increment(GameMetricOptions.Merges);
					}
					else {
						mergeGroup.Update();
						this.mergeList[i] = mergeGroup;
					}
				}
			}
			if (this.unitList.Count > 0) {
				for (int i = this.unitList.Count - 1; i >= 0; i--) {
					if (this.unitList[i].unit == null) {
						this.unitList.RemoveAt(i);
						continue;
					}
					NetworkIdentity id = this.unitList[i].unit.GetComponent<NetworkIdentity>();
					if (!id.hasAuthority) {
						CmdRemoveUnitList(this.unitList[i].unit);
					}
				}
			}
			else {
				CmdShowReport();
			}
		}

		private void ManageNonAuthorityLists() {
			if (this.splitList.Count > 0) {
				for (int i = this.splitList.Count - 1; i >= 0; i--) {
					Split splitGroup = this.splitList[i];
					if (splitGroup.owner == null || splitGroup.split == null) {
						this.splitList.Remove(splitGroup);
					}
					if (splitGroup.elapsedTime > 1f) {
						NewGameUnit unit = splitGroup.owner.GetComponent<NewGameUnit>();
						NewChanges changes = unit.CurrentProperty();
						changes.isSelected = false;
						changes.isSplitting = false;
						//CmdUpdateUnitProperty(splitGroup.owner.gameObject, changes);
						unit.NewProperty(changes);
						unit = splitGroup.split.GetComponent<NewGameUnit>();
						unit.NewProperty(changes);
						//CmdUpdateUnitProperty(splitGroup.split.gameObject, changes);
						//this.unitList.Add(new NewUnitStruct(splitGroup.split.gameObject));
						this.splitList.Remove(splitGroup);

						//GameMetricLogger.Increment(GameMetricOptions.Splits);
					}
					else {
						splitGroup.Update();
						this.splitList[i] = splitGroup;
					}
				}
			}
			if (this.mergeList.Count > 0) {
				for (int i = this.mergeList.Count - 1; i >= 0; i--) {
					Merge mergeGroup = this.mergeList[i];
					if (mergeGroup.elapsedTime > 1f) {
						if (mergeGroup.merge != null) {
							NewGameUnit unit = mergeGroup.merge.GetComponent<NewGameUnit>();
							if (unit != null) {
								NewChanges changes = unit.CurrentProperty();
								changes.damage = unit.properties.maxHealth;
								unit.NewProperty(changes);
							}
							NewUnitStruct temp = new NewUnitStruct();
							temp.unit = unit.gameObject;
							this.unitList.Remove(temp);
							//CmdDestroy(temp.unit);
						}
						this.mergeList.RemoveAt(i);

						//GameMetricLogger.Increment(GameMetricOptions.Merges);
					}
					else {
						mergeGroup.Update();
						this.mergeList[i] = mergeGroup;
					}
				}
			}
		}

		private void HandleSelection() {
			if (this.minimapCamera == null || !this.selectionBoxRenderer.enabled) {
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

		private void TempRectSelectObjects() {
			for (int i = this.unitList.Count - 1; i >= 0; i--) {
				NewUnitStruct temp = this.unitList[i];
				if (temp.unit == null) {
					CmdRemoveUnitList(this.unitList[i].unit);
					continue;
				}
				Vector3 projectedPosition = Camera.main.WorldToScreenPoint(temp.unit.transform.position);
				projectedPosition.y = Screen.height - projectedPosition.y;
				if (this.selectionBox.Contains(projectedPosition)) {
					NewGameUnit unit = temp.unit.GetComponent<NewGameUnit>();
					if (unit.properties.isSelected || !unit.hasAuthority) {
						continue;
					}
					if (temp.unit == null) {
						CmdRemoveUnitList(this.unitList[i].unit);
						continue;
					}
					this.changes = unit.CurrentProperty();
					this.changes.isSelected = true;
					CmdUpdateUnitProperty(unit.gameObject, this.changes);
				}
				else {
					continue;
				}
			}
		}

		private void SelectObjects() {
			for (int i = this.unitList.Count - 1; i >= 0; i--) {
				NewUnitStruct temp = this.unitList[i];
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.unitList.Remove(temp);
					continue;
				}
				if (this.selectedList.Contains(temp)) {
					NewGameUnit unit = obj.GetComponent<NewGameUnit>();
					if (unit != null && unit.hasAuthority) {
						this.changes = unit.CurrentProperty();
						changes.isSelected = true;
						CmdUpdateUnitProperty(unit.gameObject, this.changes);
					}
				}
			}
		}

		private void SelectObjectsInRect() {
			for (int i = this.unitList.Count - 1; i >= 0; i--) {
				NewUnitStruct temp = this.unitList[i];
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					this.unitList.Remove(temp);
					continue;
				}
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				Vector3 projectedPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
				projectedPosition.y = Screen.height - projectedPosition.y;
				if (unit != null && unit.hasAuthority) {
					if (this.isBoxSelecting) {
						if (this.selectionBox.Contains(projectedPosition)) {
							if (this.selectedList.Contains(temp)) {
								this.changes = unit.CurrentProperty();
								this.changes.isSelected = false;
								CmdUpdateUnitProperty(unit.gameObject, this.changes);
								this.selectedList.Remove(temp);
							}
							else {
								this.changes = unit.CurrentProperty();
								changes.isSelected = true;
								CmdUpdateUnitProperty(unit.gameObject, this.changes);
								this.selectedList.Add(temp);
							}
						}
					}
					else {
						if (unit.properties.isSelected) {
							if (!this.selectedList.Contains(temp)) {
								this.selectedList.Add(temp);
							}
						}
						else {
							if (!this.selectionBox.Contains(projectedPosition)) {
								if (this.selectedList.Contains(temp)) {
									this.changes = unit.CurrentProperty();
									this.changes.isSelected = false;
									CmdUpdateUnitProperty(unit.gameObject, this.changes);
									this.selectedList.Remove(temp);
								}
							}
							else {
								if (!this.selectedList.Contains(temp)) {
									this.changes = unit.CurrentProperty();
									;
									changes.isSelected = true;
									CmdUpdateUnitProperty(unit.gameObject, this.changes);
									this.selectedList.Add(temp);
								}
							}
						}
					}
				}
			}
		}

		private void ClearSelectObjects() {
			for (int i = this.unitList.Count - 1; i >= 0; i--) {
				NewUnitStruct temp = this.unitList[i];
				if (temp.unit == null) {
					CmdRemoveUnitList(this.unitList[i].unit);
					continue;
				}
				GameObject obj = temp.unit.gameObject;
				if (obj == null) {
					CmdRemoveUnitList(this.unitList[i].unit);
					continue;
				}
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				changes = unit.CurrentProperty();
				changes.isSelected = false;
				CmdUpdateUnitProperty(unit.gameObject, this.changes);
			}
			this.selectedList.Clear();
		}

		private void SelectObjectAtPoint() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hits = Physics.RaycastAll(ray);
			foreach (RaycastHit hit in hits) {
				GameObject obj = hit.collider.gameObject;
				if (obj.tag.Equals("Unit")) { //May need to change this back to checking if Tag is "Unit".
					NewUnitStruct temp = new NewUnitStruct(obj);
					NewGameUnit unit = temp.unit.GetComponent<NewGameUnit>();
					if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
						if (this.unitList.Contains(temp) && unit.hasAuthority) {
							if (!this.selectedList.Contains(temp)) {
								this.changes = unit.CurrentProperty();
								changes.isSelected = true;
								unit.NewProperty(changes);
								CmdUpdateUnitProperty(unit.gameObject, this.changes);
								this.selectedList.Add(temp);
							}
							else if (this.selectedList.Contains(temp)) {
								this.changes = unit.CurrentProperty();
								this.changes.isSelected = false;
								unit.NewProperty(changes);
								CmdUpdateUnitProperty(unit.gameObject, this.changes);
								this.selectedList.Remove(temp);
							}
						}
					}
					else {
						if (unit != null && unit.hasAuthority) {
							this.changes = unit.CurrentProperty();
							changes.isSelected = true;
							unit.NewProperty(changes);
							CmdUpdateUnitProperty(unit.gameObject, this.changes);
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
					this.changes = unit.CurrentProperty();
					this.changes.mousePosition = hit.point;
					CmdUpdateUnitProperty(unit.gameObject, this.changes);
					break;
				}
			}
		}
	}
}

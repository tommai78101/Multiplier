using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Common;
using MultiPlayer;

public class SingleStartGame : MonoBehaviour {
	public GameObject spawnPrefab;
	public GameObject selectionManagerPrefab;
	public GameObject splitManagerPrefab;
	public GameObject mergeManagerPrefab;
	public GameObject parent;
	public Camera minimapCamera;
	public CanvasGroup attributePanelGroup;

	public void StartGame() {
		if (this.attributePanelGroup != null) {
			this.attributePanelGroup.alpha = 0f;
			this.attributePanelGroup.blocksRaycasts = false;
			this.attributePanelGroup.interactable = false;
		}

		GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag("MainCamera");
		foreach (GameObject cam in cameraObjects) {
			GameObject minimap = GameObject.FindGameObjectWithTag("Minimap");
			if (minimap != null) {
				Material borderMaterial = Resources.Load<Material>("Border");
				if (borderMaterial != null) {
					if (cam.GetComponent<PostRenderer>() == null) {
						cam.AddComponent<PostRenderer>();
					}
					PostRenderer postRenderer = cam.GetComponent<PostRenderer>();
					if (postRenderer != null) {
						postRenderer.borderMaterial = borderMaterial;
						postRenderer.minimapCamera = minimap.GetComponent<Camera>();
						if (postRenderer.minimapCamera == null) {
							Debug.LogError("Unable to assign minimap camera to post renderer.");
						}
					}
				}

				if (cam.GetComponent<CameraPanning>() == null) {
					Debug.Log("Camera Panning is added to camera. Please check.");
					CameraPanning panning = cam.AddComponent<CameraPanning>();
					MinimapStuffs stuffs = minimap.GetComponent<MinimapStuffs>();
					if (stuffs != null) {
						stuffs.playerCameraPanning = panning;
					}
					panning.cameraPanning = true;
				}
				else {
					if (!cam.GetComponent<CameraPanning>().cameraPanning) {
						cam.GetComponent<CameraPanning>().cameraPanning = true;
					}
				}
			}
		}

		//Player unit
		GameObject playerObject = MonoBehaviour.Instantiate(this.spawnPrefab) as GameObject;
		playerObject.transform.position = this.transform.position;
		playerObject.transform.SetParent(this.parent.transform);

		//Player selection manager
		GameObject manager = MonoBehaviour.Instantiate(this.selectionManagerPrefab) as GameObject;
		SelectionManager selectionManager = manager.GetComponent<SelectionManager>();
		if (selectionManager != null) {
			selectionManager.allObjects.Add(playerObject);
			selectionManager.minimapCamera = this.minimapCamera;
			selectionManager.transform.SetParent(this.parent.transform);
		}

		//Player split manager
		manager = MonoBehaviour.Instantiate(this.splitManagerPrefab) as GameObject;
		SplitManager splitManager = manager.GetComponent<SplitManager>();
		if (splitManager != null) {
			splitManager.selectionManager = selectionManager;
			splitManager.transform.SetParent(this.parent.transform);
		}

		//Player merge manager
		manager = MonoBehaviour.Instantiate(this.mergeManagerPrefab) as GameObject;
		MergeManager mergeManager = manager.GetComponent<MergeManager>();
		if (mergeManager != null) {
			mergeManager.selectionManager = selectionManager;
			mergeManager.transform.SetParent(this.parent.transform);
		}

		//Set camera to view exactly towards game unit.
		Vector3 pos = playerObject.transform.position;
		pos.y = Camera.main.transform.position.y;
		Camera.main.transform.position = pos;

		int colorValue;
		switch (Spawner.colorCode) {
			default:
				colorValue = -1;
				break;
			case 0:
				colorValue = 0;
				break;
			case 1:
				colorValue = 1;
				break;
			case 2:
				colorValue = 2;
				break;
		}

		Spawner.colorCode++;
		if (Spawner.colorCode > 2) {
			Spawner.colorCode = 0;
		}
	}
}

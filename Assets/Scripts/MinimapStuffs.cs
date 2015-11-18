using UnityEngine;
using System.Collections;

public class MinimapStuffs : MonoBehaviour {
	public Camera playerCamera;
	public Camera minimapCamera;
	public Collider floorCollider;
	public CameraPanning playerCameraPanning;
	public Vector3 topLeftPosition, topRightPosition, bottomLeftPosition, bottomRightPosition;
	public Vector3 mousePosition;
	public Vector3 minimapMousePosition;
	public Vector3 newCameraPosition;

	public static MinimapStuffs Instance;

	public void Start() {
		if (this.playerCamera == null) {
			Debug.LogError("Unable to determine where the Player Camera component is at.");
		}

		if (this.minimapCamera == null) {
			Debug.LogError("Unable to determine where the Minimap Camera component is at.");
		}

		if (this.floorCollider == null) {
			GameObject floorObject = GameObject.FindGameObjectWithTag("FloorCollider");
			this.floorCollider = floorObject.GetComponent<Collider>();
			if (this.floorCollider == null) {
				Debug.LogError("Cannot set Quad floor collider to this variable. Please check.");
			}
		}

		if (MinimapStuffs.Instance == null) {
			MinimapStuffs.Instance = this;
		}

		//if (this.minimapCollider == null) {
		//	GameObject minimapObject = GameObject.FindGameObjectWithTag("MinimapCollider");
		//	this.minimapCollider = minimapObject.GetComponent<Collider>();
		//	if (this.minimapCollider == null) {
		//		Debug.LogError("Cannot set Minimap collider to this variable. Please check.");
		//	}
		//}
	}

	public void Update() {
		//Input.mousePosition is screen position.
		if (this.playerCameraPanning != null) {
			this.mousePosition = this.playerCamera.ScreenToViewportPoint(Input.mousePosition);
			if (this.minimapCamera.rect.Contains(this.mousePosition)) {
				if (Input.GetMouseButton(0)) {
					Ray worldRay = this.minimapCamera.ScreenPointToRay(Input.mousePosition);
					RaycastHit floorHit;
					if (this.floorCollider.Raycast(worldRay, out floorHit, 100f)) {
						this.newCameraPosition = floorHit.point;
						this.newCameraPosition.y = this.playerCamera.transform.position.y;
						this.playerCamera.gameObject.transform.position = this.newCameraPosition;
					}
				}
			}
		}


		Ray topLeftCorner = this.playerCamera.ScreenPointToRay(new Vector3(0f, 0f));
		Ray topRightCorner = this.playerCamera.ScreenPointToRay(new Vector3(Screen.width, 0f));
		Ray bottomLeftCorner = this.playerCamera.ScreenPointToRay(new Vector3(0, Screen.height));
		Ray bottomRightCorner = this.playerCamera.ScreenPointToRay(new Vector3(Screen.width, Screen.height));

		RaycastHit[] hits = new RaycastHit[4];
		if (this.floorCollider.Raycast(topLeftCorner, out hits[0], 40f)) {
			this.topLeftPosition = hits[0].point;
		}
		if (this.floorCollider.Raycast(topRightCorner, out hits[1], 40f)) {
			this.topRightPosition = hits[1].point;
		}
		if (this.floorCollider.Raycast(bottomLeftCorner, out hits[2], 40f)) {
			this.bottomLeftPosition = hits[2].point;
		}
		if (this.floorCollider.Raycast(bottomRightCorner, out hits[3], 40f)) {
			this.bottomRightPosition = hits[3].point;
		}

		this.topLeftPosition = this.minimapCamera.WorldToViewportPoint(this.topLeftPosition);
		this.topRightPosition = this.minimapCamera.WorldToViewportPoint(this.topRightPosition);
		this.bottomLeftPosition = this.minimapCamera.WorldToViewportPoint(this.bottomLeftPosition);
		this.bottomRightPosition = this.minimapCamera.WorldToViewportPoint(this.bottomRightPosition);

		this.topLeftPosition.z = -1f;
		this.topRightPosition.z = -1f;
		this.bottomLeftPosition.z = -1f;
		this.bottomRightPosition.z = -1f;


	}

	public void OnPostRender() {
		GL.PushMatrix();
		{
			GL.LoadOrtho();
			GL.Begin(GL.LINES);
			{
				GL.Color(Color.red);
				GL.Vertex(this.topLeftPosition);
				GL.Vertex(this.topRightPosition);
				GL.Vertex(this.topRightPosition);
				GL.Vertex(this.bottomRightPosition);
				GL.Vertex(this.bottomRightPosition);
				GL.Vertex(this.bottomLeftPosition);
				GL.Vertex(this.bottomLeftPosition);
				GL.Vertex(this.topLeftPosition);
			}
			GL.End();
		}
		GL.PopMatrix();
	}
}

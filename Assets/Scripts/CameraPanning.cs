using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraPanning : MonoBehaviour {
	[SerializeField]
	private bool panningEnableFlag;
	[SerializeField]
	private bool mouseInFocusFlag;
	[SerializeField]
	private bool cameraZoomingFlag;

	public readonly float aspectRatio = (float) Screen.currentResolution.width / (float) Screen.currentResolution.height;
	public int marginSize = 15;
	public bool useDebugSceneCameraBorder = true;
	public float cameraSpeed = 1f;
	public int zoomLevel = 3;

	//Some fancy way of using the C# setter/getter code design pattern. Note that the actual variables for storing data has FLAG suffixes.

	public bool cameraPanning {
		set {
			this.panningEnableFlag = value;
		}
		get {
			return this.panningEnableFlag;
		}
	}

	public bool cameraZooming {
		set {
			this.cameraZoomingFlag = value;
		}
		get {
			return this.cameraZoomingFlag;
		}
	}

	public bool mouseInFocus {
		set {
			this.mouseInFocusFlag = value;
		}
		get {
			return this.mouseInFocusFlag;
		}
	}

	public float aspectRatioMarginSize {
		get {
			return this.marginSize * this.aspectRatio;
		}
	}

	public void Start() {
		this.mouseInFocusFlag = true;
		this.cameraZoomingFlag = true;
	}

	public void OnApplicationFocus(bool focus) {
		this.mouseInFocus = focus;
		this.cameraPanning = focus;
		this.cameraZooming = focus;
	}

	public void Update() {
		CameraZoomMethod();
		CameraPanMethod();
	}

	public void CameraPanMethod() {
		if (this.panningEnableFlag && this.mouseInFocusFlag) {
			Vector2 mousePosition = Input.mousePosition;

#if UNITY_EDITOR
			//This takes into account the game screen resolution in the Unity Editor.
			//If we are not playing the game in the Unity Editor, the preprocessor would choose the actual monitor screen resolution instead.
			this.useDebugSceneCameraBorder = true;
			Vector2 screen = (this.useDebugSceneCameraBorder ? Handles.GetMainGameViewSize() : new Vector2(Screen.currentResolution.width, Screen.currentResolution.height));
#else
			Vector2 screen = new Vector3(Screen.width, Screen.height, 0f);
#endif


			if (mousePosition.x > 0 && mousePosition.x < this.aspectRatioMarginSize) {
				Vector3 camPos = this.transform.position;
				camPos.x -= cameraSpeed;
				this.transform.position = camPos;
			}
			if (mousePosition.y > 0 && mousePosition.y < this.aspectRatioMarginSize) {
				Vector3 camPos = this.transform.position;
				camPos.z -= cameraSpeed;
				this.transform.position = camPos;
			}
			if (mousePosition.x > (screen.x - this.aspectRatioMarginSize) && mousePosition.x <= screen.x) {
				Vector3 camPos = this.transform.position;
				camPos.x += cameraSpeed;
				this.transform.position = camPos;
			}
			if (mousePosition.y > (screen.y - this.aspectRatioMarginSize) && mousePosition.y <= screen.y) {
				Vector3 camPos = this.transform.position;
				camPos.z += cameraSpeed;
				this.transform.position = camPos;
			}
		}
	}

	public void SetCameraPosition(float xDiff, float yDiff, float zDiff) {
		Vector3 camPos = Camera.main.transform.position;
		if (!float.IsNaN(xDiff)) {
			camPos.x = xDiff;
		}
		if (!float.IsNaN(yDiff)) {
			camPos.y = yDiff;
		}
		if (!float.IsNaN(zDiff)) {
			camPos.z = zDiff;
		}
		Camera.main.transform.position = camPos;
	}

	public void CameraZoomMethod() {
		if (this.cameraZoomingFlag && this.mouseInFocusFlag) {
			float delta = Input.GetAxis("Mouse ScrollWheel");
			if (delta > 0f) {
				//Scroll up
				this.zoomLevel++;
				if (this.zoomLevel > 20) {
					this.zoomLevel = 20;
				}
				SetCameraPosition(float.NaN, (float) this.zoomLevel, float.NaN);
			}
			else if (delta < 0f) {
				//Scroll down
				this.zoomLevel--;
				if (this.zoomLevel < 3) {
					this.zoomLevel = 3;
				}
				SetCameraPosition(float.NaN, (float) this.zoomLevel, float.NaN);
			}
			this.cameraSpeed = 0.05f + (this.zoomLevel - 3) * 0.015f;
		}
	}
}

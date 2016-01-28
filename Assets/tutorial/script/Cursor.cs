using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;

namespace Tutorial {
	[System.Serializable]
	public struct CursorPanGroup {
		public Vector3 start;
		public Vector3 end;

		public CursorPanGroup(Vector3 s, Vector3 e) {
			this.start = s;
			this.end = e;
		}
	}

	public enum CursorButton {
		Nothing, Left_Click, Right_Click
	}

	public class Cursor : MonoBehaviour {
		public CanvasGroup cursorGroup;

		public RectTransform rectTransform;
		public Vector3 startingPosition;
		public Vector3 endingPosition;

		public bool isAppearing;
		public bool isPanning;
		public float panningElapsedTime;

		public bool isButtonPressed;
		public bool isButtonHeld;
		public float buttonPressedElapsedTime;
		public CursorButtonIcon icon;

		public void Start() {
			this.rectTransform = this.GetComponent<RectTransform>();
			if (this.rectTransform == null) {
				Debug.LogError("Cannot obtain cursor's RectTransform. Please check.");
			}
			this.cursorGroup = this.GetComponent<CanvasGroup>();
			if (this.cursorGroup == null) {
				Debug.LogError("Cannot obtain cursor's CanvasGroup. Please check.");
			}
			this.icon = this.GetComponentInChildren<CursorButtonIcon>();
			if (this.icon == null) {
				Debug.LogError("Cannot obtain cursor mouse button click icons. Please check.");
			}

			this.isAppearing = false;
			this.isPanning = false;
			this.cursorGroup.alpha = 0f;
			this.cursorGroup.interactable = false;
			this.cursorGroup.blocksRaycasts = false;
			this.panningElapsedTime = 0f;

			this.rectTransform.localPosition = Vector3.zero;
			this.endingPosition = this.startingPosition = Vector3.zero;
		}

		public void Update() {
			this.CursorDragAnimation();
		}

		public void Appear() {
			this.isAppearing = true;
		}

		public void Disappear() {
			this.isAppearing = false;
		}

		public void OnDrawGizmosSelected() {
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(this.startingPosition, this.endingPosition);
		}

		public void OnDrawGizmos() {
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(this.startingPosition, this.endingPosition);
		}


		/// <summary>
		/// True if the cursor is already panning, and I do not want to disturb the panning in progress.
		/// False if the cursor has finished panning, and there's nothing else to do.
		/// </summary>
		/// <param name="start">Vector3 position to indicate where the cursor begins the panning animation.</param>
		/// <param name="end">Vector3 position to indicate where the cursor ends in the panning animation.</param>
		/// <returns></returns>

		public bool PanCursor(Vector3 start, Vector3 end) {
			if (this.isPanning) {
				return false;
			}
			this.startingPosition = start;
			this.endingPosition = end;
			this.rectTransform.localPosition = start;
			this.panningElapsedTime = 0f;
			this.isAppearing = true;
			return true;
		}


		/// <summary>
		/// True if the cursor is already panning, and I do not want to disturb the panning in progress.
		/// False if the cursor has finished panning, and there's nothing else to do.
		/// </summary>
		/// <param name="start">Vector3 position to indicate where the cursor begins the panning animation.</param>
		/// <param name="end">Vector3 position to indicate where the cursor ends in the panning animation.</param>
		/// <returns></returns>
		public bool PanCursor(CursorPanGroup group, CursorButton button) {
			if (this.isPanning) {
				return false;
			}
			this.startingPosition = group.start;
			this.endingPosition = group.end;
			this.rectTransform.localPosition = group.start;
			this.panningElapsedTime = 0f;
			this.isAppearing = true;


			ObtainStartingPosition s = this.GetComponentInChildren<ObtainStartingPosition>();
			s.rectTransform.localPosition = group.start;
			ObtainEndingPosition e = this.GetComponentInChildren<ObtainEndingPosition>();
			e.rectTransform.localPosition = group.end;

			if (!button.Equals(CursorButton.Nothing)) {
				this.icon.SetButton(button);
				this.buttonPressedElapsedTime = 0f;
				this.isButtonPressed = true;
				this.isButtonHeld = false;
			}

			return true;
		}


		public bool PanCursorWithAction(CursorPanGroup group, CursorButton button, TutorialAIManager manager, float delayTime, string methodName) {
			if (this.isPanning) {
				return false;
			}

			this.startingPosition = group.start;
			this.endingPosition = group.end;
			this.rectTransform.localPosition = group.start;
			this.panningElapsedTime = 0f;
			this.isAppearing = true;


			ObtainStartingPosition s = this.GetComponentInChildren<ObtainStartingPosition>();
			s.rectTransform.localPosition = group.start;
			ObtainEndingPosition e = this.GetComponentInChildren<ObtainEndingPosition>();
			e.rectTransform.localPosition = group.end;

			if (!button.Equals(CursorButton.Nothing)) {
				this.icon.SetButton(button);
				this.buttonPressedElapsedTime = 0f;
				this.isButtonPressed = true;
			}

			manager.Invoke(methodName, delayTime);
			return true;
		}

		public bool PanCursorWithHeldAction(CursorPanGroup group, CursorButton button, TutorialAIManager manager, float delayTime, string methodName, bool heldFlag) {
			if (this.isPanning) {
				return false;
			}

			this.startingPosition = group.start;
			this.endingPosition = group.end;
			this.rectTransform.localPosition = group.start;
			this.panningElapsedTime = 0f;
			this.isAppearing = true;


			//NOTE(Thompson): I have no idea what the codes below are doing. CursorPanGroup should already have the coordinates set before this.
			ObtainStartingPosition s = this.GetComponentInChildren<ObtainStartingPosition>();
			s.rectTransform.localPosition = group.start;
			ObtainEndingPosition e = this.GetComponentInChildren<ObtainEndingPosition>();
			e.rectTransform.localPosition = group.end;

			if (!button.Equals(CursorButton.Nothing)) {
				this.icon.SetButton(button);
				this.buttonPressedElapsedTime = 0f;
				this.isButtonPressed = true;
				this.isButtonHeld = heldFlag;
			}

			manager.Invoke(methodName, delayTime);
			return true;
		}

		public void DragSelectionBox(Camera main, CursorPanGroup group, CursorButton button, TutorialAIManager manager, float delayTime, string methodName) {
			BoxSelector selector = main.GetComponent<BoxSelector>();
			if (selector == null) {
				Debug.LogError("Cannot find Box Selector component from camera, " + main.ToString() + ".");
				return;
			}

			if (button != CursorButton.Left_Click) {
				Debug.LogError("Selection box only works with left mouse button.");
				return;
			}

			selector.StartBoxSelection(group, 0.5f);
			this.icon.SetButton(button);
			this.buttonPressedElapsedTime = 0f;
			this.isButtonPressed = true;
			this.isButtonHeld = true;
			this.isAppearing = true;
			this.panningElapsedTime = 0f;
			this.startingPosition = group.start;
			this.endingPosition = group.end;
			this.rectTransform.position = group.start;

			manager.Invoke(methodName, delayTime);
			this.Invoke("HeldButtonRelease", delayTime);
		}


		//   -----------------------------------   PRIVATE METHODS   ---------------------------------------

		private void CursorClickAnimation() {
			if (this.isAppearing) {
				if (this.cursorGroup.alpha < 1f) {
					this.cursorGroup.alpha += Time.deltaTime;
				}
				else {
					this.rectTransform.position = Vector3.Lerp(this.startingPosition, this.endingPosition, this.panningElapsedTime);
					if (this.panningElapsedTime < 1f) {
						this.panningElapsedTime += Time.deltaTime / 2f;
					}
					else {
						CanvasGroup mouseButtonGroup = this.icon.GetComponent<CanvasGroup>();
						if (this.isButtonPressed) {
							mouseButtonGroup.alpha += Time.deltaTime * 2.5f;
							if (mouseButtonGroup.alpha >= 1f) {
								mouseButtonGroup.alpha = 1f;
								this.isButtonPressed = false;
							}
						}
						if (!this.isButtonPressed) {
							mouseButtonGroup.alpha -= Time.deltaTime * 2.5f;
							if (mouseButtonGroup.alpha <= 0) {
								mouseButtonGroup.alpha = 0;
								this.isAppearing = false;
							}
						}
					}
				}
			}
			if (!this.isAppearing) {
				if (this.cursorGroup.alpha > 0f) {
					this.cursorGroup.alpha -= Time.deltaTime;
				}
			}
		}

		private void CursorDragAnimation() {
			if (this.isAppearing) {
				CanvasGroup mouseButtonGroup = this.icon.GetComponent<CanvasGroup>();
				if (this.cursorGroup.alpha < 1f) {
					this.cursorGroup.alpha += Time.deltaTime;
					if (this.cursorGroup.alpha > 0.5f && mouseButtonGroup.alpha < 1f) {
						mouseButtonGroup.alpha += Time.deltaTime * 2.5f;
					}
				}
				else {
					this.rectTransform.position = Vector3.Lerp(this.startingPosition, this.endingPosition, this.panningElapsedTime);
					if (this.panningElapsedTime < 1f) {
						this.panningElapsedTime += Time.deltaTime / 2f;
					}
					else {
						if (this.isButtonPressed) {
							if (mouseButtonGroup.alpha >= 1f && !this.isButtonHeld) {
								mouseButtonGroup.alpha = 1f;
								this.isButtonPressed = false;
							}
						}
						if (!this.isButtonPressed) {
							mouseButtonGroup.alpha -= Time.deltaTime * 2.5f;
							if (mouseButtonGroup.alpha <= 0) {
								mouseButtonGroup.alpha = 0;
								this.isAppearing = false;
							}
						}
					}
				}
			}
			if (!this.isAppearing) {
				if (this.cursorGroup.alpha > 0f) {
					this.cursorGroup.alpha -= Time.deltaTime;
				}
			}
		}

		private void HeldButtonRelease() {
			if (this.isButtonHeld) {
				this.isButtonHeld = false;
			}
		}
	}
}

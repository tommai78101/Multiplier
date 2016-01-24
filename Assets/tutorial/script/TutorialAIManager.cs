using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;
using SinglePlayer;
using System;

namespace Tutorial {
	public enum Parts {
		Introduction, Camera_Controls, Unit_Controls, Splitting, Merging, Invalid, Error
	}

	//This code layout is the most readable layout that I can think of for others to understand and follow. Nothing wrong with redundancy.
	public class StringConstants {
		public static string Values(Parts parts, int section) {
			switch (parts) {
				case Parts.Introduction:
					switch (section) {
						case 0:
							return "Hello there. Welcome to the tutorial for Multiplier (working title). To begin, please press the \"Next Step\" button on the left. You may exit the tutorial at anytime by pressing the \"Return to Main Menu\" button on the left.";
						case 1:
							return "In this tutorial, you will learn how the game is to be played, as well as how this game can be used as a tool for you to play around with.";
						case 2:
							return "Let's begin, shall we?";
					}
					break;
				case Parts.Camera_Controls:
					switch (section) {
						case 0:
							return "The first lesson is Camera Controls. Here, we introduce to you the basics of moving around the camera.";
						case 1:
							return "To start, move your mouse to the edge of the game boundaries. Press \"Next Step\" to continue once you are done.";
						case 2:
							return "When you put your mouse at the edge of where you wished to go, the camera will move in that general direction. This is useful if you want to \"nudge\" the camera just a tiny bit to see clearly.";
						case 3:
							return "The other method of moving the camera is by using the minimap shown in the lower right screen. Click and drag in the minimap at the lower right of your screen to move the camera around.";
						case 4:
							return "This method of control is best suited for quickly panning the camera to where you want to see.";
						case 5:
							return "Now, let's move onwards to learn about Game Unit Controls.";
					}
					break;
				case Parts.Unit_Controls:
					switch (section) {
						case 0:
							return "Here, we introduce to you the game unit, Capsule. It is shaped like a capsule, and has a team color labeled at the top.";
						case 1:
							return "Capsules in the game can be interacted with the mouse and the keyboard. We'll start off with showing what the mouse can do.";
						case 2:
							return "The following animation shows what happens when the player controls the mouse, and left clicks on the Capsule. (Blue marker is left mouse button click.)";
						case 3:
							return "The Capsule will indicate that it has been selected. ";
						case 4:
							return "Once the Capsule is selected, you may press the S key on your keyboard to initiate the \"Split\" command. This is what happens.";
						case 5:
							return "When a Capsule is split, it retains all of the unit attributes the Capsule had before the split. The Capsule cannot fight and be controlled while splitting.";
                        case 6:
                            return "After the Capsule has split, you can box select Capsules by pressing and holding down the left mouse button";
					}
					break;
			}
			return "";
		}
	}

	public class TutorialAIManager : MonoBehaviour {
		public Parts currentTutorialStage;
		public Text dialogueText;
		public int stringLetterCounter;
		public string dialogue;
		public bool startTextRollingFlag;
		public float delay;
		[Range(0.01f, 2f)]
		public float delayInterval;
		public int dialogueSectionCounter;
		public Camera mainCamera;
		public Vector3 cameraOrigin;
		public MinimapStuffs minimap;
		public NewTutorialAIUnit tutorialUnit;
		public SplitMergeManager splitMergeManager;
		public Transform tutorialUnitParent;
		public GameObject cursorPrefab;
		public Cursor mainCursor;
		public Canvas mainCanvas;

		public List<CursorPanGroup> groupList = new List<CursorPanGroup>();
		public int groupListCounter = 0;

		public void Start() {
			this.delay = 0f;
			this.delayInterval = 0.01f;
			this.dialogueSectionCounter = 0;
			this.currentTutorialStage = Parts.Introduction;
			this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
			this.dialogueSectionCounter++;
			this.stringLetterCounter = 0;
			this.startTextRollingFlag = true;
			if (this.dialogueText != null) {
				this.dialogueText.text = "";
			}
			if (this.mainCamera == null) {
				Debug.LogError("Cannot obtain main camera. Please check.");
			}
			this.cameraOrigin = this.mainCamera.transform.position;

			CameraPanning panning = this.mainCamera.GetComponent<CameraPanning>();
			if (panning != null) {
				panning.enabled = false;
			}
			Camera minimapCamera = this.minimap.GetComponent<Camera>();
			if (minimapCamera != null) {
				minimapCamera.enabled = false;
			}

			//Cursor setup
			GameObject obj = MonoBehaviour.Instantiate(this.cursorPrefab) as GameObject;
			obj.transform.SetParent(this.mainCanvas.transform);
			this.mainCursor = obj.GetComponent<Cursor>();
			if (this.mainCursor == null) {
				Debug.LogError("Cursor isn't obtained. Please check.");
			}

			InitializeCursorPanGroups();
		}

		public void Update() {
			if (this.startTextRollingFlag) {
				if (this.delay < this.delayInterval) {
					this.delay += Time.deltaTime;
					return;
				}
				else {
					if (this.stringLetterCounter < this.dialogue.Length) {
						this.dialogueText.text = this.dialogueText.text.Insert(this.dialogueText.text.Length, this.dialogue[this.stringLetterCounter].ToString());
						this.stringLetterCounter++;
					}
					else {
						this.startTextRollingFlag = false;
					}
					this.delay = 0f;
				}
			}
		}

		//This code layout aims to have the highest readability level possible. Dialogues are broken into tutorial stages and sections. Magic numbers correspond to the total sections listed above
		//in the StringConstants class.
		public void OnClickAction() {
			switch (this.currentTutorialStage) {
				default:
				case Parts.Error:
					this.currentTutorialStage = Parts.Invalid;
					Debug.LogError("Current tutorial state has hit error. It will be reset to Invalid state.");
					break;
				case Parts.Invalid:
					Debug.LogWarning("Current tutorial state has hit an invalid state. Please check.");
					break;
				case Parts.Introduction:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= 2) {
						this.currentTutorialStage = Parts.Camera_Controls;
						this.dialogueSectionCounter = 0;
						break;
					}
					this.dialogueSectionCounter++;
					break;
				case Parts.Camera_Controls:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= 5) {
						this.currentTutorialStage = Parts.Unit_Controls;
						this.dialogueSectionCounter = 0;
						break;
					}
					if (this.dialogueSectionCounter == 1) {
						CameraPanning panning = this.mainCamera.GetComponent<CameraPanning>();
						if (panning != null) {
							panning.enabled = true;
						}
					}
					else if (this.dialogueSectionCounter == 3) {
						Camera minimapCamera = this.minimap.GetComponent<Camera>();
						if (minimapCamera != null) {
							minimapCamera.enabled = true;
						}
					}
					this.dialogueSectionCounter++;
					break;
				case Parts.Unit_Controls:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= 5) {
						this.currentTutorialStage = Parts.Unit_Controls;
						this.dialogueSectionCounter = 0;
						break;
					}
                    if (this.dialogueSectionCounter == 0) {
                        this.tutorialUnit.gameObject.SetActive(true);
                    }
                    else if (this.dialogueSectionCounter == 3) {
                        //TODO(Thompson): Make the camera panning automatic and have it pan smoothly.
                        this.mainCamera.transform.position = this.cameraOrigin;
						//Selecting with cursor
						//this.mainCursor.PanCursor(this.GetNextPanning(), CursorButton.Left_Click);

						//Show selection ring after action
						this.mainCursor.PanCursorWithAction(this.GetNextPanning(), CursorButton.Left_Click, this, 3f, "DelayShowSelectionRing");
                    }
                    else if (this.dialogueSectionCounter == 5) {
                        //Splitting
                        GameObject clone = MonoBehaviour.Instantiate(this.tutorialUnit.gameObject) as GameObject;
                        clone.SetActive(true);
                        clone.transform.SetParent(this.tutorialUnitParent);
                        this.splitMergeManager.splitGroupList.Add(new Group(this.tutorialUnit.gameObject, clone));
                    }
                    else if (this.dialogueSectionCounter == 6) {
                        //TODO(Thompson): Continue with selecting units and merging.
                        //TODO(Thompson): Disable camera panning during Unit Controls section.
                    }
					this.dialogueSectionCounter++;
					break;
			}
			this.dialogueText.text = "";
			this.stringLetterCounter = 0;
			this.startTextRollingFlag = true;
		}

		//It loops back to the beginning if there's no more Pan Group.
		public CursorPanGroup GetNextPanning() {
			if (this.groupListCounter + 1 < this.groupList.Count) {
				return this.groupList[this.groupListCounter++];
			}
			else {
				this.groupListCounter = 0;
				return this.groupList[this.groupListCounter];
			}
		}

		//------------------------------       PRIVATE METHODS      --------------------------

		private void InitializeCursorPanGroups() {
			//Initialize panning groups here. Crude way, not elegant in readability, but it gets the job done faster.
			Debug.Log("Adding new cursor pan group.");
			this.groupList.Add(new CursorPanGroup(new Vector3(130f, -150f), new Vector3(10f, -20f)));
			//this.groupList.Add(new CursorPanGroup());
		}

		private void DelayShowSelectionRing() {
			this.tutorialUnit.ToggleSelectionRing(true);
		}

		private void DelayShowAllSelectionRings() {
			foreach (Transform child in this.tutorialUnitParent.transform) {
				NewTutorialAIUnit unit = child.GetComponent<NewTutorialAIUnit>();
				if (unit != null) {
					unit.ToggleSelectionRing(true);
				}
			}
		}

		private void DelayHideSelectionRing() {
			this.tutorialUnit.ToggleSelectionRing(false);
		}

		private void DelayHideAllSelectionRings() {
			foreach (Transform child in this.tutorialUnitParent.transform) {
				NewTutorialAIUnit unit = child.GetComponent<NewTutorialAIUnit>();
				if (unit != null) {
					unit.ToggleSelectionRing(false);
				}
			}
		}
	}
}

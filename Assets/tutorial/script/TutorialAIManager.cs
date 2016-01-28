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
							return "To start, move your mouse to the edge of the game boundaries. Press \"Next Step\" to continue when you are done.";
						case 2:
							return "When you put your mouse at the edge of where you wished to go, the camera will move in that general direction. This is useful if you want to \"nudge\" the camera just a tiny bit to see clearly.";
						case 3:
							return "The other method of moving the camera is by using the minimap shown in the lower right screen. Click and drag in the minimap at the lower right of your screen to move the camera around. Press \"Next Step\" when you are done.";
						case 4:
							return "This method of control is best suited for quickly panning the camera to where you want to see.";
						case 5:
							return "Now, let's move onwards to learn about Game Unit Controls. We'll focus on the units and not on the camera.";
					}
					break;
				case Parts.Unit_Controls:
					switch (section) {
						case 0:
							return "Here, we introduce to you the game unit, Capsule. It is shaped like a capsule, and has a team color labeled at the top.";
						case 1:
							return "Capsules in the game can be interacted with the mouse and the keyboard. We'll start off with showing what the Capsules can do with the mouse.";
						case 2:
							return "The following animation shows what happens when the player controls the mouse, and drag a selection box on the Capsule. (Blue marker is left mouse button click.)";
						case 3:
							return "The Capsule will indicate that it has been selected. ";
						case 4:
							return "Once the Capsule is selected, you may press the \"S\" key on your keyboard to initiate the \"Split\" command. This is what happens.";
						case 5:
							return "When a Capsule is split, it retains all of the unit attributes the Capsule had before the split. The Capsule cannot fight and be controlled while splitting.";
						case 6:
							return "The next interaction the Capsule can do is merging together with other split Capsules. This can be done using the \"D\" key.";
						case 7:
							return "The following animation show that Capsules can merge to form a stronger unit by selecting and commanding the units with the \"D\" key. This is essentially \"upgrading\" the 2 Capsules to 1 higher-tiered Capsule.";
						case 8:
							return "Do note that merged Capsules can no longer be split again. This is the only game rule you must understand!";
						case 9:
							return "As you can see, the properties of a higher-tiered Capsule shows it is bigger than what it was before the merge.";
						case 10:
							return "However, be aware that unit attributes may not correlate with the tiers, since the game allows you to customize your unit attributes. Your customizations may not always give a net positive upgrade.";
						case 11:
							return "For attacking, Capsules attack automatically if a nearby enemy Capsule is close enough. There is no need for interactions to attack.";
						case 12:
							return "And that is it for the tutorial! Go and experiment with the game!";
					}
					break;
			}
			return "";
		}
	}

	public class TutorialAIManager : MonoBehaviour {
		public bool isInitialized;
		public bool isTutorialFinished;
		public Parts currentTutorialStage;
		public Text dialogueText;
		public int stringLetterCounter;
		public string dialogue;
		public bool startTextRollingFlag;
		public float delay;
		[Range(0.0001f, 1f)]
		public float delayInterval;
		public int dialogueSectionCounter;
		public Camera mainCamera;
		public Vector3 cameraOrigin;
		public MinimapStuffs minimap;
		public Camera minimapCamera;
		public CameraPanning mainCameraPanning;
		public NewTutorialAIUnit tutorialUnit;
		public SplitMergeManager splitMergeManager;
		public Transform tutorialUnitParent;
		public GameObject cursorPrefab;
		public Cursor mainCursor;
		public Canvas mainCanvas;

		public Button nextStepButton;

		public List<CursorPanGroup> groupList = new List<CursorPanGroup>();
		public int groupListCounter = 0;

		public void Start() {
			this.isInitialized = false;
			this.isTutorialFinished = false;
			this.delay = 0f;
			this.delayInterval = 0.0001f;
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

			this.mainCameraPanning = this.mainCamera.GetComponent<CameraPanning>();
			if (this.mainCameraPanning != null) {
				this.mainCameraPanning.enabled = false;
			}
			this.minimapCamera = this.minimap.GetComponent<Camera>();
			if (this.minimapCamera != null) {
				this.minimapCamera.enabled = false;
			}
			if (this.minimap == null) {
				Debug.LogError("Couldn't obtain minimap stuffs. Please check.");
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
						//#DEBUG
						//this.nextStepButton.interactable = true;

						this.nextStepButton.interactable = false;
						this.dialogueText.text = this.dialogueText.text.Insert(this.dialogueText.text.Length, this.dialogue[this.stringLetterCounter].ToString());
						this.stringLetterCounter++;
					}
					else {
						this.startTextRollingFlag = false;
						if (!this.isTutorialFinished) {
							this.nextStepButton.interactable = true;
						}
					}
					this.delay = 0f;
				}
			}

			if (!this.isInitialized) {
				this.minimap.enabled = false;
				this.isInitialized = true;
			}
		}

		//This code layout aims to have the highest readability level possible. Dialogues are broken into tutorial stages and sections. Magic numbers correspond to the total sections listed above
		//in the StringConstants class.

		//Also, I would like to point out that I did not use switch..case within switch..case, because there may be moments where the action needs to span more than a few sections without
		//being re-triggered again.
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
						this.mainCameraPanning.enabled = true;
					}
					else if (this.dialogueSectionCounter == 3) {
						this.minimapCamera.enabled = true;
						this.minimap.enabled = true;
					}
					this.dialogueSectionCounter++;
					break;
				case Parts.Unit_Controls:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= 12) {
						this.currentTutorialStage = Parts.Unit_Controls;
						this.dialogueSectionCounter = 12;
						this.isTutorialFinished = true;
						this.nextStepButton.interactable = false;
						break;
					}
					if (this.dialogueSectionCounter == 0) {
						this.tutorialUnit.gameObject.SetActive(true);
						this.minimap.enabled = false;
						this.minimapCamera.enabled = false;
						this.mainCamera.transform.position = this.cameraOrigin;
						this.Invoke("DelayTurnOffCameraPanning", 0.1f);
					}
					else if (this.dialogueSectionCounter == 3) {
						//Dragging selection box.
						this.mainCursor.DragSelectionBox(this.mainCamera, this.GetNextPanning(), CursorButton.Left_Click, this, 3f, "DelayShowSelectionRing");
					}
					else if (this.dialogueSectionCounter == 5) {
						//Splitting
						GameObject clone = MonoBehaviour.Instantiate(this.tutorialUnit.gameObject) as GameObject;
						clone.SetActive(true);
						clone.transform.SetParent(this.tutorialUnitParent);
						this.splitMergeManager.splitGroupList.Add(new Group(this.tutorialUnit.gameObject, clone));

						//Stop selecting the unit.
						this.Invoke("DelayHideSelectionRing", 0.1f);
					}
					else if (this.dialogueSectionCounter == 8) {
						//Dragging selection box.
						this.mainCursor.DragSelectionBox(this.mainCamera, this.GetNextPanning(), CursorButton.Left_Click, this, 3f, "DelayShowAllSelectionRings");
						this.Invoke("DelayMergeUnits", 4f);
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
			if (this.groupListCounter < this.groupList.Count) {
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
			this.groupList.Add(new CursorPanGroup(new Vector3(642.5f, 145f), new Vector3(508.5f, 329f)));
			this.groupList.Add(new CursorPanGroup(new Vector3(828.5f, 102f), new Vector3(141.5f, 446f)));
		}

		private void DelayTurnOffCameraPanning() {
			this.mainCameraPanning.enabled = false;
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

		private void DelayMergeUnits() {
			//Merging
			GameObject child_1 = this.tutorialUnitParent.GetChild(0).gameObject;
			GameObject child_2 = this.tutorialUnitParent.GetChild(1).gameObject;
			this.splitMergeManager.mergeGroupList.Add(new Group(child_1, child_2));

			this.Invoke("DelayHideAllSelectionRings", 0.1f);
		}
	}
}

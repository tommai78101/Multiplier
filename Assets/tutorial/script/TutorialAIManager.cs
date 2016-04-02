using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;
using SinglePlayer;
using System;


namespace Tutorial {
	public enum Parts {
		Introduction, Attributes_Editor, Camera_Controls, Unit_Controls, Splitting, Merging, Invalid, Error
	}

	//This code layout is the most readable layout that I can think of for others to understand and follow. Nothing wrong with redundancy.
	public class StringConstants {
		public const int INTRODUCTION_SIZE = 8;
		public const int CAMERA_CONTROLS_SIZE = 5;
		public const int UNIT_CONTROLS_SIZE = 20;
		public const int ATTRIBUTES_EDITOR_SIZE = 20;

		public static string Values(Parts parts, int section) {
			switch (parts) {
				case Parts.Introduction:
					switch (section) {
						case 0:
							return "Hello there. Welcome to the tutorial for Multiplier, a real-time strategy game and simulation tool. To return to the Main Menu in other game modes, press ESCAPE key to open up the menu.";
						case 1:
							return "To begin, premise of the game is about managing your units by splitting and merging units to create more powerful units, and destroy your opponent's game units!";
						case 2:
							return "4 simple concepts: Your game units are expendable resources. Splitting is to build up your resources for merging. Merging is to upgrade your units to higher levels. When you have no more units, you lose the game!";
						case 3:
							return "Each concept is equally important, not only as part of the game. To see them in action, you can check out the simulation tool provided, and tweak variables using the Attributes Editor.";
						case 4:
							return "With the editor, your units and attributes are now based upon the leveling rates determined by math equations as results. Of which, they are usually done in Excel spreadsheets, without being able to see immediate results.";
						case 5:
							return "You can customize your unit's leveling rates by either selecting the preset default difficulty settings, or choosing the Custom preset.";
						case 6:
							return "If you choose the Custom preset, you can then enter a math equation in the input field shown, and the resulting answers will update the leveling rates.";
						case 7:
							return "You can decide how weak or powerful your units will be through the use of the Attributes Editor. Creating your balanced units, or really broken units. The sky's the limit!";
						case 8:
							return "Now we can jump in to the actual gameplay controls.";
					}
					break;
				case Parts.Attributes_Editor:
					switch (section) {
						case 0:
							return "This lesson is about the Attributes Editor. Here's an overview of what the Attributes Editor looks like, shown below. We'll do a quick rundown of how to control this beast.";
						case 1:
							return "When in any game modes, to return to the Main Menu, press ESCAPE key to open up the menu.";
						case 2:
							return "There are two areas in the Attributes Editor, the Player Configuration Area, and the A.I. Configuration Area.";
						case 3:
							return "We'll start with the Player Configuration Area, which is the top part of the Attributes Editor, shown below.";
                        case 4:
							return "Located at the top is the Presets, or pre-configured defaults the editor has provided you with.";
						case 5:
							return "The presets are labeled as simple math equations. They determine the leveling rates of all unit attributes of a game unit. The 'Health Heavy' preset means the game units will have more health, but less overall stats, which is useful for testing realistic situations.";
						case 6:
							return "Speaking of the 'Custom' option in the Presets downdown, this option enables more functionalities of the editor, allowing you to fully customize your game unit attributes.";
						case 7:
							return "When the preset is set to 'Custom', you will see the Equation Input Field, located at the bottom, is now active. You type the equations in there.";
						case 8:
							return "In the equation field, it is okay to type numbers or math equations starting with 'y='. If you input something wrong, it will report back '[Invalid Token]'.";
						case 9:
							return "When a correct math equation is entered, the Leveling Rates, above the Equation Input Field, will update all the leveling rates based on your equations. For example, y is equal to 42. Therefore, for each level, the Leveling Rate is 42.";
						case 10:
							return "The table below shows what symbols can be typed, which is used in the Equation Input Field. The editor will then read the equation and display the results to each Leveling Rates.";
                        case 11:
                            return "Going back up, the Category toggles allow you to specify which unit attribute you want to modify. As an example shown below, Health is selected, meaning the equation will only modify all Health points leveling rates.";
						case 12:
							return "The Leveling Rates shows what unit attributes per level will be applied to your game unit. For example, a Level 3 unit will have the Level 3 leveling rates for each attributes, a Level 6 unit will have Level 6 leveling rates, and so on.";
						case 13:
							return "Now, this next area, shown below, is the A.I. Configuration Area, where you can customize the A.I. player's game units. Because this area is similar to the Player Configuration Area, we'll only introduce the Mode Selection.";
						case 14:
                            return "This is the Mode Selection. You can choose to leave the A.I. as Default pre-configurations, or use Custom configurations, which will enable the rest of the editor. Difficulty only affects the game.";
						case 15:
							return "We now introduce to you the \"Simulation Editor\". Purpose is to run simulations to find if your math equations are balanced for use, especially since the high-level algorithms to determine unit properties can be applied elsewhere outside of this tool.";
						case 16:
							return "As shown, you have the Yellow Team and the Blue Team, each with their own Leaderboards. You also have the editor at the bottom, and the session counter in between the Leaderboards. The numbers on the Leaderboards are updated in real-time as the simulations are running.";
						case 17:
							return "The bottom editor is very much the same in other game modes, with the only difference is the Categories are placed to the right of the Equation Input Field, and are lined up in the same order to save space.";
						case 18:
							return "The final part of this tutorial section is the Main Control Panel. You edit the unit attributes of either the Yellow Team or the Blue Team here. \"Generate Report\" is used for logging the statistics of your simulations, as well as your testing sessions. Copy and paste the report somewhere safe!";
						case 19:
							return "That is it for this lesson.";
						case 20:
							return "EMPTY STRING. PLEASE IGNORE!";
					}
					break;
				case Parts.Camera_Controls:
					switch (section) {
						case 0:
							return "This lesson is the Camera Controls, teaching you the basics of moving around the camera.";
						case 1:
							return "Move your mouse to the edge of the game boundaries (near the flashing red borders).";
						case 2:
							return "When you put your mouse at the edge of where you wished to go, the camera will move in that general direction. Useful for panning the camera around.";
						case 3:
							return "The other method of moving the camera is by using the minimap shown in the lower right screen. Click and drag in the minimap at the lower right of your screen to move the camera around.";
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
							return "The Capsule will indicate that it has been selected. You can tell by seeing a green selection ring surrounding your selected unit.";
						case 4:
							return "Once the Capsule is selected, you may press the \"S\" key on your keyboard to initiate the \"Split\" command. This is what happens.";
						case 5:
							return "When a Capsule is split, each new capsule retains all of the unit attributes of the original capsule. The Capsule cannot fight and be controlled while splitting.";
						case 6:
							return "The Capsule can also merge with other split Capsules. This can be done using the \"D\" key.";
						case 7:
							return "The following animation shows that Capsules can merge to form a stronger unit by selecting and commanding the units with the \"D\" key. This is essentially \"upgrading\" the 2 Capsules to 1 higher-tiered Capsule.";
						case 8:
							return "Note that merged Capsules can no longer be split.";
						case 9:
							return "As you can see, a higher-tiered Capsule is bigger than it was before the merge.";
						case 10:
							return "However, be aware that unit attributes may not correlate with the tiers, since the game allows you to customize your unit attributes. Your customizations may not always give a net positive upgrade.";
						case 11:
							return "The following animation sequence shows what a non-positive upgrade will look like for the unit attribute, \"Speed\", according to the given mathematical equation. You will normally see this when customizing your unit attributes.";
						case 12:
							return "The higher the tier level of a Capsule, the less speed it moves from one side to the other.";
						case 13:
							return "The speeds of the Capsules are inversely proportional to their tier levels. This is called a non-positive upgrade, in which your unit attributes are nerfed the higher up the tier levels.";
						case 14:
							return "Capsules attack automatically if a nearby enemy Capsule is close enough. There is no need for interactions to attack.";
						case 15:
							return "How close enough is \"close enough\"? Imagine the diameter of your selected Capsule is 1 unit. You will need 3 units to detect an enemy nearby, and 1 unit to attack the enemy. The following animation will explain this more clearly.";
						case 16:
							return "The red colored Capsules are your units. The gray colored Capsules are used to measure the distance. By lining up the gray colored Capsules, you can see how long \"close enough\" really is. Also, the higher the tier level, the longer the distance will be.";
						case 17:
							return "When a unit is taking damage from an enemy attack, it will flare up as red, as shown below.";
						case 18:
							return "Once the unit takes damage, you can see its health points decreasing. The left number represents the current health points, and the right number represents full health points.";
						case 19:
							return "That is it for Unit Controls. Go, have fun, and experiment!";
						case 20:
							return "Thank you for watching! Go and experiment!";
					}
					break;
			}
			return "";
		}
	}

	public class TutorialAIManager : MonoBehaviour {
		public static TutorialAIManager Instance;

		public bool debugFlag;

		public Camera mainCamera;
		public Camera minimapCamera;
		public CameraPanning mainCameraPanning;
		public MinimapStuffs minimap;

		public Transform tutorialUnitParent;
		public Transform nonPositiveUnitParent;
		public Transform distanceUnitParent;
		public NewTutorialAIUnit tutorialUnit;

		public SplitMergeManager splitMergeManager;
		public ImageManager imageManager;

		public GameObject cursorPrefab;
		public Cursor mainCursor;

		public Canvas mainCanvas;
		public Button nextStepButton;
		public Text dialogueText;
		public GameObject uiBorderPanel;
		public GameObject tutorialSections;
		public GameObject dialogueBox;

		[SerializeField]
		private bool isInitialized;
		[SerializeField]
		private bool isTutorialFinished;
		[SerializeField]
		private bool startTextRollingFlag;
		[SerializeField]
		private int stringLetterCounter;
		[SerializeField]
		private int groupListCounter = 0;
		[Range(0f, 1f)]
		private float delay;
		[SerializeField]
		private float delayInterval;
		[SerializeField]
		private string dialogue;
		[SerializeField]
		private List<CursorPanGroup> groupList = new List<CursorPanGroup>();
		[SerializeField]
		private Vector3 cameraOrigin;
		[SerializeField]
		private Parts currentTutorialStage;
		[SerializeField]
		private int dialogueSectionCounter;

		public void Start() {
			TutorialAIManager.Instance = this;

			this.isInitialized = false;

			this.isTutorialFinished = false;
			this.delay = 0f;
			this.delayInterval = 0f;
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
			if (this.nextStepButton == null) {
				Debug.LogError("The next button isn't set.");
			}
			if (this.dialogueText == null) {
				Debug.LogError("Dialogue text field is not bind to this variable.");
			}
			if (this.tutorialSections == null) {
				Debug.LogError("Tutorial sections are not set.");
			}
			if (this.dialogueBox == null) {
				Debug.LogError("Dialogue box not set.");
			}

			//Cursor setup
			GameObject obj = MonoBehaviour.Instantiate(this.cursorPrefab) as GameObject;
			obj.transform.SetParent(this.mainCanvas.transform);
			this.mainCursor = obj.GetComponent<Cursor>();
			if (this.mainCursor == null) {
				Debug.LogError("Cursor isn't obtained. Please check.");
			}

			InitializeCursorPanGroups();
			InitializeTutorial();
		}

		public void InitializeTutorial() {
			this.dialogueBox.gameObject.SetActive(false);
			this.tutorialSections.gameObject.SetActive(true);
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
						if (this.debugFlag) {
							this.nextStepButton.interactable = true;
							this.dialogueText.text = this.dialogue;
							this.stringLetterCounter = this.dialogue.Length;
						}
						else {
							this.nextStepButton.interactable = false;
							this.dialogueText.text = this.dialogueText.text.Insert(this.dialogueText.text.Length, this.dialogue[this.stringLetterCounter].ToString());
							this.stringLetterCounter++;
						}
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
					if (this.dialogueSectionCounter >= StringConstants.INTRODUCTION_SIZE) {
						this.ReturnToSections();
						break;
					}
					GameObject img = null;
					//Image index 0 ~ 5: General Section
					//ImageManager.Obtain() takes in image index as parameter, to display which specific image to show.
					//Image index is determined at compile time, while placing images in the Unity editor.
					switch (this.dialogueSectionCounter) {
						case 1:
							img = this.imageManager.Obtain(0);
							img.SetActive(true);
							break;
						case 2:
							img = this.imageManager.Obtain(0);
							img.SetActive(false);
							img = this.imageManager.Obtain(1);
							img.SetActive(true);
							break;
						case 3:
							img = this.imageManager.Obtain(1);
							img.SetActive(false);
							img = this.imageManager.Obtain(2);
							img.SetActive(true);
							break;
						case 4:
							img = this.imageManager.Obtain(2);
							img.SetActive(false);
							img = this.imageManager.Obtain(5);
							img.SetActive(true);
							break;
						case 5:
							img = this.imageManager.Obtain(5);
							img.SetActive(false);
							img = this.imageManager.Obtain(3);
							img.SetActive(true);
							break;
						case 6:
							img = this.imageManager.Obtain(3);
							img.SetActive(false);
							img = this.imageManager.Obtain(4);
							img.SetActive(true);
							break;
						case 7:
							img = this.imageManager.Obtain(4);
							img.SetActive(false);
							break;
						default:
							for (int i = 0; i < this.imageManager.images.Count; i++) {
								img = this.imageManager.Obtain(i);
								img.SetActive(false);
							}
							break;
					}
					this.dialogueSectionCounter++;
					break;
				//Image index 6 ~ 12: Attributes Editor
				case Parts.Attributes_Editor:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= StringConstants.ATTRIBUTES_EDITOR_SIZE) {
						this.ReturnToSections();
						break;
					}
					switch (this.dialogueSectionCounter) {
						case 1:
							img = this.imageManager.Obtain(6);
							img.SetActive(true);
							break;
						case 2:
							img = this.imageManager.Obtain(6);
							img.SetActive(false);
							img = this.imageManager.Obtain(7);
							img.SetActive(true);
							break;
						case 3:
							img = this.imageManager.Obtain(8);
							img.SetActive(true);
							break;
						case 4:
							img = this.imageManager.Obtain(7);
							img.SetActive(false);
							img = this.imageManager.Obtain(8);
							img.SetActive(false);
							img = this.imageManager.Obtain(9);
							img.SetActive(true);
							break;
						case 5:
							img = this.imageManager.Obtain(9);
							img.SetActive(false);
							img = this.imageManager.Obtain(10);
							img.SetActive(true);
							break;
						case 6:
							img = this.imageManager.Obtain(10);
							img.SetActive(false);
							img = this.imageManager.Obtain(11);
							img.SetActive(true);
							break;
						case 7:
							img = this.imageManager.Obtain(11);
							img.SetActive(false);
							img = this.imageManager.Obtain(12);
							img.SetActive(true);
							break;
						case 10:
							img = this.imageManager.Obtain(12);
							img.SetActive(false);
							img = this.imageManager.Obtain(13);
							img.SetActive(true);
							break;
						case 11:
							img = this.imageManager.Obtain(13);
							img.SetActive(false);
							img = this.imageManager.Obtain(14);
							img.SetActive(true);
							break;
						case 12:
							img = this.imageManager.Obtain(14);
							img.SetActive(false);
							img = this.imageManager.Obtain(15);
							img.SetActive(true);
							break;
						case 13:
							img = this.imageManager.Obtain(15);
							img.SetActive(false);
							img = this.imageManager.Obtain(16);
							img.SetActive(true);
							break;
						case 14:
							img = this.imageManager.Obtain(16);
							img.SetActive(false);
							img = this.imageManager.Obtain(17);
							img.SetActive(true);
							break;
						case 15:
							img = this.imageManager.Obtain(17);
							img.SetActive(false);
							img = this.imageManager.Obtain(18);
							img.SetActive(true);
							break;
						case 18:
							img = this.imageManager.Obtain(18);
							img.SetActive(false);
							img = this.imageManager.Obtain(19);
							img.SetActive(true);
							break;
						case 19:
							img = this.imageManager.Obtain(19);
							img.SetActive(false);
							break;
						default:
							//for (int i = 0; i < this.imageManager.images.Count; i++) {
							//	img = this.imageManager.Obtain(i);
							//	img.SetActive(false);
							//}
							break;
					}
					this.dialogueSectionCounter++;
					break;
				case Parts.Camera_Controls:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= StringConstants.CAMERA_CONTROLS_SIZE) {
						this.ReturnToSections();
						break;
					}
					switch (this.dialogueSectionCounter) {
						case 1:
							this.mainCameraPanning.enabled = true;
							this.uiBorderPanel.gameObject.SetActive(true);
							break;
						case 2:
							this.uiBorderPanel.gameObject.SetActive(false);
							break;
						case 3:
							this.minimapCamera.enabled = true;
							this.minimap.enabled = true;
							break;
						default:
							for (int i = 0; i < this.imageManager.images.Count; i++) {
								img = this.imageManager.Obtain(i);
								img.SetActive(false);
							}
							break;
					}
					this.dialogueSectionCounter++;
					break;
				case Parts.Unit_Controls:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					if (this.dialogueSectionCounter >= StringConstants.UNIT_CONTROLS_SIZE) {
						//this.currentTutorialStage = Parts.Unit_Controls;
						//this.dialogueSectionCounter = StringConstants.UNIT_CONTROLS_SIZE;
						//this.isTutorialFinished = true;
						//this.nextStepButton.interactable = false;
						this.distanceUnitParent.gameObject.SetActive(false);
						this.ReturnToSections();
						break;
					}
					switch (this.dialogueSectionCounter) {
						case 0:
							this.tutorialUnit.gameObject.SetActive(true);
							this.minimap.enabled = false;
							this.minimapCamera.enabled = false;
							this.mainCamera.transform.position = this.cameraOrigin;
							this.Invoke("DelayTurnOffCameraPanning", 0.1f);
							break;
						case 3:
							//Dragging selection box.
							this.mainCursor.DragSelectionBox(this.mainCamera, this.GetNextPanning(), CursorButton.Left_Click, this, 3f, "DelayShowSelectionRing");
							break;
						case 5:
							//Splitting
							GameObject clone = MonoBehaviour.Instantiate(this.tutorialUnit.gameObject) as GameObject;
							clone.SetActive(true);
							clone.transform.SetParent(this.tutorialUnitParent);
							this.splitMergeManager.splitGroupList.Add(new Group(this.tutorialUnit.gameObject, clone));

							//Stop selecting the unit.
							this.Invoke("DelayHideSelectionRing", 0.1f);
							break;
						case 8:
							//Dragging selection box.
							this.mainCursor.DragSelectionBox(this.mainCamera, this.GetNextPanning(), CursorButton.Left_Click, this, 3f, "DelayShowAllSelectionRings");
							this.Invoke("DelayMergeUnits", 4f);
							break;
						case 12:
							//TODO(Thompson): Show what a non-positive upgrade is for unit customization, via animation.
							if (this.tutorialUnitParent != null) {
								this.tutorialUnitParent.gameObject.SetActive(false);
							}
							if (this.nonPositiveUnitParent != null) {
								this.nonPositiveUnitParent.gameObject.SetActive(true);
							}
							break;
						case 14:
							if (this.nonPositiveUnitParent != null) {
								this.nonPositiveUnitParent.gameObject.SetActive(false);
							}
							break;
						case 16:
							//TODO(Thompson): Show how long the line of sight distance will be for LV1 unit and LV2 unit. Compare them via animation.
							if (this.distanceUnitParent != null) {
								this.distanceUnitParent.gameObject.SetActive(true);
							}
							break;
						case 17:
							if (this.distanceUnitParent != null) {
								this.distanceUnitParent.gameObject.SetActive(false);
							}
							img = this.imageManager.Obtain(20);
							img.SetActive(true);
							break;
						case 18:
							img = this.imageManager.Obtain(20);
							img.SetActive(false);
							img = this.imageManager.Obtain(21);
							img.SetActive(true);
							break;
						case 19:
							img = this.imageManager.Obtain(21);
							img.SetActive(false);
							break;
						default:
							break;
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

		public void PrepareGeneralSection() {
			this.currentTutorialStage = Parts.Introduction;
			this.dialogueSectionCounter = 0;
			this.debugFlag = true;
			this.OnClickAction();
		}

		public void PrepareEditorSection() {
			this.currentTutorialStage = Parts.Attributes_Editor;
			this.dialogueSectionCounter = 0;
			this.debugFlag = true;
			this.OnClickAction();
		}

		public void PrepareCameraSection() {
			this.currentTutorialStage = Parts.Camera_Controls;
			this.dialogueSectionCounter = 0;
			this.debugFlag = true;
			this.OnClickAction();
		}

		public void PrepareGameplaySection() {
			this.currentTutorialStage = Parts.Unit_Controls;
			this.dialogueSectionCounter = 0;
			this.debugFlag = false;
			this.OnClickAction();
		}

		public void ReturnToSections() {
			this.dialogueBox.gameObject.SetActive(false);
			this.tutorialSections.gameObject.SetActive(true);
			for (int i = 0; i < this.imageManager.images.Count; i++) {
				GameObject img = this.imageManager.Obtain(i);
				if (img != null) {
					img.SetActive(false);
				}
			}
			if (this.mainCameraPanning != null) {
				this.mainCameraPanning.enabled = false;
			}
			if (this.minimapCamera != null) {
				this.minimapCamera.enabled = false;
			}
		}
	}
}

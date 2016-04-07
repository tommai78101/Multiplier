using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class BaseTooltip : MonoBehaviour {
	public int paddingSize;
	public float screenPlaneDistanceFromCamera;
	public bool followCursorFlag;
	public CanvasGroup canvasGroup;
	public Image panel;
	public KeyCode toggleKey;
	public KeyCode alternateToggleKey;
	public Text tooltipTextContent;
	public RectTransform rectTransform;
	public RectTransform target;

	[Range(-400f, 400f)]
	public float xOffset;
	[Range(-400f, 400f)]
	public float yOffset;

	private String textMemory;
	private bool enableTooltipFlag;

	//Initialization stuffs.
	public void Start() {
		this.panel = this.GetComponent<Image>();  //Part of the default Panel UI component.
		this.canvasGroup = this.GetComponent<CanvasGroup>(); //Used to switch Tooltip to be visible/invisible.
		this.rectTransform = this.GetComponent<RectTransform>();  //Gets the Panel UI's RectTransform.
		this.tooltipTextContent = this.GetComponentInChildren<Text>(); //Gets the Text UI component that's the child of this Panel UI.
		if (this.paddingSize == 0) {
			this.paddingSize = 10; //Sets the text margins on all 4 sides (top, left, bottom, right).
		}
		this.transform.position = Vector3.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
		this.tooltipTextContent.text = "";  //Initialize the actual Tooltip text content to an empty string.
		this.enableTooltipFlag = true;  //True, if I want to enable the Tooltip. False, otherwise.
		this.SetToolTipHidden(true); //Toggles visibility using the CanvasGroup.
	}

	public void Update() {
		if (Input.GetKeyUp(this.toggleKey) || Input.GetKeyUp(this.alternateToggleKey)) {  //The toggle keys are LEFT and RIGHT SHIFT.
			this.enableTooltipFlag = !this.enableTooltipFlag;
			this.SetToolTipHidden(this.enableTooltipFlag);
		}

		if (this.panel != null && this.tooltipTextContent != null) {
			this.tooltipTextContent.text = this.textMemory;  //Sets the tooltip text content to the child Text UI component.

			//The following code adjusts the tooltip Panel UI's width and height. There's no other way I know of that auto-adjusts the child Text UI component's text content to fit nicely.
			if (this.tooltipTextContent.preferredWidth > Screen.width) {
				this.panel.rectTransform.sizeDelta = new Vector2(this.tooltipTextContent.preferredWidth * 0.5f, this.tooltipTextContent.preferredHeight + this.paddingSize * 2f);
			}
			else {
				this.panel.rectTransform.sizeDelta = new Vector2(this.tooltipTextContent.preferredWidth, this.tooltipTextContent.preferredHeight + this.paddingSize * 2f);
			}
		}

		//If followCursorFlag is TRUE, the tooltip will follow the cursor at a specific offset from the mouse cursor's position.
		if (this.target != null && this.followCursorFlag) {
			Vector2 pos = Input.mousePosition;
			pos.x -= this.xOffset;
			pos.y -= this.yOffset;
			this.rectTransform.localPosition = pos;
		}
	}

	//Just setting text to the child Text UI component.
	public void SetText(string hint) {
		this.textMemory = hint;
	}

	//Toggles tooltip visibility/
	public void SetToolTipHidden(bool flag) {
		if (this.enableTooltipFlag) {
			if (flag) {
				this.canvasGroup.alpha = 0f;
				this.canvasGroup.interactable = false;
				this.canvasGroup.blocksRaycasts = false;
			}
			else {
				this.canvasGroup.alpha = 1f;
				this.canvasGroup.interactable = true;
				this.canvasGroup.blocksRaycasts = false;
			}
		}
		else {
			this.canvasGroup.alpha = 0f;
			this.canvasGroup.interactable = false;
			this.canvasGroup.blocksRaycasts = false;
		}
	}

	//This sets the target. Target is a UI component that contains tooltip hint text contents. 
	//When a mouse cursor enters/exits the target's RectTransform boundaries, this function 
	//will be called. Its intended purpose is merely just to point out the last touched UI 
	//component the player has the mouse hovering over.
	public void SetTarget(RectTransform obj) {
		this.target = obj;
	}
}

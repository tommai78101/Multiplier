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

	public void Start() {
		this.panel = this.GetComponent<Image>();
		this.canvasGroup = this.GetComponent<CanvasGroup>();
		this.rectTransform = this.GetComponent<RectTransform>();
		this.tooltipTextContent = this.GetComponentInChildren<Text>();
		if (this.paddingSize == 0) {
			this.paddingSize = 10;
		}
		this.transform.position = Vector3.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
		this.enableTooltipFlag = true;
		this.SetToolTipHidden(true);
	}

	public void Update() {
		if (Input.GetKeyUp(this.toggleKey) || Input.GetKeyUp(this.alternateToggleKey)) {
			this.enableTooltipFlag = !this.enableTooltipFlag;
			this.SetToolTipHidden(this.enableTooltipFlag);
		}

		if (this.panel != null && this.tooltipTextContent != null) {
			this.tooltipTextContent.text = this.textMemory;
			if (this.tooltipTextContent.preferredWidth > Screen.width) {
				this.panel.rectTransform.sizeDelta = new Vector2(this.tooltipTextContent.preferredWidth * 0.5f, this.tooltipTextContent.preferredHeight + this.paddingSize * 2f);
			}
			else {
				this.panel.rectTransform.sizeDelta = new Vector2(this.tooltipTextContent.preferredWidth, this.tooltipTextContent.preferredHeight + this.paddingSize * 2f);
			}
		}

		if (this.target != null) {
			Vector2 pos = Input.mousePosition;
			pos.x -= this.xOffset;
			pos.y -= this.yOffset;
			this.rectTransform.localPosition = pos;
		}
	}

	public void SetText(string hint) {
		this.textMemory = hint;
	}

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

	public void SetTarget(RectTransform obj) {
		this.target = obj;
	}
}

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
	public GridLayoutGroup gridGroup;
	public Image panel;
	public Text tooltipTextContent;
	public RectTransform rectTransform;

	public RectTransform target;

	[Range(-400f, 400f)]
	public float xOffset;
	[Range(-400f, 400f)]
	public float yOffset;

	private String textMemory;

	public void Start() {
		this.panel = this.GetComponent<Image>();
		this.canvasGroup = this.GetComponent<CanvasGroup>();
		this.gridGroup = this.GetComponent<GridLayoutGroup>();
		this.rectTransform = this.GetComponent<RectTransform>();
		this.tooltipTextContent = this.GetComponentInChildren<Text>();
		if (this.paddingSize == 0) {
			this.paddingSize = 10;
		}
		this.transform.position = Vector3.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
	}

	public void Update() {
		if (this.panel != null && this.gridGroup != null) {
			this.gridGroup.padding.bottom = this.gridGroup.padding.top = this.gridGroup.padding.right = this.gridGroup.padding.left = this.paddingSize;
			Vector2 size = this.panel.rectTransform.sizeDelta;
			size.x -= this.gridGroup.padding.left * 2;
			size.y -= this.gridGroup.padding.top * 2;
			this.gridGroup.cellSize = size;
		}
		if (this.tooltipTextContent != null) {
			this.tooltipTextContent.text = this.textMemory;
		}

		if (this.target != null) {
			//this.rectTransform.localPosition = Vector3.zero;
			//Vector3 pos = Input.mousePosition;
			//pos.z = this.screenPlaneDistanceFromCamera;
			//pos = Camera.main.ScreenToWorldPoint(pos);
			//pos.z = 0f;
			//Vector3 targetPos = this.target.transform.position;
			//targetPos.z = 0f;
			////this.transform.position = targetPos + pos;
			//this.rectTransform.localScale = Vector3.one / this.screenPlaneDistanceFromCamera;
			//this.rectTransform.localPosition = targetPos + pos;
			Vector2 pos = Input.mousePosition;
			pos.x -= this.xOffset;
			pos.y -= this.yOffset;
			this.rectTransform.localPosition = pos;
			//Debug.Log("mouse: " + Input.mousePosition + " pos: " + this.transform.position + " rect: " + this.rectTransform.localPosition);
		}


	}

	public void SetText(string hint) {
		this.textMemory = hint;
	}

	public void SetToolTipHidden(bool flag) {
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

	public void SetTarget(RectTransform obj) {
		this.target = obj;
	}
}

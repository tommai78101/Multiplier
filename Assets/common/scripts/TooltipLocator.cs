using UnityEngine;
using System.Collections;

public class TooltipLocator : MonoBehaviour {
	public BaseTooltip tooltip;

	public void Start() {
		if (this.tooltip == null) {
			Debug.LogError("Cannot find tooltip.");
		}
		else {
			RectTransform rect = this.tooltip.GetComponent<RectTransform>();
			if (rect != null) {
				rect.localPosition = Vector3.zero;
				this.tooltip.transform.localScale = Vector3.one;
			}
		}
	}
}

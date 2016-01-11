using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class RainbowEffect : MonoBehaviour {
	public Shadow textShadow;
	public Text text;
	public float degrees;

	public void Start() {
		this.degrees = Random.Range(0f, 360f);
	}

	// Update is called once per frame
	void Update () {
		this.degrees = (this.degrees + Time.deltaTime * 50f) % 360f;
#if UNITY_5_2_3
		this.textShadow.effectColor = EditorGUIUtility.HSVToRGB(degrees / 360f, 1f, 1f);
		this.text.color = EditorGUIUtility.HSVToRGB(((degrees + 180f) % 360f) / 360f, 1f, 1f);
#else
		this.textShadow.effectColor = Color.HSVToRGB(degrees / 360f, 1f, 1f);
		this.text.color = Color.HSVToRGB(((degrees + 180f) % 360f) / 360f, 1f, 1f);
#endif
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tutorial {
	public class ImageManager : MonoBehaviour {
		public List<Transform> images;

		void Start() {
			this.images = new List<Transform>();
			foreach (Transform child in this.transform) {
				//NewRawImage img = child.GetComponent<NewRawImage>();
				//img.Initialize();
				//Color color = img.color;
				//color.a = 0f;
				//img.color = color;
				child.gameObject.SetActive(false);
				this.images.Add(child);
			}
		}

		public GameObject Obtain(int index) {
			for (int i = 0; i < this.images.Count; i++) {
				ImageIndex imageIndex = this.images[i].GetComponent<ImageIndex>();
				if (imageIndex != null && imageIndex.index == index) {
					return this.images[i].gameObject;
				}
			}
			return null;
		}
	}
}

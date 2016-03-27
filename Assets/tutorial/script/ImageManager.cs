using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tutorial {
	public class ImageManager : MonoBehaviour {
		public List<NewRawImage> images;

		void Start() {
			this.images = new List<NewRawImage>();
			foreach (Transform child in this.transform) {
				NewRawImage img = child.GetComponent<NewRawImage>();
				img.Initialize();
				Color color = img.color;
				color.a = 0f;
				img.color = color;
				this.images.Add(img);
			}
		}

		public NewRawImage Obtain(int index) {
			for (int i = 0; i < this.images.Count; i++) {
				if (this.images[i].index.index == index) {
					return this.images[i];
				}
			}
			return null;
		}
	}
}

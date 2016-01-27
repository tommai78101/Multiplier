using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class BoxSelector : MonoBehaviour {
		public Material borderMaterial;
		public Vector3 viewport_startVertex;
		public Vector3 viewport_endVertex;
		public Vector3 viewport_vertexIterator;
		public float panningElapsedTime;
		public Cursor mainCursor;

		public void Start() {
			this.panningElapsedTime = 2f;
			this.borderMaterial = (Material) Resources.Load("Border");
			this.viewport_endVertex = this.viewport_startVertex = -Vector3.one;
			if (this.mainCursor == null) {
				Debug.LogError("Cursor object is not set.");
			}
		}

		public void Update() {
			if (this.panningElapsedTime < 1f) {
				this.panningElapsedTime += Time.deltaTime;
				this.viewport_vertexIterator = Vector3.Lerp(this.viewport_startVertex, this.viewport_endVertex, this.panningElapsedTime);
			}



			if (Input.GetKeyUp(KeyCode.G)) {
				ObtainStartingPosition startPos = this.mainCursor.GetComponentInChildren<ObtainStartingPosition>();
				ObtainEndingPosition endPos = this.mainCursor.GetComponentInChildren<ObtainEndingPosition>();
				float startX = startPos.rectTransform.position.x / Screen.width;
				float startY = startPos.rectTransform.position.y / Screen.height;
				float endX = endPos.rectTransform.position.x / Screen.width;
				float endY = endPos.rectTransform.position.y / Screen.height;

				this.viewport_startVertex.x = startX;
				this.viewport_startVertex.y = startY;
				this.viewport_startVertex.z = 0f;

				this.viewport_endVertex.x = endX;
				this.viewport_endVertex.y = endY;
				this.viewport_endVertex.z = 0f;

				this.panningElapsedTime = 0f;
			}
		}

		public void OnPostRender() {
			if (this.borderMaterial == null) {
				Debug.LogError("Cannot obtain border material for selection box.");
				return;
			}

			if (this.viewport_startVertex == -Vector3.one || this.viewport_endVertex == -Vector3.one) {
				return;
			}

			GL.PushMatrix();
			{
				if (this.borderMaterial.SetPass(0)) {
					GL.LoadOrtho();
					GL.Begin(GL.LINES);
					{
						GL.Color(Color.green);
						//Top
						GL.Vertex(this.viewport_vertexIterator);
						GL.Vertex(new Vector3(this.viewport_startVertex.x, this.viewport_vertexIterator.y, 0f));
						//Right
						GL.Vertex(new Vector3(this.viewport_startVertex.x, this.viewport_vertexIterator.y, 0f));
						GL.Vertex(this.viewport_startVertex);
						//Bottom
						GL.Vertex(this.viewport_startVertex);
						GL.Vertex(new Vector3(this.viewport_vertexIterator.x, this.viewport_startVertex.y, 0f));
						//Left
						GL.Vertex(new Vector3(this.viewport_vertexIterator.x, this.viewport_startVertex.y, 0f));
						GL.Vertex(this.viewport_vertexIterator);
					}
					GL.End();
				}
			}
			GL.PopMatrix();
		}
	}
}

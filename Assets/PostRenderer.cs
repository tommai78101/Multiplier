using UnityEngine;
using System.Collections;

public class PostRenderer : MonoBehaviour {
	public Material borderMaterial;
	public Vector3 initialClick, startingVertex, endingVertex;

	//This script is added to the Main Camera. It is mostly used for post-processing the drawing of a box.

	public void Update() {
		if (Input.GetMouseButtonDown(0)) {
			this.initialClick = Input.mousePosition;

		}
		else if (Input.GetMouseButtonUp(0)) {
			this.initialClick = -Vector3.one;
		}
		this.startingVertex = GetStartingVertex();
		this.endingVertex = GetEndingVertex();
	}

	public void OnPostRender() {
		if (this.borderMaterial == null) {
			Debug.LogWarning("Border material is not set. Trying to fix it.");
			Material border = Resources.Load<Material>("Border");
			if (border != null) {
				this.borderMaterial = border;
			}
			return;
		}

		if (this.startingVertex == -Vector3.one || this.endingVertex == -Vector3.one) {
			return;
		}

		if (Input.GetMouseButton(0)) {
			GL.PushMatrix();
			if (this.borderMaterial.SetPass(0)) {
				GL.LoadOrtho();
				GL.Begin(GL.LINES);
				{
					GL.Color(Color.green);
					//Top line
					GL.Vertex(this.startingVertex);
					GL.Vertex(new Vector3(this.endingVertex.x, this.startingVertex.y, 0f));
					//Right line
					GL.Vertex(new Vector3(this.endingVertex.x, this.startingVertex.y, 0f));
					GL.Vertex(this.endingVertex);
					//Bottom line
					GL.Vertex(this.endingVertex);
					GL.Vertex(new Vector3(this.startingVertex.x, this.endingVertex.y, 0f));
					//Left line
					GL.Vertex(new Vector3(this.startingVertex.x, this.endingVertex.y, 0f));
					GL.Vertex(this.startingVertex);
				}
				GL.End();
			}
			GL.PopMatrix();
		}
	}

	public Vector3 GetStartingVertex() {
		if (this.initialClick == -Vector3.one) {
			return -Vector3.one;
		}
		float x = Input.mousePosition.x < this.initialClick.x ? Input.mousePosition.x : this.initialClick.x;
		float y = Input.mousePosition.y < this.initialClick.y ? Input.mousePosition.y : this.initialClick.y;
		x /= Screen.width;
		y /= Screen.height;
		return new Vector3(x, y, 0f);
	}

	public Vector3 GetEndingVertex() {
		if (this.initialClick == -Vector3.one) {
			return -Vector3.one;
		}
		float x = Input.mousePosition.x < this.initialClick.x ? this.initialClick.x : Input.mousePosition.x;
		float y = Input.mousePosition.y < this.initialClick.y ? this.initialClick.y : Input.mousePosition.y;
		x /= Screen.width;
		y /= Screen.height;
		return new Vector3(x, y, 0f);
	}
}

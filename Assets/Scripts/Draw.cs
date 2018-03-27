using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draw : MonoBehaviour
{
	public int verticesPerPoint = 6;
	public float stroke_width = 0.0015f;
	public List<int> ns;
	public List<Vector3> points;
	public List<Vector3> normals;
	public List<float> timestamps;
	private int pointsInCurStroke = 0;
	private MeshFilter mf;
	private Mesh stroke;

	private enum Mode
	{
Drawing,
		Erasing}

	;

	private Mode mode = Mode.Drawing;
	private MeshCollider mc;
	private bool drawnLastFrame;
	//private Material mat;
	// Use this for initialization
	void Start ()
	{
		mf = GetComponent<MeshFilter> ();
		mc = GetComponent<MeshCollider> ();
		stroke = new Mesh ();
		ns = new List<int> ();
		points = new List<Vector3> ();
		normals = new List<Vector3> ();
		timestamps = new List<float> ();
		//mat = GetComponent<Renderer>().material;
	}

	// Update is called once per frame
	void Update ()
	{
		bool drawn = false;
		if (Input.GetMouseButton (0)) {
			if (mode == Mode.Drawing) {
				drawn = DrawPoint ();
			} else {
				if (Input.GetMouseButtonDown (0)) {
					EraseStroke ();
				}				
			}
            
		}
		if (Input.GetMouseButtonUp (0) && drawnLastFrame) {
			ns.Add (pointsInCurStroke);
			pointsInCurStroke = 0;
		}
		drawnLastFrame = drawn;
	}

	bool DrawPoint ()
	{
		RaycastHit hitInfo;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		int layermask = 1 << 9;
		if (EventSystem.current.IsPointerOverGameObject()) {
			return false;
		}
		if (Physics.Raycast (ray, out hitInfo, Mathf.Infinity, layermask)) {
			Vector3[] vertices;
			Vector3[] normals;
			int[] triangles;
			if (mf.sharedMesh) {
				vertices = mf.sharedMesh.vertices;
				triangles = mf.sharedMesh.triangles;
				normals = mf.sharedMesh.normals;
				mf.sharedMesh.Clear ();
			} else {
				vertices = new Vector3[0];
				normals = new Vector3[0];
				triangles = new int[0];
			}
			int oldVerticeLength = vertices.Length;
			Array.Resize (ref vertices, oldVerticeLength + verticesPerPoint);
			Array.Resize (ref normals, oldVerticeLength + verticesPerPoint);
			MeshCollider meshCollider = hitInfo.collider as MeshCollider;
			Mesh mesh = meshCollider.sharedMesh;
			Vector3 bn;
			if (Input.GetMouseButtonDown (0)) {
				//First point of the stroke, no way to predict the tangent, so can only make a coarse approximation
				Vector3 vertex1 = mesh.vertices [mesh.triangles [hitInfo.triangleIndex * 3]];
				Vector3 vertex2 = mesh.vertices [mesh.triangles [hitInfo.triangleIndex * 3 + 1]];
				bn = vertex1 - vertex2;
			} else {
				Vector3 prevPOS = points [points.Count - 1];
				Vector3 tangent = (hitInfo.point - prevPOS).normalized;
				bn = Vector3.Cross (tangent, hitInfo.normal);
			}
			Vector3 n = hitInfo.normal;
			bn = bn.normalized;
			n = n.normalized;
			float r = stroke_width / 2.0f;

			for (int i = 0; i < verticesPerPoint; ++i) {
				vertices [oldVerticeLength + i] =
                hitInfo.point +
				(float)Mathf.Cos (2 * Mathf.PI * (i) / verticesPerPoint) * r * bn +
				(float)Mathf.Sin (2 * Mathf.PI * (i) / verticesPerPoint) * r * n;
				normals [oldVerticeLength + i] = (vertices [oldVerticeLength + i] - hitInfo.point).normalized;
			}

			if (!Input.GetMouseButtonDown (0)) {
				int oldTriangleLength = triangles.Length;
				Array.Resize (ref triangles, oldTriangleLength + verticesPerPoint * 6);
				for (int quad = 0; quad < verticesPerPoint; ++quad) {
					triangles [oldTriangleLength + quad * 6 + 0] = (oldVerticeLength - 6) + quad;
					triangles [oldTriangleLength + quad * 6 + 1] = oldVerticeLength + quad; 
					triangles [oldTriangleLength + quad * 6 + 2] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
					triangles [oldTriangleLength + quad * 6 + 3] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
					triangles [oldTriangleLength + quad * 6 + 4] = oldVerticeLength + quad;
					triangles [oldTriangleLength + quad * 6 + 5] = oldVerticeLength + (quad + 1) % verticesPerPoint;
				}
			}
			stroke.vertices = vertices;
			stroke.normals = normals;
			stroke.triangles = triangles;
			mf.sharedMesh = stroke;
			if (mc.sharedMesh) {
				mc.sharedMesh = null;
			}
			mc.sharedMesh = stroke;
			//Debug code
			/*GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.localScale = new Vector3(stroke_width, stroke_width, stroke_width);
            quad.transform.position = hitInfo.point;
            quad.transform.right = hitInfo.normal;
            quad.GetComponent<Renderer>().material = mat;
            if (!Input.GetMouseButtonDown(0)) {
                Vector3 prevPOS = points[points.Count - 1];
                Vector3 tangent = (hitInfo.point - prevPOS).normalized;
                Vector3 bn = Vector3.Cross(tangent, hitInfo.normal);//First point of the stroke, no way to predict the tangent, so can only make a coarse
                bn.Normalize();
                quad.transform.up = bn;
                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cylinder.transform.localScale = new Vector3(stroke_width/10f, stroke_width/10f, stroke_width/10f);
                cylinder.transform.position = hitInfo.point;
                cylinder.transform.up = tangent;
                cylinder.GetComponent<Renderer>().material = mat2;
            }*/
			AddNewPoint (ref hitInfo);
			return true;
		} else {
			return false;
		}	
	}

	void EraseStroke ()
	{
		RaycastHit hitInfo;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		int layermask = 1 << 8;
		if (Physics.Raycast (ray, out hitInfo, Mathf.Infinity, layermask)) {
			if (mf == null) {
				return;
			}
			int vIdx = 0;
			try {
				vIdx = mf.sharedMesh.triangles [hitInfo.triangleIndex * 3];
			} catch (IndexOutOfRangeException e) {
				Debug.Log (hitInfo.collider.name);
			}
			int pointIdx = vIdx / verticesPerPoint;
			int points = 0;
			int i;
			int size = ns.Count;
			for (i = 0; i < size; ++i) {
				points += ns [i];
				if (points >= pointIdx + 1) {
					break;
				}
			}
			int verticeLength = mf.sharedMesh.vertices.Length;
			Vector3[] vertices = new Vector3[0];
			Vector3[] normals = new Vector3[0];
			try {
				vertices = new Vector3[verticeLength - ns [i] * verticesPerPoint];
				normals = new Vector3[verticeLength - ns [i] * verticesPerPoint];
			}
			catch(OverflowException e) {
				int a = verticeLength;
				int b = ns [i];
				Debug.Log (a);
			}
			/*Vector3[] vertices = new Vector3[verticeLength - ns [i] * verticesPerPoint];
			Vector3[] normals = new Vector3[verticeLength - ns [i] * verticesPerPoint];*/
			int triangleLength = mf.sharedMesh.triangles.Length;
			int[] triangles = new int[0];
			try {
				triangles = new int[triangleLength - (ns [i] - 1) * verticesPerPoint * 6];
			} catch (ArgumentOutOfRangeException e) {
				int a = (ns [i] - 1) * verticesPerPoint * 6;
				Debug.Log (a);
			}
			/*Adding stuff uptil the stroke to be removed*/
			int numVertices = (points - ns [i]) * verticesPerPoint;
			int pt;
			for (pt = 0; pt < numVertices; ++pt) {
				vertices [pt] = mf.sharedMesh.vertices [pt];
				normals [pt] = mf.sharedMesh.normals [pt];
			}
			int numTriangles = (points - ns [i] - i) * verticesPerPoint * 6;
			int tpt;
			for (tpt = 0; tpt < numTriangles; ++tpt) {
				triangles [tpt] = mf.sharedMesh.triangles [tpt];
			}
			/*********************************************/
			/*Adding stuff after the stroke to be removed*/
			for (int ptSkipped = points * verticesPerPoint; ptSkipped < verticeLength; ++ptSkipped) {
				vertices [pt] = mf.sharedMesh.vertices [ptSkipped];
				normals [pt] = mf.sharedMesh.normals [ptSkipped];
				pt++;
			}
			int skipped = ns [i] * verticesPerPoint;
			for (int j = (points - i - 1) * verticesPerPoint * 6; j < triangleLength; ++j) {
				triangles [tpt] = mf.sharedMesh.triangles [j] - skipped;
				tpt++;
			}
			ns.RemoveAt (i);
			/*int prevStrokeSegs = 0;
			int curStrokeSegs = 0;
			for(int k=0; k < ns.Count; ++k){
				curStrokeSegs += ns[k] - 1;
				for (; prevStrokeSegs < curStrokeSegs; ++prevStrokeSegs) {
					for (int quad = 0; quad < verticesPerPoint; ++quad) {
						triangles[prevStrokeSegs * verticesPerPoint * 6 + quad * 6 + 0] = verticesPerPoint * (prevStrokeSegs - k) + quad;
						triangles[prevStrokeSegs * verticesPerPoint * 6 + quad * 6 + 1] = verticesPerPoint * (prevStrokeSegs - k + 1) + quad;
						triangles[prevStrokeSegs * verticesPerPoint * 6 + quad * 6 + 2] = verticesPerPoint * (prevStrokeSegs - k) + (quad + 1) % verticesPerPoint;
                    	triangles[prevStrokeSegs * verticesPerPoint * 6 + quad * 6 + 3] = verticesPerPoint * (prevStrokeSegs - k) + (quad + 1) % verticesPerPoint;
						triangles[prevStrokeSegs * verticesPerPoint * 6 + quad * 6 + 4] = verticesPerPoint * (prevStrokeSegs - k + 1) + quad;
						triangles[prevStrokeSegs * verticesPerPoint * 6 + quad * 6 + 5] = verticesPerPoint * (prevStrokeSegs - k + 1) + (quad + 1) % verticesPerPoint;
					}
				}
			}*/
			/*********************************************/
			mf.sharedMesh.Clear ();
			stroke.vertices = vertices;
			stroke.normals = normals;
			stroke.triangles = triangles;
			mf.sharedMesh = stroke;
			if(mc.sharedMesh) {
				mc.sharedMesh = null;
			}
			mc.sharedMesh = stroke;
		}
	}

	void AddNewPoint (ref RaycastHit hitInfo)
	{
		// We want to store information relative to the virtual object
		points.Add (hitInfo.point);
		normals.Add (hitInfo.normal);
		timestamps.Add (Time.time);
		pointsInCurStroke++;
	}

	public void SwitchMode ()
	{
		if (mode == Mode.Drawing) {
			mode = Mode.Erasing;
		} else {
			mode = Mode.Drawing;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public MeshFilter mf;
	public int verticesPerPoint = 6;
	public float stroke_width = 0.0015f;
	public float mouseSensitivity = 2.0f;
	public float zoomSensitivity = 0.1f;
	public float translationSpeed = 0.2f;
	public float orbitDampening = 2f;
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	private float rotationYAxis = 0.0f;
	private float rotationXAxis = 0.0f;
	private float velocityX = 0.0f;
	private float velocityY = 0.0f;
	public GameObject curModel;
	private Mesh stroke;
	//private Vector3 oldScreenPos;
	// Use this for initialization
	void Start ()
	{
		stroke = new Mesh ();
		Vector3 angles = transform.eulerAngles;
		rotationYAxis = angles.y;
		rotationXAxis = angles.x;
		transform.LookAt (curModel.transform);
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		Camera.main.transform.position += zoomSensitivity * distance * Input.GetAxis ("Mouse ScrollWheel") * Camera.main.transform.forward;
		if (Input.GetMouseButton (0)) {
			DrawPoint ();
		} else if (Input.GetMouseButton (2)) {
			TranslateCamera ();
		}
		else if (Input.GetMouseButton (1)) {
			RotateCameraAroundOrigin ();
		}

	}

	void RotateCameraAroundOrigin ()
	{
		/*if (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0) {
			velocityX += Input.GetAxis ("Mouse X") * mouseSensitivity;
			velocityY += Input.GetAxis ("Mouse Y") * mouseSensitivity; 
		}
		rotationYAxis += velocityX;
		rotationXAxis -= velocityY;
		rotationXAxis = ClampAngle (rotationXAxis, yMinLimit, yMaxLimit);
		Quaternion fromRotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
		Quaternion toRotation = Quaternion.Euler (rotationXAxis, rotationYAxis, 0);
		Quaternion rotation = toRotation;
		transform.rotation = rotation;
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + curModel.transform.position;
              
        transform.rotation = rotation;
        transform.position = position;
  
        velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * orbitDampening);
        velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * orbitDampening);*/
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		TranslateCamera ();
		transform.LookAt (curModel.transform);
		transform.position *= (distance / transform.position.magnitude);
	}

	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}

	void TranslateCamera ()
	{
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		if (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0) {
			Camera.main.transform.position += Camera.main.transform.right * -Input.GetAxis ("Mouse X") * mouseSensitivity * distance * translationSpeed;
			Camera.main.transform.position += Camera.main.transform.up * -Input.GetAxis ("Mouse Y") * mouseSensitivity * distance * translationSpeed;
		}
	}

	void DrawPoint ()
	{
		RaycastHit hitInfo;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hitInfo)) {
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
			Vector3 vertex1 = mesh.vertices [mesh.triangles [hitInfo.triangleIndex * 3]];
			Vector3 vertex2 = mesh.vertices [mesh.triangles [hitInfo.triangleIndex * 3 + 1]];
			Vector3 bn = vertex1 - vertex2;
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
					triangles [oldTriangleLength + quad * 6 + 1] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
					triangles [oldTriangleLength + quad * 6 + 2] = oldVerticeLength + quad;
					triangles [oldTriangleLength + quad * 6 + 3] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
					triangles [oldTriangleLength + quad * 6 + 4] = oldVerticeLength + (quad + 1) % verticesPerPoint;
					triangles [oldTriangleLength + quad * 6 + 5] = oldVerticeLength + quad;
				}
			}
			stroke.vertices = vertices;
			stroke.normals = normals;
			stroke.triangles = triangles;
			mf.sharedMesh = stroke;
		}
	}
}

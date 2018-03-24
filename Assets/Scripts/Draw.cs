using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Draw : MonoBehaviour {
    public int verticesPerPoint = 6;
    public float stroke_width = 0.0015f;
    public List<int> ns;
    public List<Vector3> points;
    public List<Vector3> normals;
    public List<float> timestamps;
    private int pointsInCurStroke = 0;
    private MeshFilter mf;
    private Mesh stroke;
    private Material mat;
    // Use this for initialization
    void Start () {
        mf = GetComponent<MeshFilter>();
        stroke = new Mesh();
        ns = new List<int>();
        points = new List<Vector3>();
        normals = new List<Vector3>();
        timestamps = new List<float>();
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButton(0)) {
            DrawPoint();
        }
        else if (Input.GetMouseButtonUp(0)) {
            ns.Add(pointsInCurStroke);
            pointsInCurStroke = 0;
        }
    }

    void DrawPoint() {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo)) {
            AddNewPoint(ref hitInfo);
            /*Vector3[] vertices;
            Vector3[] normals;
            int[] triangles;
            if (mf.sharedMesh) {
                vertices = mf.sharedMesh.vertices;
                triangles = mf.sharedMesh.triangles;
                normals = mf.sharedMesh.normals;
                mf.sharedMesh.Clear();
            }
            else {
                vertices = new Vector3[0];
                normals = new Vector3[0];
                triangles = new int[0];
            }
            int oldVerticeLength = vertices.Length;
            Array.Resize(ref vertices, oldVerticeLength + verticesPerPoint);
            Array.Resize(ref normals, oldVerticeLength + verticesPerPoint);
            MeshCollider meshCollider = hitInfo.collider as MeshCollider;
            Mesh mesh = meshCollider.sharedMesh;
            Vector3 bn;
            if (Input.GetMouseButtonDown(0)) {
                //First point of the stroke, no way to predict the tangent, so can only make a coarse approximation
                Vector3 vertex1 = mesh.vertices[mesh.triangles[hitInfo.triangleIndex * 3]];
                Vector3 vertex2 = mesh.vertices[mesh.triangles[hitInfo.triangleIndex * 3 + 1]];
                bn = vertex1 - vertex2;
            }
            else {
                Vector3 prevPOS = points[points.Count - 1];
                Vector3 tangent = (hitInfo.point - prevPOS).normalized;
                bn = Vector3.Cross(tangent, hitInfo.normal);
            }
            Vector3 n = hitInfo.normal;
            bn = bn.normalized;
            n = n.normalized;
            float r = stroke_width / 2.0f;

            for (int i = 0; i < verticesPerPoint; ++i) {
                vertices[oldVerticeLength + i] =
                hitInfo.point +
                (float)Mathf.Cos(2 * Mathf.PI * (i) / verticesPerPoint) * r * bn +
                (float)Mathf.Sin(2 * Mathf.PI * (i) / verticesPerPoint) * r * n;
                normals[oldVerticeLength + i] = (vertices[oldVerticeLength + i] - hitInfo.point).normalized;
            }

            if (!Input.GetMouseButtonDown(0)) {
                int oldTriangleLength = triangles.Length;
                Array.Resize(ref triangles, oldTriangleLength + verticesPerPoint * 6);
                for (int quad = 0; quad < verticesPerPoint; ++quad) {
                    triangles[oldTriangleLength + quad * 6 + 0] = (oldVerticeLength - 6) + quad;
                    triangles[oldTriangleLength + quad * 6 + 1] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
                    triangles[oldTriangleLength + quad * 6 + 2] = oldVerticeLength + quad;
                    triangles[oldTriangleLength + quad * 6 + 3] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
                    triangles[oldTriangleLength + quad * 6 + 4] = oldVerticeLength + (quad + 1) % verticesPerPoint;
                    triangles[oldTriangleLength + quad * 6 + 5] = oldVerticeLength + quad;
                }
            }
            stroke.vertices = vertices;
            stroke.normals = normals;
            stroke.triangles = triangles;
            mf.sharedMesh = stroke;*/
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
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
            }
        }
    }

    void AddNewPoint(ref RaycastHit hitInfo) {
        // We want to store information relative to the virtual object
        points.Add(hitInfo.point);
        normals.Add(hitInfo.normal);
        timestamps.Add(Time.time);
        pointsInCurStroke++;
    }
}

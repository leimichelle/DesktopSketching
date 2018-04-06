using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveAndSwitchModels : MonoBehaviour {
	public GameObject sketch;
	public CameraController CC;
	public Slider slider;
	private int modelIdx;
	// Use this for initialization
	void Start () {
		if (gameObject.transform.childCount > 0) {
			modelIdx = 0;
			CC.SetCurModel(gameObject.transform.GetChild (modelIdx).gameObject);
			gameObject.transform.GetChild (modelIdx).gameObject.SetActive (true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Return)) {
			SaveDrawing ();
			gameObject.transform.GetChild (modelIdx).gameObject.SetActive (false);
			if (modelIdx + 1 < gameObject.transform.childCount) {
				modelIdx++;
				GameObject nextModel = gameObject.transform.GetChild (modelIdx).gameObject;
				CC.SetCurModel(nextModel);
				nextModel.SetActive (true);
			}
			else {
				this.enabled = false;
			}
			slider.value = 1.0f;
		}
	}

	void SaveDrawing () {
		// TODO: Actually saving the stuff!!
		Draw draw = sketch.GetComponent<Draw> ();
		draw.ns.Clear ();
		draw.points.Clear ();
		draw.normals.Clear ();
		draw.timestamps.Clear ();
		MeshFilter mf = sketch.GetComponent<MeshFilter> ();
		MeshCollider mc = sketch.GetComponent<MeshCollider> ();
		if (mf.sharedMesh){
			mf.sharedMesh.Clear ();
		}
		if(mc.sharedMesh) {
			mc.sharedMesh = null;
		}
	}

	public void ToggleModelRenderer() {
		Renderer rend = gameObject.transform.GetChild (modelIdx).gameObject.GetComponent<Renderer> ();
		if (rend == null) {
			Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer> ();
			if (renderers.Length != 1) {
				Debug.Log("Model" + rend.name + "has zero or more than one renderer.");
				return;
			}
			else {
				rend = renderers[0];
			}
		}
		if (rend.enabled) {
			rend.enabled = false;
		}
		else {
			rend.enabled = true;
		}
	}

	public void ChangeTransparency(Slider s) {
		//argument alpha ranges from 0.0 to 1.0
		Renderer rend = gameObject.transform.GetChild (modelIdx).gameObject.GetComponent<Renderer> ();
		if (rend == null) {
			Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer> ();
			if (renderers.Length != 1) {
				Debug.Log("Model" + rend.name + "has zero or more than one renderer.");
				return;
			}
			else {
				rend = renderers[0];
			}
		}
		Color c = rend.material.color;
		c.a = s.normalizedValue;
		rend.material.color = c;
	}
}

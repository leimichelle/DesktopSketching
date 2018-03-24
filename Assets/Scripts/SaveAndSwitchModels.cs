using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAndSwitchModels : MonoBehaviour {
	public MeshFilter mf;
	public CameraController CC;
	private int modelIdx;
	// Use this for initialization
	void Awake () {
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
				CC.SetCurModel(gameObject.transform.GetChild (modelIdx).gameObject);
				gameObject.transform.GetChild (modelIdx).gameObject.SetActive (true);
			}
			else {
				this.enabled = false;
			}
		}
	}

	void SaveDrawing () {
		if (mf.sharedMesh){
			mf.sharedMesh.Clear ();
		}
	}
}

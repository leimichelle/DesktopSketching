using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
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
	private GameObject curModel;
    private Vector3 pivot;
	//private Vector3 oldScreenPos;
	// Use this for initialization
	void Start ()
	{
		Vector3 angles = transform.eulerAngles;
		rotationYAxis = angles.y;
		rotationXAxis = angles.x;
        if (curModel) {
            transform.LookAt(curModel.transform);
            //curModel.GetComponent<Renderer>().enabled = false;
        }
        pivot = new Vector3(0f, 0f, 0f);

    }
	
	// Update is called once per frame
	void LateUpdate ()
	{
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		Camera.main.transform.position += zoomSensitivity * distance * Input.GetAxis ("Mouse ScrollWheel") * Camera.main.transform.forward;		
		if (Input.GetMouseButton (2)) {
			TranslateCamera (0);
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
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            float distance = Vector3.Distance(Camera.main.transform.position, curModel.transform.position);
            TranslateCamera(1);
            //transform.LookAt (curModel.transform);
            transform.LookAt(pivot);
            transform.position *= (distance / transform.position.magnitude);
        }
	}

	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}

	void TranslateCamera (int flag)
	{
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		if (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0) {
            Vector3 rightTranslation = Camera.main.transform.right * -Input.GetAxis("Mouse X") * mouseSensitivity * distance * translationSpeed;
            Vector3 upTranslation = Camera.main.transform.up * -Input.GetAxis("Mouse Y") * mouseSensitivity * distance * translationSpeed;
            Camera.main.transform.position += rightTranslation;
            Camera.main.transform.position += upTranslation;
            if (flag == 0) {
                pivot += rightTranslation;
                pivot += upTranslation;
            }
		}
	}

    public void SetCurModel(GameObject model) {
        curModel = model;
    }
}

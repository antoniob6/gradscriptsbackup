using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class PlayerCamera : MonoBehaviour {
    public GameObject TargetObject;
    private PlayerConnectionObject parentPCO;
    public int zoomLimit = 5;

    private int zoomLevel = 2;
    private Camera CameraComponenet;
    private float startingCameraSize;
    void Start() {
        parentPCO = transform.parent.GetComponent<PlayerConnectionObject>();
        CameraComponenet= GetComponent<Camera>();
        startingCameraSize= CameraComponenet.orthographicSize;
    }

    // Update is called once per frame
    void Update () {
        int previousSelectedZoomLevel = zoomLevel;
        if (TargetObject) { 
            transform.position = TargetObject.transform.position;
            transform.position += Vector3.back;
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, 0, TargetObject.transform.rotation.eulerAngles.z);

            if (GravitySystem.instance.gravityType != GravitySystem.GravityType.ToCenter &&
                GravitySystem.instance.isReverseGravity) {
                transform.Rotate(0, 0, 180);
            }
            
        }
        if (parentPCO.isLocal()) {//local player can change weapon
            if (Input.GetAxis("Mouse ScrollWheel") < 0.0f) {
                if (zoomLevel >= zoomLimit)
                    zoomLevel = zoomLimit;
                else
                    zoomLevel++;
            }
            
            if (Input.GetAxis("Mouse ScrollWheel") > 0.0f) {
                if (zoomLevel <= 1)
                    zoomLevel = 1;
                else
                    zoomLevel--;
            }
                        

        }
        if (previousSelectedZoomLevel != zoomLevel) {
            selectZoom();
        }

    }

    void selectZoom() {
        CameraComponenet.orthographicSize = zoomLevel * startingCameraSize;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementControls : MonoBehaviour {
    Camera mainc;
	// Use this for initialization
	void Start () {
        mainc = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        checkInput();
	}

    private void checkInput() {

        if (Input.GetKey(KeyCode.W)) {
            mainc.orthographicSize -= 2;
        }
        if (Input.GetKey(KeyCode.S)) {
            mainc.orthographicSize += 2;
        }
        if (Input.GetKey(KeyCode.A)) {
            transform.Translate(-2, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            transform.Translate(2, 0, 0);
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterface : MonoBehaviour {
    public GameObject MainMenuCanvas;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void openMainMenu() {
        MainMenuCanvas.SetActive(true);

    }
    public void closeMainMenu() {
        MainMenuCanvas.SetActive(false);

    }

}

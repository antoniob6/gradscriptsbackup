using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BackGroundManager : NetworkBehaviour {
    public Sprite[] backgroundImages;
    public Image canvas;
    public float imageTime=5f;
    [HideInInspector] public int imageIndex=0;

    private float timeLeft;
	// Use this for initialization
	void Start () {
        timeLeft = imageTime;
	}
	
	// Update is called once per frame
	void Update () {

        /*
        if (isLocalPlayer&& backgroundImages.Length>=1) {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f) {
              //  Debug.Log("changing background image");
                timeLeft = imageTime;
                CmdNextImage();
            }
        }
        */
		
	}
    public void nextImage() {
        if (!isLocalPlayer || backgroundImages.Length < 1)
            return;
        imageIndex++;
        if (imageIndex == backgroundImages.Length)
            imageIndex = 0;

        updateImage();

    }


    [Command] public void CmdNextImage() {
        RpcNextImage();
    }
    [ClientRpc]public void RpcNextImage() {
        if (!isLocalPlayer)
            return;
        imageIndex++;
        if (imageIndex == backgroundImages.Length)
            imageIndex = 0;

        updateImage();
    }
    public void updateImage() {
        canvas.sprite = backgroundImages[imageIndex];
    }
}

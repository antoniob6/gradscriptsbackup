/*
 * this give a message to all the clients 
 * meaning a message that pops on the screen that all players see,
 * obviously the message is synced across all the clients and so everyone sees the same message, as intended
 *
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameMessage : NetworkBehaviour {


    public Text textUI;

    [SyncVar]
    public string sText = "MessageText";

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    [ClientRpc]
    public void RpcUpdateText(string text) {
        sText = text;

        textUI.text= text;

    }
    public void onClick() {
      //  Debug.Log("clicked on message");
       // Destroy(gameObject);

    }



}

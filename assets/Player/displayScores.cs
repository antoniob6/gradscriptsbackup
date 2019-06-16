using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class displayScores : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isActiveAndEnabled)
            return;
        string text = "game has ended, here are the scores \n";
        PlayerConnectionObject[] PCOs = FindObjectsOfType<PlayerConnectionObject>();
        foreach(PlayerConnectionObject PCO in PCOs) {
            if (!PCO)
                continue;
            PlayerData pdata = PCO.GetComponent<PlayerData>();
            text += pdata.playerName + " score: " + pdata.score + "\n";
         
        }
        Text textRefernce = GetComponent<Text>();
        if (textRefernce.text!=text)
            textRefernce.text = text;
		
	}
}

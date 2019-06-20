
using System.Collections.Generic;
using UnityEngine;


public class MatchPlayerLocRot : MonoBehaviour {
    PlayerConnectionObject PCO;
	// Use this for initialization

     private GameObject targetToMatch;
	void Start () {
        PCO = GetComponentInParent<PlayerConnectionObject>();
        transform.SetParent(null);

        //transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
	}
	
	// Update is called once per frame
	void Update () {
        if (targetToMatch) {
            transform.position =new Vector3( targetToMatch.transform.position.x, targetToMatch.transform.position.y,5f);
           // transform.rotation = targetToMatch.transform.rotation;
        } else {
            if (PCO) {
                if (PCO.playerBoundingCollider) {
                    targetToMatch = PCO.playerBoundingCollider.gameObject;
                }
            } else {
                PCO = GetComponentInParent<PlayerConnectionObject>();
            }
        }
	}
}

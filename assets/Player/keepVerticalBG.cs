using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keepVerticalBG : MonoBehaviour {
    public GameObject targetToMatch;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (targetToMatch) {
            transform.rotation = targetToMatch.transform.rotation;
            Debug.Log("rotation = " + targetToMatch.transform.rotation);
            Debug.Log("rotation2 = " + transform.rotation);
        } else {
            PlayerConnectionObject PCO = GetComponentInParent<PlayerConnectionObject>();
            if (PCO && PCO.playerBoundingCollider) {
                targetToMatch = PCO.playerBoundingCollider.gameObject;
            }
        }
	}
}

/*
 * give the apropriate rotation to the player so he keeps standing upwards
 * (not upside down)
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetaryRotation : MonoBehaviour {


    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        GameObject gs = GravitySystem.instance.gameObject;
        if (!gs) {
            return;
        }
        if (GravitySystem.instance.gravityType==GravitySystem.GravityType.ToCenter ||
            GravitySystem.instance.gravityType == GravitySystem.GravityType.ToOut)
        {
            Vector3 gravityUp = transform.position - gs.transform.position ;
            Quaternion targetRotatoion = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotatoion, 50 * Time.deltaTime);
            transform.rotation = targetRotatoion;
        }
        else if(GravitySystem.instance.gravityType == GravitySystem.GravityType.Down ||
            GravitySystem.instance.gravityType == GravitySystem.GravityType.Up) {
            if(transform.rotation!= Quaternion.identity)
            transform.rotation = Quaternion.identity;
        }
    }
}

/*this class stops the characters from falling outside of map
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class KillOutOfMapChecker : NetworkBehaviour {

    private float maxRecoveryTime = 1f;
    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame

    private float lastDamageTime = 0f;
    void Update () {
        if (Time.time - lastDamageTime <= maxRecoveryTime) {
            // Debug.Log("damage prevented: " + (Time.time - lastDamageTime));
            return;
        }
        lastDamageTime = Time.time;

        //Debug.Log("checking on obj");

        if (GravitySystem.instance.didObjFallOutside(transform.position)) {



            PlayableCharacter PC = GetComponent<PlayableCharacter>();
            if (PC) {
               // Debug.Log("player fallen outside of map");
                PlayerReceiveDamage PRD = PC.PCO.GetComponent<PlayerReceiveDamage>();
                if (PRD.isLocalPlayer) {
                    //Debug.Log("giving damage to player that fell out of map");
                    PRD.CmdTakeDamage(1000);
                }
                return;
            }
            EnemyRecieveDamage ERD = GetComponent<EnemyRecieveDamage>();
            if (ERD) {
               // Debug.Log("enemy fallen outside of map");
                if(ERD.hasAuthority|| ERD.isLocalPlayer)
                    ERD.CmdTakeDamage(1000);
                return;
            }

            //Debug.Log("some unknown object fell outside of map");


        }
	}
}

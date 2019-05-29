/*this class stops the characters from falling outside of map
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RespawnOutOfMapChecker : NetworkBehaviour {

    private float maxRecoveryTime = 1f;
    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame

    private float lastDamageTime = 0f;
    void Update () {
        if (!isServer)
            return;
        //Debug.Log("checking on obj");
        
        if (GravitySystem.instance.didObjFallOutside(transform.position)) {
            if (Time.time - lastDamageTime <= maxRecoveryTime) {
                // Debug.Log("damage prevented: " + (Time.time - lastDamageTime));
                return;
            }
            lastDamageTime = Time.time;

            MapManager MM = FindObjectOfType<MapManager>();
            if (MM) {
                Vector3 spawnPosition= MM.getRandomPositionAboveMap();

                //Debug.Log("respawning player on the server");
                transform.position = spawnPosition;

                //RpcChangeLocalPosition(spawnPosition);
            }
            //Debug.Log("some unknown object fell outside of map");


        }
	}

    [ClientRpc]public void RpcChangeLocalPosition(Vector3 newPos) {
        if (!isLocalPlayer)
            return;
        Debug.Log("respawning player on the local instance");
        transform.position = newPos;
    }


}

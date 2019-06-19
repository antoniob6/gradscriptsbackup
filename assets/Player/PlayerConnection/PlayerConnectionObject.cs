/* this is what connects the server to the clients and the other way around
 * it hold all the information about the client that the server needs
 * and is responsible for the player character
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerConnectionObject : NetworkBehaviour
{
    public GameObject[] spawnableCharacters;
    public PlayerCamera playerCamera;
    public GameObject spawnBoxPrefab;

    [HideInInspector]   public bool active=false;

    [HideInInspector]public BoxCollider2D playerBoundingCollider;
    [HideInInspector] public PlayableCharacter PC;


    public bool facingRight = true;

    private int index = 0;
    
    private void Start() {
        if (!isLocalPlayer) {
            active = false;
            //Debug.Log("not local player on client");
            return;
        }
        active = true;
        Invoke("delayedStart", 2f);

    }


    public void delayedStart() {
        CmdSpawnMyUnit();
    }


    [SyncVar(hook = "OnPlayerNameChanged")]
    public string PlayerName = "Anonymous";

    // Update is called once per frame
    void Update() {
        if (!isLocalPlayer) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.C)) {

            if (index >= spawnableCharacters.Length-1) {
                index = 0;
            } else {
                index++;
            }
            Debug.Log("changing character "+ index);

            CmdSpawnMyUnit();
        }

    }

    void OnPlayerNameChanged(string newName) {
        PlayerName = newName;
        gameObject.name = "PlayerConnectionObject [" + newName + "]";
    }


    [Command]
    public void CmdSpawnMyUnit() {
        Vector3 up = GravitySystem.instance.getUpDirection(transform.position);
        Vector3 spawnPoint = transform.position+up*2;//old character position

        if (playerBoundingCollider != null) {//replace an existing character
            spawnPoint = playerBoundingCollider.gameObject.transform.position;
            NetworkServer.Destroy(playerBoundingCollider.gameObject);
        } else {                    //or spawn in a random position
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("Spawn");
            if (spawnObjects.Length != 0) {
                int randomSpawnIndex = Random.Range(0, spawnObjects.Length);
                spawnPoint = spawnObjects[randomSpawnIndex].transform.position;
            }
        }

        MapManager MM = FindObjectOfType<MapManager>();
        if(MM)
            spawnPoint = MM.getRandomPositionAboveMap();
        //spawn the charater int the position
        GameObject PlayerObject = Instantiate(spawnableCharacters[index],spawnPoint,Quaternion.identity);
        if (!PlayerObject) {
            Debug.Log("couldn't spawn player object: "+gameObject.name);
        }
        NetworkServer.SpawnWithClientAuthority(PlayerObject, connectionToClient);
        RpcUpdateTarget(PlayerObject);

     //   PC = PlayerObject.GetComponent<PlayableCharacter>();
     //   playerBoundingCollider = PC.getPlayerBoundingCollider();
     //   PC.RD = gameObject.GetComponent<PlayerReceiveDamage>();
       // PC.PCO = this;


    }
    

    [ClientRpc]void RpcUpdateTarget(GameObject newTarget) {
        if (isLocalPlayer) {
          //  Debug.Log("updated camera on local player");
            playerCamera.TargetObject = newTarget;


        } else {//instances that are on the other clients

        }

        PC = newTarget.GetComponent<PlayableCharacter>();
        playerBoundingCollider = PC.getPlayerBoundingCollider();
        PC.RD = gameObject.GetComponent<PlayerReceiveDamage>();
        PC.PCO = this;

    }
    

    [Command]
    void CmdChangePlayerName(string n) {
        Debug.Log("CmdChangePlayerName: " + n);
        PlayerName = n;
        // Tell all the client what this player's name now is.
        //RpcChangePlayerName(PlayerName);
    }
    public Camera getPlayerCamera() {
        return playerCamera.GetComponent<Camera>();
    }


    public void printString(string s) {
        CmdPrintString(s);
    }
    [Command] void CmdPrintString(string s) {
        print(s);
    }

    public bool isLocal() {
        return isLocalPlayer;
    }


    public void relayERDAttack(EnemyRecieveDamage ERD, int amount, PlayerData PDWhoHit) {
        if (!ERD)
            return;
        //Debug.Log("local player trying to damage enemy");
        
        CmdRelayERDAttack(ERD.gameObject, amount, PDWhoHit.gameObject);
    }
    [Command] public void CmdRelayERDAttack(GameObject damageRecieverGO,int amount, GameObject damagerGO) {
        EnemyRecieveDamage ERD = damageRecieverGO.GetComponent<EnemyRecieveDamage>();
        PlayerData PDWhoHit = damagerGO.GetComponent<PlayerData>();
        if (!ERD || !PDWhoHit) {
            Debug.Log("cant find ERD/PDWhoHit from object on the server");
        }

        ERD.takeDamageWithPD(amount, PDWhoHit);

    }


    [Command]public void CmdSpawnBoxOnPosition(Vector3 spawnPosition) {
        if (!spawnBoxPrefab) {
            Debug.Log("spawn box prefab not assigned");
            return;
        }
        GameObject go = Instantiate(spawnBoxPrefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(go);

    }

    
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayableCharacter : NetworkBehaviour {

    public Text playerTextRefernce;
    public GameObject deathEffect;
    public BoxCollider2D PlayerBoundingCollider;

    public Animator animator;
    public Rigidbody2D rigibodyComp;
    public SpriteRenderer spriteRenderer;


    public PlayerReceiveDamage RD;//set up by PlayerConnectionObject
    public PlayerConnectionObject PCO;//set up by PlayerConnectionObject
    // public float timeBetweenDmg;


    private void Start() {
        if (!animator)
            animator = GetComponent<Animator>();
        if (!rigibodyComp)
            rigibodyComp = GetComponent<Rigidbody2D>();
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        

        if (!spriteRenderer || !rigibodyComp || !animator)
            Debug.Log("please assign PC components manually");
        Invoke("delayedDisplayName", 2f);

    }
    void delayedDisplayName() {
        if (RD) {
            setDisplayName(RD.gameObject.GetComponent<PlayerData>().playerName);
            setPlayerColor(RD.gameObject.GetComponent<PlayerData>().playerColor);
        }
    }
    public void setPlayerColor(Color newColor) {
        if (spriteRenderer) {
            spriteRenderer.color = newColor;
        }
    }

    public void setDisplayName(string newName) {
            if (playerTextRefernce) {
                playerTextRefernce.text = newName;
           // Debug.Log("set player name to: " + newName);
            }
        
    }

    public BoxCollider2D getPlayerBoundingCollider() {
        if (!PlayerBoundingCollider) {
            Debug.Log("player bounding collider not assigned");
            return null;
        }
        return PlayerBoundingCollider;
    }

    void OnTriggerEnter2D(Collider2D collider) {//runs on all instances
                                     //but later deals damage only on server

        if (!RD) {
            Debug.Log("RD not assigned");
            return;
        }
        RD.characterTriggered(collider);
    }


    public void playerDied() {
        if (!isServer)
            return;
 MapManager MM = FindObjectOfType<MapManager>();
        if (!MM) {
            Debug.Log("map manager not found");
            return;
        }
        Vector3 newSpawnPos = MM.getRandomPositionAboveMap();
        
        PCO.PC.RpcPlayerDied(newSpawnPos);
    }
    
    [ClientRpc]public void RpcPlayerDied(Vector3 newSpawnPos) {
        //respawn the player
        //transform.position = newSpawnPos;
        //RD.currentHealth = RD.maxHealth;
        //RD.didWeCheckDeath = false;
        StartCoroutine(deathCo(newSpawnPos,3));
    }

    IEnumerator deathCo(Vector3 newSpawnPos, float respawnTime) {
        if (deathEffect) {
            GameObject DE = Instantiate(deathEffect, gameObject.transform.position, transform.rotation);
            DE.transform.parent = transform;
            AudioManager.instance.play("playerDied");
            
        } else {
            Debug.Log("player death effect not assigned");
        }


        Color oldSpriteColor =Color.white;
        //freze player because dead

        UnityEngine.RigidbodyConstraints2D oldConstraints = rigibodyComp.constraints;
        if (rigibodyComp)
            rigibodyComp.constraints = RigidbodyConstraints2D.FreezeAll;
        if (animator)
            animator.SetBool("IsDead", true);
        if (spriteRenderer) {
            oldSpriteColor = spriteRenderer.color;
            spriteRenderer.color = Color.black;
        }


        yield return new WaitForSeconds(respawnTime);


        if(rigibodyComp)
            rigibodyComp.constraints = oldConstraints;
        if (animator)
            animator.SetBool("IsDead", true);
        if (spriteRenderer)
            spriteRenderer.color = oldSpriteColor;


        //respawn the player
        transform.position = newSpawnPos;
        RD.currentHealth =RD.maxHealth;
        RD.didWeCheckDeath = false;

    }

    [ClientRpc] public void RpcPlayerTakenDamage() {
        animator.SetBool("IsHurt", true);
        Invoke("finishedTakingDamage", 0.5f);
        rigibodyComp.velocity=Vector2.zero;
      //  Debug.Log("adding kickback to player");


    }
    void finishedTakingDamage() {
        animator.SetBool("IsHurt", false);
    }



    [ClientRpc]public void RpcTeleportObject(Vector3 newPosition) {
        if (!hasAuthority)
            return;
        transform.position = newPosition;
    }
   

    






}

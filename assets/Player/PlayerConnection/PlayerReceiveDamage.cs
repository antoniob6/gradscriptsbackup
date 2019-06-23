/* 
 * gives the object a way to interact with other objects
 * it checks the objects that collides with this object and gives the apropriate response 
 * such as gitting hit by bullets, reducing the health, and giving the effect of the player dying
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerReceiveDamage : NetworkBehaviour {
    public GameObject recieveDamageEffect;


	public int maxHealth = 10;

	[SyncVar]public int currentHealth;

    public float maxRecoveryTime=0.5f;
    public float deathCoolDownTime = 5f;
    [SerializeField]
	private bool destroyOnDeath;

	public Vector2 initialPosition;
    private Rigidbody2D m_Rigidbody2D;
    private bool isdead = false;


    public GameObject lastHitby = null;


    private PlayerData PD;
    private PlayerConnectionObject PCO;
    private void Start() {
        PCO = GetComponent<PlayerConnectionObject>();
        this.currentHealth = this.maxHealth;
        this.initialPosition = this.transform.position;
        PD = GetComponent<PlayerData>();
    }



    public int getHealth() {
        return currentHealth;
    }


    private float lastDamageTime = 0f;
    public void characterTriggered(Collider2D collider) {
        if (Time.time - lastDamageTime <= maxRecoveryTime) {
           // Debug.Log("damage prevented: " + (Time.time - lastDamageTime));
            return;
        }

        //Debug.Log("character triggered: "+ currentHealth);
        //hit by bullet
        if (collider.tag == "Bullet" ) {
            //Destroy(collider.gameObject);
            if (isServer && collider.gameObject.GetComponent<Bullet>().owner != gameObject.GetComponent<PlayerReceiveDamage>()) {
                lastHitby = collider.gameObject.GetComponent<Bullet>().owner.gameObject;
                NetworkServer.Destroy(collider.gameObject);
                TakeDamage(1);
            }

           
           // m_Rigidbody2D.velocity = new Vector3(0, 0, 0);
           
           

        }
        //hit by enemy
        else if (collider.tag == "Enemy") {
            Destroy(collider.gameObject);
            this.TakeDamage (1);

			
		} else if (collider.tag == "Boss") {//the boss deals damage to the player
            //Destroy(collider.gameObject);
            //this.TakeDamage(3);

        }
  //touched the death layer
  else if (collider.tag == "Death")
        {
            TakeDamage(100);
            m_Rigidbody2D.velocity = new Vector3(0, 0, 0);

        } else if(collider.tag == "Candy") {
            if(isServer)
                PD.playerCollectedCandy();
        }

    }

    private float lastCheckTime = 0f;
    public bool didWeCheckDeath = false;


	public void TakeDamage(int amount) {
        AudioManager.instance.play("playerGotDamaged");
        if (!isServer)//only server deals actual damage
            return;
        lastDamageTime = Time.time;
        currentHealth -= amount;//reduce the amount of health

        if (recieveDamageEffect) {//add an effect that damage was received
            GameObject GO = Instantiate(recieveDamageEffect, PCO.PC.transform.position, Quaternion.identity);
            NetworkServer.Spawn(GO);
        } else {
            Debug.Log("effect recieve damage  not assigned");
        }

        if (PD)
            PD.takenDamage(currentHealth, maxHealth);
        else
            Debug.Log("PD not assigned");

        if (currentHealth <= 0) {//player died
            if(!didWeCheckDeath || Time.time - lastCheckTime >= deathCoolDownTime){// we only die once
                // second part of the || is a fail safe incase death wasn't registerd (e.g. packet loss)
                didWeCheckDeath = true;
                lastCheckTime = Time.time;

                if (PD) {
                    PD.playerDied();
                }
                if (lastHitby) {//last hit was by another player
                    PlayerData hitByPD = lastHitby.gameObject.GetComponent<PlayerData>();
                    if (hitByPD != null) {
                        hitByPD.playerKilledPlayer();
                    }
                } else {
                    //died but was never hit by any player
                }

                    
			}
		}


    }
    [Command]
    public void CmdTakeDamage(int damage) {
        TakeDamage(damage);
    }










}

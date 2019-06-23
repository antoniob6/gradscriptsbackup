using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Boss : NetworkBehaviour {


    public int damage=3;
    public float timeBtwDamage = 1.5f;
    private float timeBtwLeft = 1.5f;
    public Slider healthBar;
    private Animator anim;
    public bool isDead;
    private EnemyRecieveDamage RD;
    private void Start()
    {
        anim = GetComponent<Animator>();
        RD = GetComponent<EnemyRecieveDamage>();
    }

    private void Update()
    {

        if (RD.currentHealth <= 25) {
            if(anim)
                anim.SetTrigger("stageTwo");
        }

        if (RD.currentHealth <= 0) {
            if(anim)
                anim.SetTrigger("death");
        }

        // give the player some time to recover before taking more damage !
        if (timeBtwLeft > 0) {
            timeBtwLeft -= Time.deltaTime;
        }
        if(healthBar)
            healthBar.value = RD.currentHealth;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer)//only server version deals damage
            return;
        if (tag.Equals("Enemy")) {
            return;
        }
        //Debug.Log("touched plyer"+timeBtwLeft);
        if (other.CompareTag("PlayerObject") && isDead == false) {
            if (timeBtwLeft <= 0) {
                PlayableCharacter PC = other.GetComponent<PlayableCharacter>();
                if (PC) {//collided with player deal damage
                    timeBtwLeft = timeBtwDamage;
                    PC.RD.TakeDamage(damage);
                    //Debug.Log("boss touched player dealing damage");
                }
            }
        } 
    }
}

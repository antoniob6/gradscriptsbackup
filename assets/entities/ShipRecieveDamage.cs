using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipRecieveDamage : NetworkBehaviour
{
    public GameObject onDamageEffect;
    public GameObject onDeathEffect;
    public SimpleHealthBar SHB;
    [SerializeField] private int maxHealth = 5;
    [SyncVar] public int currentHealth = 5;

    public bool destroyWhenDead = true;
    private bool dead = false;


    void Start() {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update() {

    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Enemy") {
            //Debug.Log("got hit by enemy");
            TakeDamage(1);
            Destroy(collider.gameObject);
        }
    }
    void updateHeathBar() {
        if (!SHB)
            return;
        if(currentHealth>=1)
            SHB.UpdateBar(currentHealth-1, maxHealth);
        else
            SHB.UpdateBar(0, maxHealth);

    }
    void TakeDamage(int amount) {
        updateHeathBar();
        if (!isServer)
            return;
        currentHealth -= amount;
        onDamage();
        if (this.currentHealth <= 0 && !dead) {//entity dead
            EntityDied();

            if (destroyWhenDead)
                Destroy(gameObject);
        }


    }

    private void EntityDied() {
        dead = true;

    }

  

    private void onDamage() {
        if (onDamageEffect) {
            Instantiate(onDamageEffect, transform.position, transform.rotation);
        }
    }


    private void OnDestroy() {
        if (onDeathEffect) {
            Instantiate(onDeathEffect, transform.position, transform.rotation);
        }
    }
}

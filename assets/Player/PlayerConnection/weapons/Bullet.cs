using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public GameObject bulletHitEffect;
    [HideInInspector]
    public PlayerReceiveDamage owner;
    [HideInInspector]
    public PlayerData ownerPD;
    private int bounce = 0;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnCollisionEnter2D(Collision2D collision) {
        if (bounce >= 2) {
            Destroy(gameObject);
            return;
        }

        if (bulletHitEffect) {
            Instantiate(bulletHitEffect, transform.position, Quaternion.identity);
            //Debug.Log("bulletImpact effect");
        }
        bounce++;
    }






}

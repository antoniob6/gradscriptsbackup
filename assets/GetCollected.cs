using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCollected : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            PlayerReceiveDamage RD = collider.gameObject.GetComponent<PlayerReceiveDamage>();
            RD.currentHealth++;
            float oldScore = collider.gameObject.GetComponent<PlayerData>().score;

            collider.gameObject.GetComponent<PlayerData>().RpcUpdateScore(oldScore+10);


            Debug.Log("touched player");
            Destroy(gameObject);

        }
        if (collider.gameObject.tag == "Bullet")
        {
            Bullet bullet = collider.gameObject.GetComponent<Bullet>();
            

            float oldScore = bullet.owner.GetComponent<PlayerData>().score;

            bullet.owner.GetComponent<PlayerData>().RpcUpdateScore(oldScore+20);


            Debug.Log("touched bullet");
            Destroy(gameObject);
        }

    }
}

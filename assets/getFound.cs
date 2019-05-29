using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getFound : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
           // PlayerReceiveDamage RD = collider.gameObject.GetComponent<PlayerReceiveDamage>();
            collider.gameObject.GetComponent<PlayerData>().RpcAddScore(40);

            
            Debug.Log("got Found by player");
            Destroy(gameObject);

        }
        if (collider.gameObject.tag == "Bullet")
        {
            Bullet bullet = collider.gameObject.GetComponent<Bullet>();

            if (bullet) {
                PlayerData bulletOwnerPD = bullet.owner.GetComponent<PlayerData>();
                if(bulletOwnerPD)
                    bulletOwnerPD.RpcAddScore(100);

            }


            Debug.Log("got hit by bullet");
            Destroy(gameObject);
        }

    }
}

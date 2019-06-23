/*this makes the player shoot bullets and remembers who is the one that shot them,
 * in the latest revision the bullets are shot according to where the mouse is clicked
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShootBullets : NetworkBehaviour {

    [SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private float bulletSpeed;

    Vector3 bulletSpawnPoint=Vector3.zero;
    public void Shoot() {

        Camera playerCamera =
            GetComponent<PlayerConnectionObject>().getPlayerCamera();


        BoxCollider2D playerBoundingCollider =
             GetComponent<PlayerConnectionObject>().playerBoundingCollider;
        if(!playerCamera || !playerBoundingCollider) {
            Debug.Log("player character still not assigned");
            return;
        }

        
            

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = playerCamera.ScreenToWorldPoint(mousePosition);
        Vector2 clickDiffrence = mousePosition - playerBoundingCollider.transform.position;
        float distance = clickDiffrence.magnitude;
        //Debug.Log(distance);
        //if the click is too close to the player then cancel
        if (clickDiffrence.sqrMagnitude < playerBoundingCollider.bounds.extents.sqrMagnitude) {//clamp the distance
            //Debug.Log("the click is too close to the player");
            return;
        }
        AudioManager.instance.play("throw");
        if (distance > 10)
            distance = 10;
        Transform player = playerBoundingCollider.transform;
        //Vector3 slashDir =-1* player.right* Mathf.Sign(clickDiffrence.x); 
        Vector3 slashDir = player.right;
        Vector2 A = player.up;
        Vector2 B = player.position - mousePosition;
        bool isleft = -A.x * B.y + A.y * B.x > 0;
        if(isleft)
            slashDir =-1* player.right;
        Vector3 slashPoint = playerBoundingCollider.bounds.center; 
        slashPoint += slashDir * playerBoundingCollider.bounds.extents.x;
        slashPoint += slashDir * 0.2f;
        Vector3 spawnPoint = slashPoint;
        //shoot object from the apropriate side of the character
        //spawnPoint.x += playerBoundingCollider.bounds.extents.x *Mathf.Sign(clickDiffrence.x); 


        Vector3 shootVelocity = clickDiffrence * bulletSpeed * distance / 10;
        //if (isLocalPlayer && !isServer) {//local instance that is not the server
        //    GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);
        //    bullet.GetComponent<Rigidbody2D>().velocity = shootVelocity;
        //    Destroy(bullet, 9.0f);
        //}
        CmdShoot(spawnPoint,shootVelocity);

    }

    private void Update() {
        if (spawnedBulletsTime.Count >= 1) {
            if(Time.time-spawnedBulletsTime[0] >= 9f) {//bullet expired
                if (spawnedBullets[0] != null) {
                    NetworkServer.Destroy(spawnedBullets[0]);

                    //Debug.Log("server removing bullet" + spawnedBulletsTime.Count + " "+ spawnedBullets.Count);
                }
                spawnedBullets.RemoveAt(0);
                spawnedBulletsTime.RemoveAt(0);
            }
        }
    }
    List<GameObject> spawnedBullets = new List<GameObject>();
    List<float> spawnedBulletsTime = new List<float>();
    [Command]
	void CmdShoot(Vector3 spawnPoint,Vector3 velocity) {
        

       // bulletSpawnPoint = spawnPoint;
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D> ().velocity = velocity;

        bullet.GetComponent<Bullet>().owner = gameObject.GetComponent<PlayerReceiveDamage>();
        bullet.GetComponent<Bullet>().ownerPD = gameObject.GetComponent<PlayerData>();

        //  bullet.transform.parent = transform;
        spawnedBullets.Add(bullet);
        spawnedBulletsTime.Add(Time.time);
        NetworkServer.Spawn (bullet);

        //Destroy(bullet, 9.0f);
        //RpcDeleteAfter(bullet, 9f);
        //RpcShoot2( spawnPoint, velocity);


    }

    /// <summary>
    /// spawns for everyone with the local player, because the latency in connection is annoying
    /// </summary>

    [ClientRpc] public void RpcShoot(GameObject bullet, Vector3 spawnPoint, Vector3 velocity) {
        if (isServer )
            return;
        //GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);
        bullet.transform.position = spawnPoint;
        bullet.GetComponent<Rigidbody2D>().velocity = velocity;

       // Destroy(bullet, 9.0f);
    }

    [ClientRpc]
    public void RpcShoot2( Vector3 spawnPoint, Vector3 velocity) {
        if (isServer)
            return;
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);
        bullet.transform.position = spawnPoint;
        bullet.GetComponent<Rigidbody2D>().velocity = velocity;

        // Destroy(bullet, 9.0f);
    }
    [ClientRpc]
    public void RpcDeleteAfter(GameObject bullet,float time) {
        if (isServer )
            return;

        Destroy(bullet,time);
    }


    private void OnDrawGizmos() {
        if(bulletSpawnPoint!=Vector3.zero)
            Gizmos.DrawSphere(bulletSpawnPoint, 0.3f);
    }
}

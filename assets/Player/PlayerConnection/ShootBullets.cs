/*this makes the player shoot bullets and remembers who is the one that shot them,
 * in the latest revision the bullets are shot according to where the mouse is clicked
 */
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
        if (distance > 10)
            distance = 10;


        Vector3 spawnPoint = playerBoundingCollider.bounds.center;
        //shoot object from the apropriate side of the character
        spawnPoint.x += playerBoundingCollider.bounds.extents.x *
                                            Mathf.Sign(clickDiffrence.x);

        Vector3 shootVelocity = clickDiffrence * bulletSpeed * distance / 10;

        CmdShoot(spawnPoint,shootVelocity);
    }
	[Command]
	void CmdShoot(Vector3 spawnPoint,Vector3 velocity) {
        

       // bulletSpawnPoint = spawnPoint;
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D> ().velocity = velocity;

        bullet.GetComponent<Bullet>().owner = gameObject.GetComponent<PlayerReceiveDamage>();
        bullet.GetComponent<Bullet>().ownerPD = gameObject.GetComponent<PlayerData>();

        //  bullet.transform.parent = transform;
        NetworkServer.Spawn (bullet);
        Destroy (bullet, 9.0f);
    }


    private void OnDrawGizmos() {
        if(bulletSpawnPoint!=Vector3.zero)
            Gizmos.DrawSphere(bulletSpawnPoint, 0.3f);
    }
}

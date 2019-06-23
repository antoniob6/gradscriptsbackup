/*
 * it spawns enemies on all the clients connected to the server
 * the location is randomly chosen based on the parameters
 */ 


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnEnemies : NetworkBehaviour {

	[SerializeField]
	private GameObject enemyPrefab;

	[SerializeField]
	private float spawnInterval = 1.0f;

	[SerializeField]
	private float enemySpeed = 1.0f;
    [SerializeField]
    private float spawnrange = 40f;




    //public override void OnStartServer () {
    void Start()
    {
    //    CancelInvoke();
        
	}
    private void OnEnable() {
        InvokeRepeating("SpawnEnemy", this.spawnInterval, this.spawnInterval);
    }
    private void OnDisable() {
        CancelInvoke();
        
    }

    List<GameObject> spawnedBullets = new List<GameObject>();
    List<float> spawnedBulletsTime = new List<float>();
    int spawnCount = 0;
    void SpawnEnemy() {
        if (!isActiveAndEnabled)
            CancelInvoke();
        if (spawnedBulletsTime.Count >= 1) {
            if (Time.time - spawnedBulletsTime[0] >= 9f || spawnedBulletsTime.Count>=30) {//bullet expired
                if (spawnedBullets[0] != null) {
                    NetworkServer.Destroy(spawnedBullets[0]);

                    //Debug.Log("server removing enemy" + spawnedBulletsTime.Count + " " + spawnedBullets.Count);
                }
                spawnedBullets.RemoveAt(0);
                spawnedBulletsTime.RemoveAt(0);
            }
        }

        //Random.InitState( System.DateTime.Now.Millisecond);
        MapManager MM = FindObjectOfType<MapManager>();
        if (!MM) {
            Debug.Log(" alien spawner cant find MapManger");
            return;
        }
        if (!MM.finishedCreatingPlatforms) {
            return;
        }
        
        if (MM.GM && MM.GM.devmode) {//spawn less enemies in dev mode
            if (spawnCount >= 4) {
                spawnCount = 0;
                return;
            }
        }
        spawnCount++;


        //spawnrange = MM.getRandomPosition().x;
        //Random.Range(-spawnrange, spawnrange)
        Vector3 randomSpawnPosition = MM.getRandomPosition();
        Vector2 spawnPosition = randomSpawnPosition+ GravitySystem.instance.getUpDirection(randomSpawnPosition) *13;
		GameObject enemy = Instantiate (enemyPrefab, spawnPosition, Quaternion.identity) as GameObject;

        //enemy.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0.0f, -this.enemySpeed);
        enemy.GetComponent<Rigidbody2D>().velocity =
            GravitySystem.instance.getUpDirection(randomSpawnPosition)*-enemySpeed;
        enemy.layer = 8;
        enemy.transform.parent = transform;
		NetworkServer.Spawn (enemy);
        spawnedBullets.Add(enemy);
        spawnedBulletsTime.Add(Time.time);
        //Destroy (enemy, 20);
    }

}
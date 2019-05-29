/*
 * it spawns enemies on all the clients connected to the server
 * the location is randomly chosen based on the parameters
 */ 

using System.Collections;
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

    void SpawnEnemy() {
        if (!isActiveAndEnabled)
            CancelInvoke();
        Random.InitState( System.DateTime.Now.Millisecond);
        MapManager MM = FindObjectOfType<MapManager>();
        if (!MM) {
            Debug.Log(" alien spawner cant find MapManger");
            return;
        }
        //spawnrange = MM.getRandomPosition().x;
        //Random.Range(-spawnrange, spawnrange)
        Vector3 randomSpawnPosition = MM.getRandomPosition();
        Vector2 spawnPosition = new Vector2 (randomSpawnPosition.x, randomSpawnPosition.y+10);
		GameObject enemy = Instantiate (enemyPrefab, spawnPosition, Quaternion.identity) as GameObject;
		enemy.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0.0f, -this.enemySpeed);
        enemy.layer = 8;
        enemy.transform.parent = transform;
		NetworkServer.Spawn (enemy);
		Destroy (enemy, 20);
	}

}
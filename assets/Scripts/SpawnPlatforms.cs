using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnPlatforms: NetworkBehaviour {

	[SerializeField]
	private GameObject platformPrefab;
    [SerializeField]
    private float mapWidth=100;
    [SerializeField]
    private float platformNumber = 10;

    public override void OnStartServer () {
        for(int i=0;i<platformNumber;i++)
         spawnMap();
	}

	void spawnMap() {
		Vector2 spawnPosition = new Vector2 (Random.Range(-mapWidth, mapWidth), this.transform.position.y);
		GameObject enemy = Instantiate (platformPrefab, spawnPosition, Quaternion.identity) as GameObject;
        Vector3 theScale = enemy.transform.localScale;
        theScale.x *= Random.Range(1, 10);
        enemy.transform.localScale = theScale;


        NetCorrector nc = enemy.GetComponent<NetCorrector>();
        nc.rotation = enemy.transform.rotation;
        nc.scale = enemy.transform.localScale;


        NetworkServer.Spawn (enemy);
	}

}
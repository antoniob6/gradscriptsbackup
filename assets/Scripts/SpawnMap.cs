using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnMap : NetworkBehaviour {

	[SerializeField]
	private GameObject mapPrefab;

	public override void OnStartServer () {
        spawnMap();
	}

	void spawnMap() {
		Vector2 spawnPosition = new Vector2 (this.transform.position.x, this.transform.position.y);
		GameObject enemy = Instantiate (mapPrefab, spawnPosition, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (enemy);
	}

}
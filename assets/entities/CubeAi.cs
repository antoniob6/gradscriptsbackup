using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeAi : MonoBehaviour {
    GameObject player;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player");

        AiRoutine();
		
	}
    void AiRoutine() {
        this.transform.position = Vector2.MoveTowards(transform.position, player.transform.position, 100 * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("have collided with something");
    }
}

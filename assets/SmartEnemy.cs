using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemy : MonoBehaviour {
    public float jumpInterval=3;
    public float jumpSpeed = 10;
    public GameManager GM;
    
	// Use this for initialization
	void Start () {
        GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (!GM) {
            Debug.Log("Couldn't find GameManager on smart enemy");
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable() {
        InvokeRepeating("jump", jumpInterval,jumpInterval);
    }
    private void OnDisable() {
        CancelInvoke();
    }

    void jump() {
        GetComponent<Rigidbody2D>().velocity = Vector2.up * jumpSpeed;
    }


}

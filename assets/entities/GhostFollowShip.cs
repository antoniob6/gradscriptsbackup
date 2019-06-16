using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostFollowShip : MonoBehaviour {

    [SerializeField] private float speed = 3f;

    [HideInInspector] public bool dead = false;


    private Rigidbody2D m_Rigidbody2D;


    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }



    GameObject target;
    // Use this for initialization
    void FixedUpdate() {
        if (dead)
            return;
        if (!target)
            target = findShip();
        if (target)
            tick();



    }

    private GameObject findShip() {
        GameObject ship = GameObject.FindGameObjectWithTag("Ship");
        if (!ship) {
            Debug.Log("ship killer couldn't find ship");
            return null;
        }
        return ship;
    }

    void tick() {
        Vector3 distanceVector = target.transform.position - transform.position;
        Vector3 distanceDirection = distanceVector.normalized;


        m_Rigidbody2D.velocity = distanceDirection * speed;


    }

}

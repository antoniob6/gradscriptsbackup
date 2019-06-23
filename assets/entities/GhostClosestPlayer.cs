/*
 * AI script that makes an entity acts as a ghost a player 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GhostClosestPlayer : NetworkBehaviour {
    public Animator animator;
    public Collider2D playerBoundingCollider;
    [SerializeField] private float speed = 3f;
    [SerializeField]private float focusTimePeriod =3.0f;
    [SerializeField] private float tickTimePeriod = 0f;
    [HideInInspector] public bool dead = false;



    const float k_GroundedRadius = .3f;
    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;

    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    private float focusTimeLeft;
    private float tickTimeLeft;
    // Use this for initialization
    void Start() {
        focusTimeLeft = focusTimePeriod;
    }







    GameObject target;
    bool inited = false;
	void Update () {
        if (dead)
            return;

        focusTimeLeft -= Time.deltaTime;

        if (focusTimeLeft <= 0f || !inited) {
            inited = true;
            focusTimeLeft = focusTimePeriod;
            target = findClosestTarget();
        }
        if (!target)
            return;


        tickTimeLeft -= Time.deltaTime;
        if (tickTimeLeft <= 0f) {
            if (target)
                tick();

            tickTimeLeft = tickTimePeriod;
        }


    }

    void tick() {
        Vector3 distanceVector = target.transform.position - transform.position;
        Vector3 distanceDirection = distanceVector.normalized;


        m_Rigidbody2D.velocity =distanceDirection*speed;


    }

    GameObject findClosestTarget() {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("PlayerObject");
        if (Players.Length <= 0) {
            Debug.Log("can't find any player");
            return null;
        }
        GameObject closestPlayer = Players[0];
        float closestDistance = Vector2.SqrMagnitude(transform.position - closestPlayer.transform.position);

        foreach(GameObject g in Players) {
            if (!g) {
                Debug.Log("skipping a null player");
                continue;
            }
            //if we find a closer player we change target
            float curDistance = Vector2.SqrMagnitude(transform.position - g.transform.position);
            if (curDistance< closestDistance) {
                closestDistance = curDistance;
                closestPlayer = g;
            }
        }
        return closestPlayer;
    }
}

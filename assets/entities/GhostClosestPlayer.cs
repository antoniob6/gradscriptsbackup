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
    [SerializeField] private float m_JumpForce = 400f;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    [SerializeField]private float focusTimePeriod =3.0f;

    [HideInInspector] public bool dead = false;



    const float k_GroundedRadius = .3f;
    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;

    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private float timeLeft;
    // Use this for initialization
    void Start() {
        timeLeft = focusTimePeriod;
    }






    GameObject target;
	void Update () {
        if (dead)
            return;
        if(!target)
            target= findClosestTarget();
        if(target)
            tick();


        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) {

            timeLeft = focusTimePeriod;
            target = findClosestTarget();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GuardRoutine : NetworkBehaviour
{
    public Animator animator;
    public Collider2D playerBoundingCollider;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float m_JumpForce = 400f;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    [SerializeField] private float focusTimePeriod = 3.0f;

    [HideInInspector] public bool dead = false;



    const float k_GroundedRadius = .3f;
    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;

    private Vector3 guardPoint;

    private float timeLeft;
    // Use this for initialization
    void Start() {
        timeLeft = focusTimePeriod;
        guardPoint = transform.position;
    }



    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    private bool m_Grounded;
    private void FixedUpdate() {
        if (dead)
            return;
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        Vector3 checkPoint = playerBoundingCollider.bounds.center;
        checkPoint.y -= playerBoundingCollider.bounds.extents.y;//find bottom center of object

        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPoint, k_GroundedRadius);
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].gameObject != gameObject) {//check if standing on something
                m_Grounded = true;
                if (animator && !wasGrounded)//just landed now
                    animator.SetTrigger("idle");
            }
        }
    }

    GameObject target;
    void Update() {
        if (dead)
            return;
        if (!target)
            target = findClosestTarget();
        if (target)
            tick();


        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) {

            timeLeft = focusTimePeriod;
            target = findClosestTarget();
        }
    }

    void tick() {
        float move = speed;
        if (target.transform.position.x < transform.position.x)//if the object is to the left
            move = -speed;

        Vector3 targetVelocity = (transform.right.normalized * move) + Vector3.Project((Vector3)(m_Rigidbody2D.velocity), transform.up);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

        if (m_Grounded && m_Rigidbody2D.velocity.x * m_Rigidbody2D.velocity.x < 0.2) {//we hit wall so jump
            //Debug.Log("jumping");
            if (animator)
                animator.SetTrigger("jump");
            m_Grounded = false;
            m_Rigidbody2D.AddForce(transform.up.normalized * m_JumpForce);
        }
    }

    GameObject findClosestTarget() {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("PlayerObject");
        if (Players.Length <= 0) {
            Debug.Log("can't find any player");
            return null;
        }
        GameObject closestPlayer = Players[0];
        float closestDistance = Vector2.SqrMagnitude(transform.position - closestPlayer.transform.position);

        foreach (GameObject g in Players) {
            if (!g) {
                Debug.Log("skipping a null player");
                continue;
            }
            //if we find a closer player we change target
            float curDistance = Vector2.SqrMagnitude(transform.position - g.transform.position);
            if (curDistance < closestDistance) {
                closestDistance = curDistance;
                closestPlayer = g;
            }
        }
        return closestPlayer;
    }
}

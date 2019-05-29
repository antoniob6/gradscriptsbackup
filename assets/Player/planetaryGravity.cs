/*
 * adds the gravity
 * not only planet like gravity but also downward and upward directed gravity
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetaryGravity : MonoBehaviour {
    private GameObject center;
    
    public bool isActive = true;
    private GravitySystem GS;


    private Rigidbody2D m_Rigidbody2D;
    // Use this for initialization
    void Start () {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();


    }
	
	// Update is called once per frame
	void Update () {
       
        if (isActive)
            if (center == null)
                 center = FindObjectOfType<GravitySystem>().gameObject;
        
		
	}
    void FixedUpdate()
    {

        Vector3 gravityDir= GravitySystem.instance.getGravityDirection(transform.position).normalized;
        float gravityForce = GravitySystem.instance.gravityForce;
        if (m_Rigidbody2D) {
            m_Rigidbody2D.AddForce(gravityDir* gravityForce);
        }

    }

}

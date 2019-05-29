using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {
    public float swordLength = 1f;
    public int swordDamage=2;
    public float knockBackForce=200;
    public GameObject sword;
    public GameObject slashingEffect;

    private bool active;

    private GameObject selecteObject;

    private Quaternion originalRotation;

    private PlayerConnectionObject PCO;
    private PlayerData PD;
    private void Start() {
        PCO = GetComponentInParent<PlayerConnectionObject>();
        PD= GetComponentInParent<PlayerData>();
        active = PCO.active;
        originalRotation = sword.transform.localRotation;

        if (active) {
            if (!PCO || !PD)
                Debug.Log("sword couldn't find it's own PCO/PD");
        }
    }
    private void Awake() {
        //Debug.Log("sword chosen");
    }


    private void Update() {
        if (!active)//only run on local player
            return;
        if (Input.GetMouseButtonDown(0)) {//only local player does this
            //Debug.Log("slashing sword");
            sword.transform.Rotate(new Vector3(0, 0, -90));
            slashSword();
        } else if (Input.GetMouseButtonUp(0)) {//return sword
            sword.transform.localRotation = originalRotation;
        }
    }

    private bool wasFacingRight = true;
    private void FixedUpdate() {

        //put the sword in the player hand
        Collider2D PBC = PCO.playerBoundingCollider;
        sword.transform.position = PBC.transform.position;
        sword.transform.position += PBC.transform.right* PBC.bounds.extents.x / 2;
        if (PCO.facingRight && !wasFacingRight ) {
            transform.localScale = Vector3.one;
            wasFacingRight = true;
        } else if(!PCO.facingRight && wasFacingRight) {
            transform.localScale = new Vector3(-1f,1f,1f);
            wasFacingRight = false;
        }

    }
    Vector3 hitCenterGizmoVector=Vector3.zero;


    private void slashSword() {//executed on local instance
        Transform player = PCO.playerBoundingCollider.transform;
        Vector3 slashDir = player.right;
        if (!PCO.facingRight)
            slashDir = player.right * -1f;

        Vector3 slashPoint = player.position;
        slashPoint+= slashDir * PCO.playerBoundingCollider.bounds.extents.x;
        slashPoint += slashDir * swordLength/2;

        hitCenterGizmoVector = slashPoint;
       Collider2D[] hits= Physics2D.OverlapCircleAll(slashPoint, swordLength);

        foreach(Collider2D c in hits) {
            Rigidbody2D RB = c.GetComponent<Rigidbody2D>();
            if (RB && c.gameObject != PCO.PC.gameObject) {
                RB.AddForce(player.right * knockBackForce);
               // Debug.Log(c.name);
            }
            
            EnemyRecieveDamage ERD = c.GetComponent<EnemyRecieveDamage>();
            if (ERD) {//hit enemy with ERD componenet
                //ERD.CmdTakeDamageWithGO(swordDamage,PD.gameObject);//cant because no autority
                PCO.relayERDAttack(ERD, swordDamage, PD);
            }
            PlayableCharacter PC = c.GetComponent<PlayableCharacter>();
            if (PC && PC.gameObject != PCO.PC.gameObject) {//hit another player
                PlayerGiveDamage PGD = PCO.GetComponent<PlayerGiveDamage>();
                if (PGD) {
                    PGD.giveDamageToPlayableCharacter(swordDamage, PC.PCO.gameObject);
                } else {
                    Debug.Log("can't find give damage");
                }
            }

        }

        if (slashingEffect) {
            GameObject GO= Instantiate(slashingEffect, slashPoint, Quaternion.identity);
            GO.transform.parent = this.transform;
        } else
            Debug.Log("slashingEffect not assigned");
    }
    private void OnDrawGizmos() {
        if (hitCenterGizmoVector != Vector3.zero)
            Gizmos.DrawSphere(hitCenterGizmoVector, swordLength);
        
    }

}

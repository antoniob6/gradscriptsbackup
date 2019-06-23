using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {
    public float swordLength = 1f;
    public int swordDamage=2;
    public float knockBackForce=200;
    public GameObject sword;
    public GameObject slashingEffect;
    public float coolDownTime = 0.3f;
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

    private float lastTpTime = 0f;
    private void Update() {
        if (!active)//only run on local player
            return;

        if (Input.GetMouseButtonDown(0)&& Time.time - lastTpTime >= coolDownTime) {//only local player does this
            //Debug.Log("slashing sword");
            lastTpTime = Time.time;
            AudioManager.instance.play("slashing");
            sword.transform.Rotate(new Vector3(0, 0, -90));
            slashSword();
        } else if (Input.GetMouseButtonUp(0)) {//return sword
            Collider2D PBC = PCO.playerBoundingCollider;
            if(PBC)
                sword.transform.localRotation = PBC.transform.rotation;
        } else if(!Input.GetMouseButton(0)) {//follow player when mouse isn't pressed
            Collider2D PBC = PCO.playerBoundingCollider;
            if(PBC)
                sword.transform.rotation = PBC.transform.rotation;
        }
    }

    private bool wasFacingRight = true;
    private void FixedUpdate() {

        //put the sword in the player hand
        Collider2D PBC = PCO.playerBoundingCollider;
        sword.transform.position = PBC.transform.position;
        sword.transform.position += PBC.transform.right* PBC.bounds.extents.x / 2;


        if (!wasFacingRight&& isMouseToTheRight()) {
            transform.localScale = Vector3.one;
            wasFacingRight = true;
        } else if( wasFacingRight&& !isMouseToTheRight()) {
            transform.localScale = new Vector3(-1f,1f,1f);
            wasFacingRight = false;
        }

    }
    Camera playerCamera;
    BoxCollider2D playerBoundingCollider;


public bool isMouseToTheRight() {
        if(!playerCamera)
            playerCamera = PCO.getPlayerCamera();
        if(!playerBoundingCollider)
            playerBoundingCollider=PCO.playerBoundingCollider;

        if (!playerCamera || !playerBoundingCollider) {
            Debug.Log("player character still not assigned");
            return true ;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = playerCamera.ScreenToWorldPoint(mousePosition);
        Vector2 clickDiffrence = mousePosition - playerBoundingCollider.transform.position;
        Transform player = playerBoundingCollider.transform;
        Vector3 slashDir = player.right;
        Vector2 A = player.up;
        Vector2 B = player.position - mousePosition;
        bool isleft = -A.x * B.y + A.y * B.x > 0;
        if (isleft)
            return false;

        return true;
    }
    Vector3 hitCenterGizmoVector=Vector3.zero;


    private void slashSword() {//executed on local instance
        Transform player = PCO.playerBoundingCollider.transform;
        Vector3 slashDir = player.right;
        if (!isMouseToTheRight())
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

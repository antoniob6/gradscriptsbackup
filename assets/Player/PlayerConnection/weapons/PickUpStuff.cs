/* this class was originally intended to give the players a mechanism to interact with each other
 * such as grabing, throwing, pushing, and pulling
 * but due to the game being multiplayer, and Unity forcing strict rules(server and client restrictions)
 * it was too complex to implement.
 * and so it was simplified to give the player the ability to teleport, with a cool down time.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpStuff : MonoBehaviour
{
    public RawImage coolDownRefernce;
    public float coolDownTime = 5f;
    //public float force = 5;


    private PlayerConnectionObject PCO;
    private bool active;//active means only local player controls it

    private GameObject selecteObject;
    private void Start() {
        PCO = GetComponentInParent<PlayerConnectionObject>();
        active = PCO.active;
        if (active) {
           // print("pick up stuff script active");
        }
    }
    private float lastTpTime = 0f;

    bool teleportActivated = false;
    private void Update() {
        if (!active)
            return;
        bool canTeleport = Time.time-lastTpTime>=coolDownTime;

        if(!teleportActivated && canTeleport) {//deactivate the visual effects(red color)
            teleportActivated = true;
            if (coolDownRefernce)
                coolDownRefernce.gameObject.SetActive(false);

            AudioManager.instance.play("teleportActive");
        }

        if (canTeleport && Input.GetMouseButtonDown(0)) {


            RaycastHit hit;
            Ray ray = PCO.getPlayerCamera().ScreenPointToRay(Input.mousePosition);
            //Debug.Log("trying to identify");
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);//test against 2d objects
            bool clickOnSomething = false;
            if (hit2D.collider != null) {
                clickOnSomething = true;
                return;
            }
            if (Physics.Raycast(ray, out hit, 100.0f)) {//test against 3d objects
                if (hit.transform != null) {
                    clickOnSomething = true;
                }
            }
            if (clickOnSomething) {//cant teleport because clicked on something
                AudioManager.instance.play("teleportBlocked");
            }

            if (!clickOnSomething) {//we clicked on empty space you can teleport
                teleportActivated = false;
                lastTpTime = Time.time;
                if (coolDownRefernce)
                    coolDownRefernce.gameObject.SetActive(true); //activate cool down visuals
                AudioManager.instance.play("teleporting");

                Invoke("teleport", 0.3f);

            }
        } 
    }

    private void teleport() {
        SpawnManager.instance.spawnObjOnLocalInstance("dustPuff", PCO.PC.transform.position);
        Vector3 mousePosition =
        PCO.getPlayerCamera().ScreenToWorldPoint(Input.mousePosition);
        PCO.PC.transform.position = new Vector3(mousePosition.x, mousePosition.y);



    }

    /*
    private void Update() {
        if (active && Input.GetMouseButtonDown(0) ) {
            RaycastHit hit;
            Ray ray = PCO.getPlayerCamera().ScreenPointToRay(Input.mousePosition);
            //Debug.Log("trying to identify");
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

            bool clickOnSomething = false;
            if (hit2D.collider != null) {
                clickOnSomething = true;
              //  Debug.Log("clicked on something 2d:"+ hit2D.collider.name);
                selecteObject = hit2D.collider.gameObject;
                return;
            }

            if (Physics.Raycast(ray, out hit, 100.0f)) {
                
                if (hit.transform != null) {
                    clickOnSomething = true;
                    //  Debug.Log("clicked on something 3d:"+ hit.transform.gameObject.name);
                    selecteObject = hit.transform.gameObject;

                }
            }
            if (!clickOnSomething) {//we clicked on empty space
                if (selecteObject) {// there is an object selected

                    Vector3 mousePosition =
                            PCO.getPlayerCamera().ScreenToWorldPoint(Input.mousePosition);

                    selecteObject.transform.position =
                        new Vector3(mousePosition.x, mousePosition.y, 0);
                }

            }
        }
        else if (active && Input.GetMouseButtonDown(0)) {
            Debug.Log("trying to identify");
            Collider2D clickedObj= Physics2D.OverlapCircle(Input.mousePosition, 5f);
            if (clickedObj) {
                selecteObject = clickedObj.gameObject;
                Debug.Log("clicked on something:" + clickedObj.name);

            }

            
        }
    }
    */










}

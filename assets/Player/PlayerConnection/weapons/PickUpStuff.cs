using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PickUpStuff : MonoBehaviour
{


    public float force = 5;


    private PlayerConnectionObject PCO;
    private bool active;

    private GameObject selecteObject;
    private void Start() {
        PCO = GetComponentInParent<PlayerConnectionObject>();
        active = PCO.active;
        if (active) {
           // print("pick up stuff script active");
        }
    }

    private void Update() {
        if (active && Input.GetMouseButtonDown(0)) {
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

            if (!clickOnSomething) {//we clicked on empty space you can teleport
                    Vector3 mousePosition =
                            PCO.getPlayerCamera().ScreenToWorldPoint(Input.mousePosition);
                    PCO.PC.transform.position = new Vector3(mousePosition.x, mousePosition.y);
                

            }
        } 
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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnStuff : MonoBehaviour
{




    private int index = 0;

    private PlayerConnectionObject PCO;
    private bool active;


    private void Start() {
        PCO = GetComponentInParent<PlayerConnectionObject>();
        active = PCO.active;
      
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

                PCO.CmdSpawnBoxOnPosition (new Vector3(mousePosition.x, mousePosition.y));


            }
        }
    }
}

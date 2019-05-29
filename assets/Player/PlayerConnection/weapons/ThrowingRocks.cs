using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingRocks : MonoBehaviour
{

    private PlayerConnectionObject parentCO;
    private bool active;
    private void Start() {
        parentCO = GetComponentInParent<PlayerConnectionObject>();
        active = parentCO.active;
        if (active) {
          //  print("script active");
        }
    }

    private void Update() {
        if (active && Input.GetMouseButtonDown(0)) {
            AudioManager.instance.play("throw");
            parentCO.GetComponent<ShootBullets>().Shoot();
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingRocks : MonoBehaviour
{
    public float coolDownTime = 0.3f;
    private PlayerConnectionObject parentCO;
    private bool active;
    private void Start() {
        parentCO = GetComponentInParent<PlayerConnectionObject>();
        active = parentCO.active;
        if (active) {
          //  print("script active");
        }
    }
    private float lastTpTime = 0f;
    private void Update() {
        if (active && Input.GetMouseButtonDown(0)&& Time.time - lastTpTime >= coolDownTime) {
            lastTpTime = Time.time;
            //AudioManager.instance.play("throw");
            parentCO.GetComponent<ShootBullets>().Shoot();
        }

    }
}

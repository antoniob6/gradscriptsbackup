using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveParent : MonoBehaviour {
    Transform parent;
	void Start () {
        parent = transform.parent;
        transform.SetParent(null);
	}
    private void Update() {
  
        if (parent) {
            transform.position = parent.position;

            if (GravitySystem.instance.isReverseGravity) {
                transform.eulerAngles = parent.rotation.eulerAngles + Vector3.forward * 180;
                //Debug.Log("rotating towards the reverse");
            } else
                transform.rotation = parent.rotation;

        } else {
            Destroy(gameObject);
        }
    }

}

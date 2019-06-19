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
            transform.rotation = parent.rotation;

        } else {
            Destroy(gameObject);
        }
    }

}

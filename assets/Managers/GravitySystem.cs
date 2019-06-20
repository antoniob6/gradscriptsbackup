/* gravity system that guides other objects to the gravity
 * examples:1)  on the server it tells the other objects where is the up direction 
 *              so that it knows where to put stuff (instead of under the map for example)
 *          2)  on the client it tells each object where does the gravity points to    
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GravitySystem :NetworkBehaviour {
    public enum GravityType
    {
        Down, Up, ToCenter, ToOut
    }
    [SyncVar] public GravityType gravityType = GravityType.Down;
    [SyncVar] public float gravityForce = 30f;
    [SyncVar] public float runningSpeed = 30f;
    [SyncVar] public float jumpHeight = 1.5f;
    [SyncVar] public bool isReverseGravity = false;


    [HideInInspector]
    public static GravitySystem instance;
    void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }





    public Vector3 getUpDirection(Vector3 position) {
        if (gravityType == GravityType.ToOut) {
            return (position - transform.position).normalized;
            //return (transform.position - position).normalized;
    }
        else if (gravityType == GravityType.ToCenter)
            return (position - transform.position).normalized;

        else if (gravityType == GravityType.Up)
            return Vector3.up;

        else
            return Vector3.up;

    }

    public Vector3 getGravityDirection(Vector3 position) {
        if (gravityType == GravityType.ToCenter)
            return transform.position - position;

        else if (gravityType == GravityType.ToOut) {
            return transform.position - position;
            //return position - transform.position;
        } else if (gravityType == GravityType.Up)
            return Vector3.down;

        else
            return Vector3.down;

    }
    public bool didObjFallOutside(Vector3 position) {
        //Debug.Log("cheking");
        if (gravityType == GravityType.Down|| gravityType == GravityType.Up) {
            if (Mathf.Abs(position.y) >= 250) 
                return true;
            return false;

        } else {
            if (position.sqrMagnitude <= 600 || position.sqrMagnitude >= 500000) {
                //Debug.Log("returning true");
                return true;
            }
            //Debug.Log("returning false: "+ position.sqrMagnitude);
            return false;
        }

    }

    public bool isCircularMap() {
        if (gravityType == GravityType.ToCenter || gravityType == GravityType.ToOut)
            return true;
        return false;
    }
}

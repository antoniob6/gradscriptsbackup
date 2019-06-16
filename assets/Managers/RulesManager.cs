using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RulesManager : NetworkBehaviour {


    [SyncVar]public Rules currentRules;


    [HideInInspector]
    public static RulesManager instance;

    void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public Rules getRules() {
        return currentRules;
    }
    public void setRules(Rules newRules) {
        currentRules = newRules;

        GravitySystem.instance.gravityForce = newRules.gravityForce;
        GravitySystem.instance.gravityType = newRules.resolveGravityType();
 
    }
    [ClientRpc]public void RpcSetRules(Rules newRules) {
        currentRules = newRules;

        GravitySystem.instance.gravityForce = newRules.gravityForce;
        GravitySystem.instance.gravityType = newRules.resolveGravityType();

    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

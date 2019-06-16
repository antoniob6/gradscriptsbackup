using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerGiveDamage : NetworkBehaviour {

    public void giveDamageToPlayableCharacter(int amount, GameObject damageReceiver) {
        if (!isLocalPlayer)
            Debug.Log("non local player giving damage to: "+damageReceiver.name);


        CmdGiveDamage(amount, damageReceiver);
    }
    [Command] public void CmdGiveDamage(int amount,GameObject damageReceiver) {
        PlayerReceiveDamage PRD = damageReceiver.GetComponent<PlayerReceiveDamage>();
        if (!PRD) {
            Debug.Log("couldn't found slashed object");
            return;
        }
        Debug.Log("success in slashing player");
        PRD.lastHitby = gameObject;
        PRD.TakeDamage(amount);
    }
}

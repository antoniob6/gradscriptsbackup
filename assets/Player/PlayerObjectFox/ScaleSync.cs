/*this class syncs the scale from the localPlayer to the rest of the clients
 * 
 * algorithem: sense the change in scale ---> tell server to change ---
 *                  ---->tell other clients to change.
 * 
 * the local player doesn't update the scale with the other clients because 
 * they are the ones choosing the change.
 * */
using UnityEngine;
using UnityEngine.Networking;

public class ScaleSync : NetworkBehaviour {

    private Vector3 oldScale;
	// Use this for initialization
	void Start () {
        oldScale = transform.localScale;
	}

	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return;
        if (oldScale != transform.localScale)
            onScaleChange();
	}

    private void onScaleChange() {
        CmdChangeScale(transform.localScale);
    }

    [Command]private void CmdChangeScale(Vector3 newScale) {
        oldScale = newScale;
        transform.localScale = newScale;
        RpcChangeScale(newScale);
    }

    [ClientRpc]private void RpcChangeScale(Vector3 newScale) {
        if (isLocalPlayer)//local player is the one who reqested the change
            return;
        oldScale = newScale;
        transform.localScale = newScale;
    }
}

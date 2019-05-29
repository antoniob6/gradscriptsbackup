using UnityEngine;
using UnityEngine.Networking;
public class myNetworkManager : NetworkManager
{
    private int playerindex = 0;
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
         
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    //    if(playerindex==0)
    //       player.GetComponent<PlayerData>().spriteColor = Color.red;
    //    else
   //         player.GetComponent<PlayerData>().spriteColor = Color.blue;
        playerindex++;
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
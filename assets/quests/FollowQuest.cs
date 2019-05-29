/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class FollowQuest:Quest{


    private float threshold=5f;

    private GameObject PlayerToTouch;


    public FollowQuest(List<GameObject> _players, GameManager _GM):base(){
        players = _players;
        GM = _GM;
        reward = Random.Range(50, 500);

        int randIndex = Random.Range(0, players.Count);
        PlayerToTouch = _players[randIndex];
        if (!PlayerToTouch) {
            Debug.Log("player to touch couldn't be found");
            questCompleted();
            return;
        }
        PlayerData PD = PlayerToTouch.GetComponent<PlayerData>();
        if (!PD) {
            Debug.Log("couldn't find PD componenet");
            questCompleted();
            return;
        }
        questMessage = "touch player \""+PD.playerName+"\""; 


        updateQuestMessage();

    }


    public override void tick() {
        if (isComplete)
            return;

        base.tick();


        foreach (GameObject p in players)
        {
            if (!p)
                continue;
            if (p == PlayerToTouch)
                continue;
            BoxCollider2D PBCToTouch = PlayerToTouch.GetComponent<PlayerConnectionObject>().playerBoundingCollider;
            BoxCollider2D PBC = p.GetComponent<PlayerConnectionObject>().playerBoundingCollider;
            if (!PBC)
                continue;


            if (Vector3.SqrMagnitude(PBCToTouch.transform.position- PBC.transform.position) < threshold * threshold)
            {
                Debug.Log("player has touched the target");
                winners.Add(p);
                questCompleted();

            }
            
        }
    }





}

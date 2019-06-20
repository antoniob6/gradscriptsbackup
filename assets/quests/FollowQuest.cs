/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class FollowQuest:Quest{


    private float threshold=5f;

    private GameObject PCOToTouchObj;
    private string playerToTouchName;

    public FollowQuest(List<GameObject> _players, GameManager _GM):base(){
        players = _players;
        GM = _GM;
        reward = Random.Range(50, 500);

        int randIndex = Random.Range(0, players.Count);
        PCOToTouchObj = _players[randIndex];
        if (!PCOToTouchObj) {
            Debug.Log("player to touch couldn't be found");
            questCompleted();
            return;
        }

        PlayerData PD = PCOToTouchObj.GetComponent<PlayerData>();
        if (!PD) {
            Debug.Log("couldn't find PD componenet");
            questCompleted();
            return;
        }
        playerToTouchName = PD.playerName;
        questMessage = "touch \""+PD.playerName+"\""; 


        updateQuestMessage();

    }

    int touchesCount = 0;
    bool shouldEnd = false;
    float endTime = 0f;
    public override void tick() {
        if (isComplete)
            return;

        base.tick();
        if (shouldEnd && Time.time > endTime) {
            questCompleted();
        }

        foreach (GameObject p in players)
        {
            if (!p)
                continue;
            if (p == PCOToTouchObj)
                continue;
            BoxCollider2D PBCToTouch = PCOToTouchObj.GetComponent<PlayerConnectionObject>().playerBoundingCollider;
            BoxCollider2D PBC = p.GetComponent<PlayerConnectionObject>().playerBoundingCollider;
            if (!PBC)
                continue;


            if (Vector3.SqrMagnitude(PBCToTouch.transform.position- PBC.transform.position) < threshold * threshold)
            {
                touchesCount++;
                //Debug.Log("player has touched the target");
                winners.Add(p);
                //questCompleted();
                if (!shouldEnd) {//give extra few seconds to touchers
                    shouldEnd = true;
                    endTime = Time.time + 3f;
                }


            }
            
        }
    }

    public override string getMessage(PlayerData PD = null) {
        if (PD == null)
            return base.getMessage(PD);
        if (didPlayerWin())
            return STRWAITWON;
        if (PD.gameObject == PCOToTouchObj) {
            if (didPlayerLose(PD))
                return STRWAITFAILED;

            return "don't get touched by players";

        }
        return "touch \"" + playerToTouchName + "\""; ;
    }
    public override bool didPlayerWin(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerWin();
        if (winners.Contains(PD.gameObject))
            return true;
        return false;

    }
    public override bool didPlayerLose(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerWin();
        if (PD.gameObject == PCOToTouchObj && touchesCount>0)
            return true ;
        return false;

    }







}

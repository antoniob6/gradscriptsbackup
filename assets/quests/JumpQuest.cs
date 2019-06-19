/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class JumpQuest : Quest
{
    int jumpCount = 10;


    public JumpQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;

        jumpCount = Random.Range(3, 20);
        reward = jumpCount * 20;


        questMessage = "be the first to jump "+jumpCount+" times";


        updateQuestMessage();

    }
    public override void init() {
        base.init();

    }

    int playersFinishedCount = 0;
    public override void tick() {
        if (isComplete)
            return;
        base.tick();

        if (playersFinishedCount == players.Count) {
            questCompleted();
        }

        foreach (GameObject p in players) {
            if (!p) {
                Debug.Log("found null player");
                continue;
            }
            PlayerData PD = p.GetComponent<PlayerData>();
            if (!PD) {
                Debug.Log("PCO not found");
                continue;
            }

            if (PD.roundJumpCount >= jumpCount) {
                if (winners.IndexOf(p) < 0) {
                    winners.Add(p);
                    playersFinishedCount++;
                    questCompleted();
                    //PD.RpcUpdateText("you have finished the quest waiting for other players");
                }
            }
        }

        if (isComplete)
            return;

    }

    public override string getMessage(PlayerData PD =null) {
        if(PD == null)
            return base.getMessage(PD);
        if(jumpCount - PD.roundJumpCount>0)
            return "be the first to jump " + (jumpCount -PD.roundJumpCount) + " times";
        return "be the first to jump " + 0 + " times";
    }


    public override bool didPlayerWin(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerWin();
        if (jumpCount - PD.roundJumpCount <= 0)
            return true;
        return false;

    }


    public override void DestroyQuest() {
       
    }




}

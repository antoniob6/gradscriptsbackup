/*
 *a kill quest that inhertes from the quest super class
 * that monitors the players, and when a player kills another first, 
 * he/she wins
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillQuest : Quest {



    private int KillLimit = 0;
    public KillQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;
        //reward = Random.Range(50, 500);
        KillLimit = Random.Range(1, 20);
        reward = KillLimit * 50;
        //killPlayers = Random.Range(0, 2) == 1 ? true : false;
        questMessage = "kill " + KillLimit + " things";

        updateQuestMessage();





    }
    public override void init() {
        base.init();


    }


    public override void tick() {

        if (isComplete)
            return;
        base.tick();


        if (winners.Count==players.Count) {
            questCompleted();
        }



        foreach (GameObject p in players) {
            if (!p)
                continue;
            int killedCount = p.GetComponent<PlayerData>().roundKilledPlayerCount;
            killedCount += p.GetComponent<PlayerData>().roundKilledEntityCount;
            if (killedCount >= KillLimit) {
                winners.Add(p);
                //p.GetComponent<PlayerData>().RpcUpdateText("you completed the quest, waiting for others to finish");
            }

        }
    }

    public override string getMessage(PlayerData PD = null) {
        if (PD == null)
            return base.getMessage(PD);
        if(KillLimit - PD.roundKilledEntityCount - PD.roundKilledPlayerCount>0)
            return questMessage = "kill " + (KillLimit- PD.roundKilledEntityCount- PD.roundKilledPlayerCount) + " things";
        return questMessage = "kill " + 0 + " things";
    }
    public override bool didPlayerWin(PlayerData PD=null) {
        if (PD == null)
            return base.didPlayerWin();
        if (KillLimit - PD.roundKilledEntityCount - PD.roundKilledPlayerCount <= 0)
            return true;
        return false;

    }



    public override void DestroyQuest() {
  

    }
}

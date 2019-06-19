/*
 *a boss quest that inhertes from the quest super class
 * that monitors the players, and if the boss is killed withing the time limit
 * the players get the reward
 * */
using System.Collections.Generic;
using UnityEngine;

public class BossQuest:Quest{

    private GameObject center;

    private float threshold;



    private GameObject bossGO;



    public BossQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;



        reward = Random.Range(150, 1000);

        questMessage = "defeat the boss";
        

        updateQuestMessage();
        if(GM.MM)
            GM.MM.createNewMapBaseOnly();

    }

    public override void init() {
        base.init();
        Vector3 spawnPosition = GM.MM.getRandomPositionAboveMap();
        bossGO = GM.networkSpawn("bossPrefab", spawnPosition);
    }
    public override void tick() {
        if (isComplete)
            return;
        base.tick();


        if (!bossGO) {
            foreach(GameObject p in players) {
                winners.Add(p);
            }
            if(!isComplete)
               questCompleted();  
        }


    }
    public override void DestroyQuest() {
       // Debug.Log("destroying the object");
        GM.networkDestroy(bossGO);

    }


}

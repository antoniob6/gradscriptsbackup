/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class GuardQuest : Quest
{


    private float threshold;

    private Vector3 spawnPosition;
    private float spawnrange;
    private GameObject foundable;
    private float spawnEvery=1;

    public GuardQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;

        spawnEvery=(float) players.Count;
        if (spawnEvery < 1f)
            spawnEvery = 1f;

        reward =(int) timeLeft*20;
        //Debug.Log("timeLeft: " + timeLeft);

        questMessage = "guard the ship";
        spawnPosition = GM.MM.getRandomPositionAboveMap();
        foundable = GM.networkSpawn("shipPrefab", spawnPosition);

        updateQuestMessage();

    }
    public override void init() {
        base.init();
        
    }
    private float lastSpawnTime = 0f;


    public override void tick() {
        base.tick();



        if (isComplete)
            return;


        if (!foundable) {
            if (!isComplete)
                questCompleted();
        }

        if (Time.time - lastSpawnTime <= spawnEvery) {
            
            return;
        }
        lastSpawnTime = Time.time;

        Vector3 updir= GravitySystem.instance.getUpDirection(foundable.transform.position) ;
        float randRight =100f * Random.Range(-1f, 1f);
        Vector3 crossingVector = new Vector3(updir.y, updir.x)*randRight;
        updir = updir * 40;
        updir = updir + crossingVector+ foundable.transform.position;

        //Debug.Log("spawning shipkiller: " + updir);
        GM.networkSpawn("shipKillerPrefab", updir);

    }

    public override void questCompleted() {
        if (foundable) {
            winners.AddRange(players);
        } else {
            winners.Clear();
        }
        base.questCompleted();
    }
    public override void DestroyQuest() {
        GM.networkDestroy(foundable);
    }




}

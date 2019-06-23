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
    public List<GameObject> enemies = new List<GameObject>();
    public GuardQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;

        spawnEvery=(float) players.Count;
        if (spawnEvery < 1f)
            spawnEvery = 1f;

        reward =(int) timeLeft*20;
        //Debug.Log("timeLeft: " + timeLeft);

        questMessage = "guard the ship";

        updateQuestMessage();
        if (GM.MM) {
            spawnPosition = GM.MM.getRandomPositionAboveMap();
            if(new System.Random().NextDouble() <=0.2)
                GM.MM.createNewMapPlatformsOnly();
            else
                GM.MM.createNewMapBaseOnly();
        }


        foundable = GM.networkSpawn("shipPrefab", spawnPosition);


    }
    public override void init() {
        base.init();

        if (!foundable) {
            if (GM.MM) {
                spawnPosition = GM.MM.getRandomPositionAboveMap();
                if (new System.Random().NextDouble() <= 0.2)
                    GM.MM.createNewMapPlatformsOnly();
                else
                    GM.MM.createNewMapBaseOnly();
            }


            foundable = GM.networkSpawn("shipPrefab", spawnPosition);
        }
        
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

        if (Time.time - lastSpawnTime >=4/ spawnEvery) {
            lastSpawnTime = Time.time;

            Vector3 updir = GravitySystem.instance.getUpDirection(foundable.transform.position);
            float randRight = 40f * Random.Range(-1f, 1f);
            Vector3 crossingVector = new Vector3(-updir.y, updir.x) * randRight;
            updir = updir * 25;
            updir = updir + crossingVector + foundable.transform.position;

            //Debug.Log("spawning shipkiller: " + updir);
            GameObject GO = GM.networkSpawn("shipKillerPrefab", updir);
            enemies.Add(GO);

        }
       

    }

    public override void questCompleted() {
        if (foundable) {
            winners.AddRange(players);
        } else {
            winners.Clear();
        }
        base.questCompleted();
    }

    public override bool didPlayerWin(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerWin();

        return winners.Contains(PD.gameObject);
    }
    public override void DestroyQuest() {
        GM.networkDestroy(foundable);
        foreach(GameObject enemy in enemies) {
            if(enemy)
                GM.networkDestroy(enemy);
        }
    }




}

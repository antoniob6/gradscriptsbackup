/*
 *a survive quest that inhertes from the quest super class
 * that monitors the players, and makes sure that when they die,
 * they don't win the quest
 * */



using System.Collections.Generic;
using UnityEngine;

public class SurviveQuest : Quest {

    private GameObject center;
    private Vector3 spawnPosition;
    private float spawnrange;
    private List<GameObject> enemies;


    private string originalQM;
    private int oldTime;

    public SurviveQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;
        reward = Random.Range(50, 500);
        reward =(int) timeLeft * 5;
        questMessage = "stay alive";
        //GM.startSpawingEnemies();
        updateQuestMessage();

    }
    public override void init() {
        base.init();

        //GM.startSpawingEnemies();
    }
    public override void tick() {
        if (isComplete)
            return;
        base.tick();
        bool allDied = true;
        foreach (GameObject p in players) {
            if (!p) {
                continue;
            }
            PlayerConnectionObject pco = p.GetComponent<PlayerConnectionObject>();
            if (!pco) {
                continue;
            }
            PlayerData pd = p.GetComponent<PlayerData>();

            if (pd.roundDeathCount <= 0) {
                allDied = false;
            }
        }
        if (allDied)
            questCompleted();
    }



    public override void questCompleted() {
        foreach (GameObject p in players) {
            if (!p) {
                Debug.Log("null player found");
                continue;
            }
            PlayerConnectionObject pco = p.GetComponent<PlayerConnectionObject>();
            if (!pco) {
                Debug.Log("pco not found");
                continue;
            }
            PlayerData pd = p.GetComponent<PlayerData>();//pco exists so this should

            if (pd.roundDeathCount <= 0) {
                winners.Add(p);
            }
        }
        

        base.questCompleted();
    }
    public override string getMessage(PlayerData PD = null) {
        if (PD == null)
            return base.getMessage();
        if(didPlayerLose())
            return STRWAITFAILED;

        return "stay alive";
    }


    public override bool didPlayerLose(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerLose(PD);
        if (PD.roundDeathCount>0)
            return true;
        return false;
    }

    public override void DestroyQuest() {
        //GM.stopSpawingEnemies();

    }





}

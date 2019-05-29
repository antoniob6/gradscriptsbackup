/*
 * this is the quest super class, quests inherets from this class.
 * 
 * */ 
using System.Collections.Generic;
using UnityEngine;


public class Quest{
    public int questType;
    public bool isComplete=false;
    public int reward=100;
    public string questMessage;
    public List<GameObject> winners = new List<GameObject>();
    public bool linkedQuest=false;
    public List<GameObject> players;

    public float timeLeft = 10f;

    

    public GameManager GM;

    public Quest() {//called once when quest is created
        setTimeLimit();

    }
    private int OldTime;


    public virtual void init() {//called once after players are ready
        //Debug.Log("initializing quest on Quest BASE type");
        foreach (GameObject p in players) {
            if (p)
                p.GetComponent<PlayerData>().resetRoundStats();
        }
    }
    bool initd = false;
    public virtual void tick() {//called every frame after players are ready
        if(isComplete)
            return;
        if (linkedQuest)
            return;

        if (!initd) {//calls the init only one
            init();
            initd = true;
        }



        if (timeLeft <= 0f ) {//updates the quest time every second
            questCompleted();
        } else {
            timeLeft -= Time.deltaTime;
            if (OldTime != (int)timeLeft) {
                updateQuestMessage();
                OldTime = (int)timeLeft;
            }
        }

    }

    public virtual void DestroyQuest() { }

    public virtual void updateQuestMessage() {
        if (linkedQuest)
            return;
        string newQuestMessage = questMessage;
        newQuestMessage += " (timeleft: " + (int)timeLeft + ", reward: "+reward+")";
        
        foreach( GameObject p in players) {
            PlayerData pd = p.GetComponent<PlayerData>();
            if(pd!=null)
                pd.RpcUpdateText(newQuestMessage);
        }
    }
    public virtual void RewardPlayers() {
        foreach (GameObject p in winners) {
            PlayerData pd = p.GetComponent<PlayerData>();
            if (pd != null) {
                pd.RpcAddScore(reward);
                
            }
        }

    }
    public virtual void questCompleted() {
        //Debug.Log("quest has been completed");
        if (isComplete)
            return;
        isComplete = true;

        if (linkedQuest)
            return;


         GM.questCompleted(this);


        foreach (GameObject p in players) {
            if (!p)
                return;
            bool playerWon = false;
            foreach (GameObject w in winners) {
                if (w == p)
                    playerWon = true;  
            }
            if(playerWon)
                p.GetComponent<PlayerData>().RpcUpdateText("you have won this round");
            else
                p.GetComponent<PlayerData>().RpcUpdateText("you have lost this round");
        }

        RewardPlayers();
        DestroyQuest();
    }
    public List<GameObject> getPlayers() {
        return players;
    }
    public string getMessage() {
        return questMessage;
    }

    public void setTimeLimit() {//set high propability for medium time ...
        float rand = Random.Range(0f, 1f);
        float selectedTime = 10f;
        if (rand < 0.2f) {
            selectedTime = Random.Range(10f, 20f);
        }else if (rand < 0.8f) {
            selectedTime = Random.Range(30f, 60f);
        } else {
            selectedTime = Random.Range(60f, 120f);
        }

        timeLeft = selectedTime;
    }


}

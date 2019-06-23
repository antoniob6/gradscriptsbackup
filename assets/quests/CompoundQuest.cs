/*
 *a compound quest that inhertes from the quest super class
 * that combines multiple quest into one big quest
 * */
using System.Collections.Generic;
using UnityEngine;

public class CompoundQuest:Quest{

    private Quest quest1;

    private Quest quest2;
    private Quest quest3;
    public CompoundQuest (List<GameObject> _players, GameManager _GM) : base() {
        reward = Random.Range(50, 500);

        GM = _GM;
        players = _players;
        quest1 = _GM.QM.createRandomQuest(_players, _GM,false);
        QuestManager.QuestTypes[] usedTypes = new QuestManager.QuestTypes[2];
        usedTypes[0] = _GM.QM.getQuestType(quest1);
        quest2 = _GM.QM.createRandomQuest(_players, _GM,false,usedTypes);
        usedTypes[1] = _GM.QM.getQuestType(quest2);
        quest3 = _GM.QM.createRandomQuest(_players, _GM,false, usedTypes);



        if (quest1 == null || quest2 == null||quest3==null) {
            Debug.Log("creating compound quest failed");
            return;
        }

        quest1.linkedQuest = true;
        quest2.linkedQuest = true;
        quest3.linkedQuest = true;



       // reward = quest1.reward + quest2.reward;
        updateQuestMessage();

    }

    public override void init() {
        //Debug.Log("created and quest");
        quest1.init();
        quest2.init();
        quest3.init();
    }
    public override void tick() {
        if (isComplete)
            return;
        base.tick();

        quest1.tick();
        quest2.tick();
        quest3.tick();
        bool shouldEnd = false;
        foreach (GameObject p in players) {//the purpose of this loop is to wait for all players to finish
            PlayerData pd = p.GetComponent<PlayerData>();
            if (pd != null) {//end quest if every one completed 
                if(quest3.didPlayerWin(pd)){
                    continue;
                }
                //here quest 3 can still be used
                if(quest1.didPlayerWin(pd) && quest2.didPlayerWin(pd)) {
                    continue;
                }
                //here means a player still didn't winning, checking for loss
                if (quest1.didPlayerLose(pd)||quest2.didPlayerLose(pd)) {
                    if(quest3.didPlayerLose(pd))
                        continue;//lost all hope of winning
                    //still have chance at quest 3
                }
                shouldEnd = false;
             }
        }
        if (shouldEnd) {
            questCompleted();
        }

        if (quest3.isComplete) {
            //Debug.Log("OR quest of compound quest completed");
            reward = quest3.reward;
            winners = quest3.winners;
            //winners.AddRange(quest2.winners);
            questCompleted();
            return;
        }
        if (quest1.isComplete&& quest2.isComplete) {
            //Debug.Log("both AND quests of compound quest completed");
            reward = quest1.reward + quest2.reward;
            winners = quest1.winners;
            foreach (GameObject w in quest1.winners) {
                if (quest2.winners.IndexOf(w) != -1) {//found someone who completed both quests
                    winners.Add(w);
                }
            }

            questCompleted();
            return;
        } else if (quest1.isComplete) {
            if (questMessage != quest2.getMessage()) {
                questMessage = quest2.getMessage()+" OR "+quest3.getMessage();
            }
        } else if (quest2.isComplete) {
            if (questMessage != quest1.getMessage()) {
                questMessage = quest1.getMessage() + " OR " + quest3.getMessage();

            }
        }





    }
    public override void RewardPlayers() {
        foreach (GameObject p in players) {
            PlayerData pd = p.GetComponent<PlayerData>();
            if (pd != null) {
                if (quest3.didPlayerWin(pd)) {
                    pd.RpcAddScore(quest3.reward);
                    continue;
                }
                if(quest1.didPlayerWin(pd)&& quest2.didPlayerWin(pd)) {
                    pd.RpcAddScore(quest1.reward + quest2.reward);
                    continue;
                }

            }
        }
    }

    public override void updateQuestMessage() {//make sure the quest discreption is up to date
        // call the base function after finishing to update them

        if (quest1.isComplete) {
                questMessage = quest2.getMessage()+" OR " + quest3.getMessage();
        } else if (quest2.isComplete) {
                questMessage = quest1.getMessage() + " OR " + quest3.getMessage();
        } else {//both quest still not completed
            questMessage = "(" + quest1.getMessage() + " AND " +
            quest2.getMessage() + ")  OR " + quest3.getMessage();
        }

        base.updateQuestMessage();
    }

    public override string getMessage(PlayerData PD = null) {
        if (quest3.didPlayerWin(PD))
            return STRWAITWON;
        if (quest3.didPlayerLose(PD)) {//it becomes like an AND quest

            if (quest1.didPlayerLose(PD)|| quest2.didPlayerLose(PD)) //lost both quest
                    return STRWAITFAILED;
            if (quest1.didPlayerWin(PD) && quest2.didPlayerWin(PD))
                return STRWAITWON;

            if (quest1.didPlayerWin(PD))
                return quest2.getMessage(PD);

            if (quest2.didPlayerWin(PD)) {
                return quest1.getMessage(PD);

            }
            if(quest1.didPlayerLose(PD) || quest2.didPlayerLose(PD)) {//failed both
                return STRWAITFAILED;
            }
            return questMessage = quest1.getMessage(PD) + " AND " + quest2.getMessage(PD);

        }
        //reaches here means quest3 is still active for current player
        if (quest1.didPlayerLose(PD) || quest2.didPlayerLose(PD)) //only third quest remains
            return quest3.getMessage(PD);

        //reached here means AND with OR quest  active 
        if (quest1.didPlayerWin(PD))
            return quest2.getMessage(PD) + " OR "+ quest3.getMessage(PD);

        if (quest2.didPlayerWin(PD)) {
            return quest1.getMessage(PD) + " OR " +quest3.getMessage(PD);

        }
        //all quests are active
        return questMessage = "(" + quest1.getMessage(PD) + " AND " + quest2.getMessage(PD) +
            ")  OR " +quest3.getMessage(PD);




    }
    public override bool didPlayerWin(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerLose();
        if ((quest1.didPlayerWin(PD) && quest2.didPlayerWin(PD)) || quest3.didPlayerWin(PD))
            return true;
        return false;
    }

    public override bool didPlayerLose(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerLose();
        if ((quest1.didPlayerLose(PD) || quest2.didPlayerLose(PD)) && quest3.didPlayerLose(PD))
            return true;
        return false;
    }

    public override void DestroyQuest() {
        quest1.DestroyQuest();
        quest2.DestroyQuest();
        quest3.DestroyQuest();
        // Debug.Log("destroying the object");

    }


}

/*
 *a OR quest that inhertes from the quest super class
 * that creates an OR logical connection between two quests
 * */
using System.Collections.Generic;
using UnityEngine;

public class ORQuest : Quest
{

    private Quest quest1;

    private Quest quest2;

    public ORQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;


        quest1 = _GM.QM.createRandomQuest(_players, _GM, false);

        QuestManager.QuestTypes[] usedTypes = new QuestManager.QuestTypes[1];
        usedTypes[0] = _GM.QM.getQuestType(quest1);
        //Debug.Log("quest1 type is: " + usedTypes[0]);


        quest2 = _GM.QM.createRandomQuest(_players, _GM, false, usedTypes);
        if (quest1 == null || quest2 == null) {
            Debug.Log("creating OR quest failed");
            return;
        }
        quest1.linkedQuest = true;
        quest2.linkedQuest = true;


        reward = quest1.reward + quest2.reward;
        updateQuestMessage();

    }

    public override void init() {
        base.init();
        //Debug.Log("initializing OR quest");
        quest1.init();
        quest2.init();
    }
    public override void tick() {
        if (isComplete)
            return;
        base.tick();

        quest1.tick();
        quest2.tick();


        if (quest1.isComplete && quest2.isComplete) {
            //Debug.Log("one OR quests completed");

            winners = quest1.winners;

            winners.AddRange(quest2.winners);
            questCompleted();

        }
    }

    public override void updateQuestMessage() {//make sure the quest discreption is up to date
        questMessage = quest1.getMessage() + " OR " + quest2.getMessage();

        base.updateQuestMessage();
    }


    public override string getMessage(PlayerData PD = null) {
       
        if (quest1.didPlayerWin(PD)) {
            return STRWAITWON;
        }
        if (quest2.didPlayerWin(PD)) {
            return STRWAITWON;
        }

        return quest1.getMessage(PD) + " OR " + quest2.getMessage(PD);

    }

    public override bool didPlayerWin(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerWin();
        if (quest1.didPlayerWin(PD) || quest2.didPlayerWin(PD))
            return true;
        return false;
    }

    public override void DestroyQuest() {
        quest1.DestroyQuest();
        quest2.DestroyQuest();
        // Debug.Log("destroying the object");

    }
    public override void RewardPlayers() {
        foreach (GameObject p in players) {
            PlayerData pd = p.GetComponent<PlayerData>();
            if (pd != null) {
                if (quest1.didPlayerWin(pd)) {
                    pd.RpcAddScore(quest1.reward);
                    continue;
                }
                if (quest2.didPlayerWin(pd)) {
                    pd.RpcAddScore(quest2.reward);
                    continue;
                }

            }
        }
    }



}

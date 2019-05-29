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
    public CompoundQuest(List<GameObject> _players, GameManager _GM) : base() {
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
        Debug.Log("created and quest");
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
        if (quest1.isComplete) {
            if (questMessage != quest2.getMessage()) {
                questMessage = quest2.getMessage()+" OR "+quest3.questMessage;
                //  updateQuestMessage();
            }
        } else if (quest2.isComplete) {
            if (questMessage != quest1.getMessage()) {
                questMessage = quest1.getMessage() + " OR " + quest3.questMessage;
                //  updateQuestMessage();
            }
        }

        if (quest3.isComplete) {
            Debug.Log("OR quest of compound quest completed");
            reward = quest3.reward ;
            winners = quest3.winners;

            //winners.AddRange(quest2.winners);
            questCompleted();

        }



        if (quest1.isComplete && quest2.isComplete) {
            Debug.Log("both AND quests of compound quest completed");
            reward = quest1.reward + quest2.reward;
            winners = quest1.winners;
            foreach (GameObject w in quest1.winners) {
                if (quest2.winners.IndexOf(w) != -1) {//found someone who completed both quests
                    winners.Add(w);
                }
            }
            //winners.AddRange(quest2.winners);
            questCompleted();

        }
    }

    public override void updateQuestMessage() {//make sure the quest discreption is up to date
        // call the base function after finishing to update them

        if (quest1.isComplete) {
                questMessage = quest2.getMessage()+" OR " + quest3.getMessage();
        } else if (quest2.isComplete) {
                questMessage = quest1.getMessage() + " OR " + quest3.getMessage();
        } else {//both quest still not completed
            questMessage = "(" + quest1.questMessage + " AND " +
            quest2.questMessage + ") OR " + quest3.questMessage;
        }

        base.updateQuestMessage();
    }

    public override void DestroyQuest() {
        quest1.DestroyQuest();
        quest2.DestroyQuest();
        quest3.DestroyQuest();
        // Debug.Log("destroying the object");

    }


}

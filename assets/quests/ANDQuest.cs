/*
 *a AND quest that inhertes from the quest super class
 * that creates an AND logical connection between two quests
 * */
using System.Collections.Generic;
using UnityEngine;

public class ANDQuest:Quest{

    private Quest quest1;

    private Quest quest2;

    public ANDQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;


        quest1 = _GM.QM.createRandomQuest(_players,_GM,false);

        QuestManager.QuestTypes[] usedTypes = new QuestManager.QuestTypes[1];
        usedTypes[0]= _GM.QM.getQuestType(quest1);
        Debug.Log("quest1 type is: " + usedTypes[0]);

        quest2= _GM.QM.createRandomQuest(_players, _GM, false,usedTypes);
        if (quest1==null || quest2==null) {
            Debug.Log("creating AND quest failed");
            return;
        }
        quest1.linkedQuest = true;
        quest2.linkedQuest = true;


        


        reward = quest1.reward + quest2.reward;
        updateQuestMessage();

    }

    public override void init(){
        base.init();
        Debug.Log("created and quest");
        quest1.init();
        quest2.init();
    }
    public override void tick() {
        base.tick();
        if (isComplete)
            return;
        quest1.tick();
        quest2.tick();



        if ((quest1.isComplete && quest2.isComplete)) {
            Debug.Log("both quests completed");

            winners = quest1.winners;
            foreach(GameObject w in quest2.winners) {
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
            if (questMessage != quest2.getMessage()) {
                questMessage = quest2.getMessage();
                //Debug.Log("first part completed");
                //  updateQuestMessage();
            }
        } else if (quest2.isComplete) {
            if (questMessage != quest1.getMessage()) {
                questMessage = quest1.getMessage();
                //Debug.Log("second part completed");

                //  updateQuestMessage();
            }
        } else {//both quest still not completed
            questMessage = quest1.questMessage + " AND " + quest2.questMessage;
        }

        base.updateQuestMessage();
    }

    public override void DestroyQuest() {
        quest1.DestroyQuest();
        quest2.DestroyQuest();
       // Debug.Log("destroying the object");

    }


}

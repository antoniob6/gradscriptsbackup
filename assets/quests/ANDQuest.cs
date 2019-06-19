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
        //Debug.Log("quest1 type is: " + usedTypes[0]);

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
        //Debug.Log("created and quest");
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


    public override string getMessage(PlayerData PD = null) {
        //Debug.Log("updating from inside the and quest");
        if (quest1.isComplete) {
            if(quest1.winners.IndexOf(PD.gameObject)>=0)
                return quest2.getMessage(PD);
            return STRWAITFAILED;
        }
        if (quest2.isComplete) {
            if (quest2.winners.IndexOf(PD.gameObject) >= 0)
                return quest1.getMessage(PD);
            return STRWAITFAILED;
        } 

        return questMessage = quest1.getMessage(PD) + " AND " + quest2.getMessage(PD);
        
    }

    public override bool didPlayerLose(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerLose();
        if (quest1.didPlayerLose(PD) || quest2.didPlayerLose(PD))
            return true;
        return false;
    }

    public override void DestroyQuest() {
        quest1.DestroyQuest();
        quest2.DestroyQuest();
       // Debug.Log("destroying the object");

    }


}

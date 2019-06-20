/*
 *a AND quest that inhertes from the quest super class
 * that creates an AND logical connection between two quests
 * */
using System.Collections.Generic;
using UnityEngine;

public class NOTQuest:Quest{

    public Quest quest1;


    public NOTQuest(Quest quest) : base() {
        players = quest.players;
        GM = quest.GM;
        quest1 = quest;
        quest1.linkedQuest = true;

        reward = quest1.reward;
        

        questMessage = "DON'T "+quest1.getMessage();

        updateQuestMessage();

        winners.AddRange(players);

    }

    public override void init(){
        base.init();
       //Debug.Log("created NOT quest");
        quest1.init();

    }
    public override void tick() {
        if (isComplete)
            return;
        base.tick();

        quest1.tick();

        if (quest1.isComplete  ) {
            questCompleted();

        }
    }
    public override void questCompleted() {
        foreach (GameObject w in quest1.winners) {
            if (winners.IndexOf(w) >= 0) {//found someone who didn't complete
                winners.Remove(w);
            }
        }
        base.questCompleted();
    }
    public override void updateQuestMessage() {//make sure the quest discreption is up to date
                                               // call the base function after finishing to update them
        questMessage = "DONT " + quest1.getMessage();
        base.updateQuestMessage();
    }

    public override string getMessage(PlayerData PD = null) {
        if (quest1.didPlayerWin(PD)) {
            return STRWAITFAILED;
        }
        if (quest1.didPlayerLose(PD)) {
            return STRWAITWON;
        }
        return "DON'T " + quest1.getMessage(PD);
    }

    public override bool didPlayerWin(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerWin();

        return quest1.didPlayerLose(PD);
    }
    public override bool didPlayerLose(PlayerData PD = null) {
        if (PD == null)
            return base.didPlayerLose();

        return quest1.didPlayerWin(PD);
    }

    public override void DestroyQuest() {
        quest1.DestroyQuest();

       // Debug.Log("destroying the object");

    }


}

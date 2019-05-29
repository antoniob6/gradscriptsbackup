/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class CondQuest:Quest{
    int condition;
    PlayerData pd;
    float threashold = 1;
    int condType;
    public CondQuest(List<GameObject> _players,int _reward, string _questMessage,
        GameManager _GM,int conditionIndex,int conditionType)
    {
        players = _players;
        reward = _reward;
        questMessage = _questMessage;
        GM = _GM;
        condition = conditionIndex;
        condType = conditionType;
        init();
    }
    private void init()   {
      
        updateQuestMessage();
        Random.InitState(System.DateTime.Now.Millisecond);
        if (players.Count != 1) {
            Debug.Log("only one player can do this type of quest");
            questCompleted();
            return;
        }
        pd = players[0].GetComponent<PlayerData>();
        if (!pd)
            Debug.Log("can't find playerData");
    }
    public override void tick() {
        if (isComplete)
            return;

        if (condType == 0) {
            Debug.Log("current stat: "+ pd.getStats()[condition]);
            if (pd.getStats()[condition] > threashold) {
                //we have achived the condition
                Debug.Log("condition quest completed");
                pd.RpcUpdateText("you have achived the condition");
                winners.Add(players[0]);
                questCompleted();
            }
        }

    }


}

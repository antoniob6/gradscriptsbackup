/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class RacingQuest:Quest{


    private float threshold = 30f;

    private Vector3 GoalLocation;


    public RacingQuest(List<GameObject> _players, GameManager _GM) {
        players = _players;
        GM = _GM;
        reward = Random.Range(50, 500);
        questMessage ="race to the right edge of the map";


        updateQuestMessage();

    }
    public override void init() {
        base.init();
        //Debug.Log("finding the end position");
        GoalLocation = GM.MM.getMapEndPosition();
    }


    public override void tick() {
        if (isComplete)
            return;
        //Debug.Log("ticking the base");
        base.tick();
        //Debug.Log("ticking the quest");



        foreach (GameObject p in players)
        {
            if (!p)
                continue;
            BoxCollider2D PBC = p.GetComponent<PlayerConnectionObject>().
                playerBoundingCollider;
            if (!PBC)
                continue;

            GameObject GO = PBC.gameObject;

            if (Vector3.Distance(GoalLocation, GO.transform.position) < threshold)
            {
                Debug.Log("some one reached the end");
                //Debug.Log("player has found the foundable goal");
                winners.Add(p);
                questCompleted();

            }
            
        }
    }
    public override void DestroyQuest() {
    }




}

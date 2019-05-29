/*
 *a location quest that inhertes from the quest super class
 * that monitors the players, and when the first one get close to the goal, he/she wins 
 * */
using System.Collections.Generic;
using UnityEngine;

public class LocationQuest:Quest{


    private float threshold;

    private Vector3 spawnPosition;
    private float spawnrange;
    private GameObject foundable;


    public LocationQuest(List<GameObject> _players, GameManager _GM):base(){
        players = _players;

        reward = Random.Range(50, 500);
        questMessage ="be the first to find the Blue candy";
        GM = _GM;
        threshold = 2f;
        updateQuestMessage();

    }
    public override void init()
    {

        Random.InitState(System.DateTime.Now.Millisecond);
        //spawnPosition = new Vector2(Random.Range(-spawnrange, spawnrange), GM.transform.position.y+10);

        spawnPosition = GM.MM.getRandomPositionAboveMap();
        //Debug.Log(spawnPosition);
        foundable =GM.networkSpawn("locationPrefab",spawnPosition);
        //GM.setTimeLimit(30f);
    }

    public override void tick() {
        base.tick();

        if (isComplete)
            return;


        if (!foundable) {
            if(!isComplete)
               questCompleted();
        }

        foreach (GameObject p in players)
        {
            BoxCollider2D PBC = p.GetComponent<PlayerConnectionObject>().
                playerBoundingCollider;
            if (!PBC)
                continue;

            GameObject GO = PBC.gameObject;
            if (!p||!foundable)
                continue;
            if (Vector3.Distance(foundable.transform.position, GO.transform.position) < threshold)
            {
                //Debug.Log("player has found the foundable goal");
                winners.Add(p);
                questCompleted();

            }
            
        }
    }
    public override void DestroyQuest() {
        GM.networkDestroy(foundable);
    }




}

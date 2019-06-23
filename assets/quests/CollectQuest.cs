/*
 *a collect quest that inhertes from the quest super class
 * that whoever helps in collecting the candy will get a reward
 * */
using System.Collections.Generic;
using UnityEngine;

public class CollectQuest : Quest
{

    private GameObject center;

    private float threshold= 4f;

    private Vector3 spawnPosition;
    private float spawnrange;
    private List<GameObject> candies;
    private List<GameObject> candiesToRemove;
    private List<GameObject> finders;

    private int collectLimit = 0;
    public CollectQuest(List<GameObject> _players, GameManager _GM) : base() {
        players = _players;
        GM = _GM;
        candies = new List<GameObject>();
        candiesToRemove = new List<GameObject>();
        finders = new List<GameObject>();

        //reward = Random.Range(50, 500);
        collectLimit = Random.Range(3, 20);
        reward = collectLimit * 20;

        questMessage = "take part of collecting " + collectLimit + " pieces of red candy";
        updateQuestMessage();
    }

    
    
    public override void init() {
        base.init();
        //Debug.Log("initializing collect quest");
        for (int i = 0; i < collectLimit * 2; i++) {
            //spawnPosition = new Vector2(players[0].transform.position.x+Random.Range(-spawnrange, spawnrange), players[0].transform.position.y + 10);
            spawnPosition = GM.MM.getRandomPositionAboveMap();

            //Debug.Log("spawnning candy");
            candies.Add(GM.networkSpawn("candyPrefab", spawnPosition));

        }
    }


    public override void tick() {
        base.tick();

        if (isComplete)
            return;

        foreach (GameObject p in players) {
            if (!p)
                continue;
            PlayableCharacter PC = p.GetComponent<PlayerConnectionObject>().PC;
            if (!PC)
                continue;
            foreach (GameObject c in candies) {

                if (Vector3.Distance(c.transform.position, PC.transform.position) < threshold) {
                    //Debug.Log("player has found a piece of candy");
                    if (finders.IndexOf(p) < 0)
                        finders.Add(p);
                    candiesToRemove.Add(c);
                    GM.networkDestroy(c);
                    collectLimit--;


                    if (collectLimit == 0) {
                        winners.AddRange(finders);
                        questCompleted();
                    }

                    questMessage = "take part of collecting " + collectLimit + " pieces of red candy";
                    updateQuestMessage();

                }
            }
            foreach(GameObject c in candiesToRemove) {
                candies.Remove(c);
            }
            candiesToRemove.Clear();

        }
    }



    public override void DestroyQuest() {
        // Debug.Log("destroying the object");
        foreach (GameObject c in candies) {
            GM.networkDestroy(c);
        }
    }


}

/* this class displayer the players' scores to the right side of the screen
 * it automaticly finds the players, and calculates their scores, and then sorts them 
 * before showing them to the player
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RankDisplayer : NetworkBehaviour {
    public PlayerRank PlayerRankPrefab;
    public GameObject CanvasReference;
    public float UpdateEvery = 1f;
    public bool isActive = true;



    private List<GameObject> oldScoresGO;
    void Start () {
        if (!isLocalPlayer)//only local player updates the ranks
            return;

        InvokeRepeating("updateRanks",0f, UpdateEvery);
        oldScoresGO = new List<GameObject>();


	}
    private void OnDisable() {
        CancelInvoke();
    }

    void updateRanks() {
        if (!isActive) {//if script is disabled
            CancelInvoke("updateRanks");
            return;
        }

        PlayerConnectionObject[] PCOs =FindObjectsOfType<PlayerConnectionObject>();
        int namesIndex = 0;
        string[] names = new string[PCOs.Length];
        float[] scores = new float[PCOs.Length];
        foreach(PlayerConnectionObject PCO in PCOs) {//find each player scores

            if (!PCO) {
                Debug.Log("null PCO");
                return;
            }
            PlayerData PD = PCO.GetComponent<PlayerData>();
            if (!PD) {
                Debug.Log("null PD");
                return;
            }
            bool localPlayer = false;
            if (PCO.gameObject==gameObject)//this is the currentPlayer
                localPlayer = true;


            string playerName = PD.playerName;
            float playerScore = PD.score;

            names[namesIndex] = playerName;
            scores[namesIndex] = playerScore;
            namesIndex++;

        }

        doubleMergeSort(scores, names);
        displayScores(names, scores, namesIndex);
    }

   

    void displayScores(string[] names, float[] scores,int namesCount) {
        if ( oldScoresGO.Count != 0) {//delete the old scores
            foreach(GameObject go in oldScoresGO) {
                Destroy(go);
            }

            oldScoresGO.Clear();

        }

        // create the new scores
        //GameObject LastPRGO = null;//got problem that the translation take time
        Vector3 newSpawnLocation = Vector3.zero;
        for (int i = 0; i<namesCount && i < names.Length; i++) {
           // Vector3 spawnLocation = Vector3.zero;
            //spawnLocation += new Vector3(512, 0);

            /*
            if (LastPRGO) {
                PlayerRank oldPR = LastPRGO.GetComponent<PlayerRank>();
                spawnLocation =  Vector3.down * oldPR.scoreReference.transform.localPosition.y;

            }
            */
 //           Debug.Log(spawnLocation);
            GameObject LastPRGO = Instantiate(PlayerRankPrefab.gameObject, Vector3.zero,
                Quaternion.identity, CanvasReference.transform);


            PlayerRank PR = LastPRGO.GetComponent<PlayerRank>();
            PR.setNameAndScore(names[i], "" + scores[i]);

            newSpawnLocation= PR.translateText(newSpawnLocation);//this changes the spawnLocation
            //Debug.Log("rank pos: " + newSpawnLocation);
            oldScoresGO.Add(LastPRGO);

        }
 //       Debug.Log("stats finished updating");
    }

    private void doubleMergeSort(float[]  arr ,string[] names) {

        float tempf = 0;
        string temps = "";
        for (int write = 0; write < arr.Length; write++) {
            for (int sort = 0; sort < arr.Length - 1; sort++) {
                if (arr[sort] < arr[sort + 1]) {
                    tempf = arr[sort + 1];
                    arr[sort + 1] = arr[sort];
                    arr[sort] = tempf;

                    temps = names[sort + 1];
                    names[sort + 1] = names[sort];
                    names[sort] = temps;
                }
            }
        }
    }

}

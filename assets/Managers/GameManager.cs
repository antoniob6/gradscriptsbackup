/*
 * this script is responsible for the game flow, meaning it gives the quests to the players and 
 * monitors them, giving out new quest when the old quests are completed.
 * also it displays the message to each client, give out round messages, rewards the players,
 * ends the game, and display player scores after the end.
 * */





using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
    // public MapGenerator mapGenerator;
    public MapManager MM;
    public QuestManager QM;


    [Header("SpawnablePrefaba")]
    public GameObject bossPrefab;
    public GameObject enemyPrefab;
    public GameObject candyPrefab;
    [SerializeField]
    private GameObject collectiblePrefab;
    [SerializeField]
    private GameObject foundablePrefab;

    public GameObject shipPrefab;
    public GameObject shipKillerPrefab;

    public GameObject playerKillerPrefab;
    [Header("Other attributes")]
    public float questTimeLimit;
    public GameObject enemieSpawner;
    public int wantedRounds = 2;
    public GameObject map;
    public GameObject center;



    public int currentRound = 0;
    private bool QuestActive = false;
    private int totalQuests = 4;



    private GameObject winnedPlayer = null;
    [HideInInspector]public List<GameObject> players;

    [SerializeField]
    private float spawnrange = 60f;



    [SerializeField]
    private float spawnInterval = 1.0f;

    [SerializeField]
    private GameObject gameOverCanvas;


    public Vector3[] spawnPoints;


    public List<Quest> quests;

    public bool mapGenerated = false;
    private bool spawningCollectibles = false;

    int spawnIndex = 0;

    private bool haveQuest = false;

    public Rules currentRules;

    private int questCount = 0;
    public bool waited = false;
    private List<Quest> activeQuests = new List<Quest>();

    private bool shouldBeQuest = true;


    public bool changeMapb = false;
    public bool changeQuest = false;

    // Use this for initialization
    void Start() {
        quests = new List<Quest>();
        players = new List<GameObject>();
        Invoke("startGame", 2);
        changeMap();

    }


    void changeMap() {
        randomRules();
       
        if((new System.Random()).NextDouble() <= 0.1)
            MM.createNewMapPlatformsOnly();
        else
            MM.createNewMap();
    }

    void startGame() {
        starting = true;
        GameObject[] pl = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in pl) {
           // p.GetComponent<PlayerConnectionObject>().CmdSpawnMyUnit();
        }

    }
    private bool starting = false;

    private bool playersReady = false;
    private bool roundMessageDisplayed = false;

    int playerCount = 0;
    string[] playerNames;
    GameObject[] oldPlayers;
    bool firstCheckCompleted = false;

    public bool devmode = true;
    void Update() {
        if (!starting || !isServer)
            return;

        checkSpawnables();

        GameObject[] curplayers = GameObject.FindGameObjectsWithTag("Player");

        //if (curplayers.Length< LobbyData.instance.numOfConnectedPlayer * 0.8) {
        //    TextManager.instance.RpcDebugOnAll("<0.8 ready: " + curplayers.Length + " of " +
        //        LobbyData.instance.numOfConnectedPlayer);

        //    return;
        //}

        if (firstCheckCompleted && playerCount > curplayers.Length) {//a player disconnected
            TextManager.instance.displayMessageToAll("player has disconnected",5);
            TextManager.instance.RpcDebugOnAll("player disconnected");
        }
        if (firstCheckCompleted && playerCount < curplayers.Length) {//a player connected late
            
            MM.syncAllMaps();
            ForceQuestsToComplete();
            //foreach (Quest q in activeQuests) {
            //    q.updateQuestMessage();
            //}
            TextManager.instance.RpcDebugOnAll("player connected late: " + curplayers.Length);

        }
        if (playerCount != curplayers.Length) {
            players.Clear();
            playerCount = curplayers.Length;
            playerNames = new string[curplayers.Length];
            //oldPlayers = new GameObject[curplayers.Length];
            for (int i = 0; i < curplayers.Length; i++) {//need to manually add because of null values(disconnection)
                if (!curplayers[i])
                    continue;
                players.Add(curplayers[i]);
                // oldPlayers[i] = curplayers[i];
                PlayerData PD = curplayers[i].GetComponent<PlayerData>();
                if (PD) {
                    playerNames[i] = PD.playerName;
                }

            }
        }
        if (playersReady) {
            tickQuests();



            if (!roundMessageDisplayed) {
                setDifficulty();
                TextManager.instance.displayMessageToAll("starting now");
                roundMessageDisplayed = true;
            }
        } else {
            checkForReadyPlayers();
        }

        checkRounds();

        if (Input.GetKeyDown(KeyCode.G)) {
            TextManager.instance.displayMessageToAll("host force changing map");
            changeMap();
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            TextManager.instance.displayMessageToAll("host force changing goal");
            activeQuests.Clear();
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            if (devmode) {
                devmode = false;
            } else {
                devmode = true;
                TextManager.instance.displayMessageToAll("host force limited rules");


            }
        }

        if (changeMapb) {//provides a way for inspector to call funtion
            changeMapb = false;
            changeMap();
        }
        if (changeQuest) {
            changeQuest = false;
            activeQuests.Clear();
        }

        firstCheckCompleted = true;

    }


    private void randomRules() {
        if (currentRules == null)
            currentRules = new Rules();
        currentRules.randomizeRules();
        if(devmode)
            currentRules.randomizeDevRules();

        GravitySystem.instance.gravityForce = currentRules.gravityForce;
        if (currentRules.isCircle)
            GravitySystem.instance.gravityType = 
                GravitySystem.GravityType.ToCenter;
        else
            GravitySystem.instance.gravityType =
                GravitySystem.GravityType.Down;

        //RulesManager.instance.setRules(currentRules);

        //update the rules on the managers

        GravitySystem.instance.gravityForce = currentRules.gravityForce;
        GravitySystem.instance.gravityType = currentRules.resolveGravityType();
        GravitySystem.instance.isReverseGravity = currentRules.isReverseGravity;
        GravitySystem.instance.runningSpeed = currentRules.runningSpeed;
        GravitySystem.instance.jumpHeight = currentRules.jumpHeight;
    }



    private void tickQuests() {
        foreach (Quest q in activeQuests) {
            q.tick();
        }
    }

    public void questCompleted(Quest q) {//gets called back from the quest
        StartCoroutine(questCompletedCo(q));
    }
    IEnumerator questCompletedCo(Quest q) {
        playersReady = false;

        if (currentRound == wantedRounds - 1)
            AudioManager.instance.play("gameOver");
        else
            AudioManager.instance.play("stageClear");

        foreach (GameObject p in players) {
            PlayerData PD = p.GetComponent<PlayerData>();
            PD.RpcRoundHasEnded();
        }

        TextManager.instance.displayMessageToAll("round ending");
        yield return new WaitForSeconds(0);
        currentRound++;
        //changeMap();
        activeQuests.Clear();
        
    }

    public void ForceQuestsToComplete() {
        foreach (Quest q in activeQuests) {
            if(starting && playersReady)
                q.questCompleted();//reward the players who completed thier section
            else
                q.DestroyQuest();//skip quest that hasn't started
           
        }
    }

    bool hasGameover = false;


    public void checkRounds() {
        if (currentRound < wantedRounds) {
            if (activeQuests.Count == 0) {
                startNewRound();
            } else {
                checkForSkip();
            }
        } else {//reached wanted amount of rounds
            if (!hasGameover) {//do the game over routine
                hasGameover = true;
                GameOverSeqence();


            } else {//check if people want to play again
                checkForPlayAgain();
            }
        }

    }

    public void startNewRound() {

        foreach (GameObject p in players) {
            if (p)
                p.GetComponent<PlayerData>().resetRoundStats();
        }
        Quest l = QM.createRandomQuest(players, this);
        Debug.Log("created: " + l.questMessage);
        if (l != null) {

            activeQuests.Add(l);
            TextManager.instance.displayMessageToAll("Waiting for players to get ready", 15);
            CancelInvoke("completeQuests");

            
        }
    }
    public void GameOverSeqence() {

        // AudioManager.instance.play("gameOver");
        if (gameOverCanvas) {
            GameObject gameOver = Instantiate(gameOverCanvas, Vector3.zero, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(gameOver);
        } else {
            Debug.Log("gameOverCanvas not assigned");
        }

        string scores = "GAME OVER\n";
        int index = 0;
        foreach (GameObject p in players) {
            index++;
            PlayerData pdata = p.GetComponent<PlayerData>();
            scores += "player " + index + " score: " + pdata.score + "\n";


            pdata.RpcGameHasEnded();

        }
        TextManager.instance.displayMessageToAll(scores, 20);
    }

    public List<GameObject> enemies= new List<GameObject>();
    public void setDifficulty() {
        clearDifficullty();

        int numOfEnemies = currentRound * 5+ (int)currentRules.length / 100;
        //Debug.Log("spawnning " + numOfEnemies + " enemies");
        for(int i = 0; i < numOfEnemies; i++) {
            Vector3 randomLocation = MM.getRandomPositionAboveMap();

            GameObject GO = networkSpawn("enemyPrefab", randomLocation);
            enemies.Add(GO);
        }

    }
    public void clearDifficullty() {
        foreach(GameObject e in enemies) {
            if (!e)
                continue;
            NetworkServer.Destroy(e);
        }
        enemies.Clear();
    }


    public void setTimeLimit(float timeLimit) {
        Invoke("completeQuests", timeLimit);
    }


    [Server]
    public void ResetPlayersScore()
    {
        foreach (GameObject p in players)
        {
            PlayerData pdata = p.GetComponent<PlayerData>();
            pdata.RpcUpdateScore(0);
            pdata.RpchasDied(false);
        }
    }
    List<GameObject> spawnedBullets = new List<GameObject>();
    List<GameObject> toSpawnList = new List<GameObject>();
    [Server]
    public GameObject networkSpawn(string message,Vector3 position)
    {

        if (spawnedBullets.Count >=100) {
                if (spawnedBullets[0] != null) {
                    NetworkServer.Destroy(spawnedBullets[0]);

                   // Debug.Log("server removing bullet" + spawnedBulletsTime.Count + " " + spawnedBullets.Count);
                }
                spawnedBullets.RemoveAt(0);

            
        }

        GameObject enemy=null;
        if (message == "locationPrefab")
        {
            enemy = Instantiate(foundablePrefab, position, Quaternion.identity) as GameObject;

        }
        if (message == "candyPrefab") {
            enemy = Instantiate(candyPrefab, position, Quaternion.identity) as GameObject;

        }
        if (message == "bossPrefab") {
            enemy = Instantiate(bossPrefab, position, Quaternion.identity) as GameObject;

        }
        if (message == "enemyPrefab") {
            enemy = Instantiate(enemyPrefab, position, Quaternion.identity) as GameObject;

        }
        if (message == "shipPrefab") {
           enemy = Instantiate(shipPrefab, position, Quaternion.identity) as GameObject;

        }
        if (message == "shipKillerPrefab") {
           enemy = Instantiate(shipKillerPrefab, position, Quaternion.identity) as GameObject;

        }
        if (message == "playerKillerPrefab") {
            enemy = Instantiate(playerKillerPrefab, position, Quaternion.identity) as GameObject;

        }
        if (enemy) {
            toSpawnList.Add(enemy);
            spawnedBullets.Add(enemy);

            return enemy;
        }

        Debug.LogError("name error in GM.networkSpawn()");
        return null;
    }
    public float lastSpawnTime = 0;
    public void checkSpawnables() {
        if (!isServer)
            return;

        if(Time.time - lastSpawnTime >= 0.2) {
            if (toSpawnList.Count >= 1) {
                if (toSpawnList[0]) {
                    NetworkServer.Spawn(toSpawnList[0]);
                    //Debug.Log("spawning network Object");
                    lastSpawnTime = Time.time;
                }
                toSpawnList.RemoveAt(0);

            }

        }
    }


    [Server]
    public void networkDestroy(GameObject toDestroy) {
        NetworkServer.Destroy(toDestroy);
    }

    public void CheckCurQuests() {
        foreach(Quest q in activeQuests) {
            if (q.isComplete) {
                foreach(GameObject g in q.winners) {
                    PlayerData pd = g.GetComponent<PlayerData>();
                    pd.RpcUpdateScore(pd.score + q.reward);
                }
            }

        }
    }


    [Server]
    public void startSpawingEnemies() {
        enemieSpawner.SetActive(true);
    }
    [Server]
    public void stopSpawingEnemies() {
       // enemieSpawner.SetActive(false);
    }

    #region checkPlayerVotes

    private void checkForReadyPlayers() {
        if (players.Count == 0)
            return;
        int readyCount = 0;
        foreach(GameObject p in players) {
            PlayerData PD = p.GetComponent<PlayerData>();
            if (PD.playerIsReady)
                readyCount++;
        }
        
        if ((float)readyCount / players.Count>=0.4f) {
            //about  half the players ready so start the game
            playersReady = true;

            foreach (GameObject p in players) {
                PlayerData PD = p.GetComponent<PlayerData>();
                PD.RpcRoundHasStarted();
            }

            // TextManager.instance.displayMessageToAll("starting now");
        }

    }

    private void checkForPlayAgain() {
        if (players.Count == 0)
            return;
        int readyCount = 0;
        foreach (GameObject p in players) {
            PlayerData PD = p.GetComponent<PlayerData>();
            if (PD.playerWantsToPlayAgain)
                readyCount++;
        }

        if ((float)readyCount / players.Count >= 0.6f) {
            //about  half the players ready so start the game
           // Debug.Log("starting new game");

            startNewGame();


            // TextManager.instance.displayMessageToAll("starting now");
        }

    }
    private void checkForSkip() {
        if (players.Count == 0)
            return;
        int skipCount = 0;
        foreach (GameObject p in players) {
            PlayerData PD = p.GetComponent<PlayerData>();
            if (PD.playerWantsToSkip)
                skipCount++;
        }

        if ((float)skipCount / players.Count >= 0.4f) {
            //about  half the players ready so skip the game
            //Debug.Log("skipping round");

            foreach (GameObject p in players) {
                PlayerData PD = p.GetComponent<PlayerData>();
                PD.RpcRoundSkipped();
            }
            ForceQuestsToComplete();
            questCompleted(null);
            currentRound--;
            // TextManager.instance.displayMessageToAll("starting now");
        }

    }

    #endregion

    private void startNewGame() {
        ForceQuestsToComplete();
        hasGameover = false;
        currentRound = 0;
        changeMap();



        foreach (GameObject p in players) {//tell each player that the game has ended
            PlayerData PD = p.GetComponent<PlayerData>();
            PD.gameHasStarted();
        }

    }


    private void OnPlayerDisconnected(NetworkIdentity player) {
        Debug.Log("player disconnected");
        PlayerConnectionObject PCO = player.GetComponent<PlayerConnectionObject>();
        if (PCO) {
            string disconnectMessage = "Player: ";
            disconnectMessage += PCO.PlayerName + ", has dissconnected";
            TextManager.instance.displayMessageToAll(disconnectMessage, 5);
        } else {
            Debug.Log("cant find PCO componenet");
        }
    }
}

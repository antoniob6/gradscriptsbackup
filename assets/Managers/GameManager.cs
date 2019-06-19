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
    void Update() {
        if (!starting || !isServer)
            return;

        GameObject[] curplayers = GameObject.FindGameObjectsWithTag("Player");
        if (playerCount > curplayers.Length) {//a player disconnected
            for(int i=0;i<players.Count;i++) {
                if (Array.IndexOf(curplayers, players[i])<=0) {
                    TextManager.instance.displayMessageToAll("player: " + playerNames[i] + " has dissconnected");
                }
            }
        }
        if (playerCount != curplayers.Length) {
            players.Clear();
            playerCount = curplayers.Length;
            playerNames = new string[curplayers.Length];
            //oldPlayers = new GameObject[curplayers.Length];
            for (int i = 0; i < curplayers.Length; i++) {
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
            //  displayMessageToAll("testing message: hello this is a test message");
            changeMap();
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            activeQuests.Clear();
        }
        if (Input.GetKeyDown(KeyCode.J)) {
           // TextManager.instance.createTextOnAll(transform.position, "90");
        }

        if (changeMapb) {
            changeMapb = false;
            changeMap();
        }
        if (changeQuest) {
            changeQuest = false;
            activeQuests.Clear();
        }

    }

    private void randomRules() {
        currentRules = new Rules();
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
            q.DestroyQuest();
            //q.questCompleted();use this to keep results from skipped quests
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
        //Debug.Log("created new quest " + l.questMessage);
        if (l != null) {
            activeQuests.Add(l);
            TextManager.instance.displayMessageToAll("Waiting for players to get ready", 100);
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

        int numOfEnemies = currentRound * 20 + (int)currentRules.length / 100;
       // Debug.Log("spawnning " + numOfEnemies + " enemies");
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

    [Server]
    public GameObject networkSpawn(string message,Vector3 position)
    {
        if (message == "locationPrefab")
        {
            GameObject enemy = Instantiate(foundablePrefab, position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(enemy);
            return enemy;
        }
        if (message == "candyPrefab") {
            GameObject enemy = Instantiate(candyPrefab, position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(enemy);
            return enemy;
        }
        if (message == "bossPrefab") {
            GameObject enemy = Instantiate(bossPrefab, position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(enemy);
            return enemy;
        }
        if (message == "enemyPrefab") {
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(enemy);
            return enemy;
        }
        if (message == "shipPrefab") {
            GameObject enemy = Instantiate(shipPrefab, position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(enemy);
            return enemy;
        }
        if (message == "shipKillerPrefab") {
            GameObject enemy = Instantiate(shipKillerPrefab, position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(enemy);
            return enemy;
        }


        Debug.LogError("name error in GM.networkSpawn()");
        return null;
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
        enemieSpawner.SetActive(false);
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

        if ((float)readyCount / players.Count >= 0.4f) {
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

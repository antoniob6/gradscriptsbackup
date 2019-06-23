/*
 *this script hold player data(as the name implies),
 * from on side it stores the data about the specific player,
 * and from the other side, it connects the user interface to the game mechanics, 
 * and displays the required info, such as the player health, score and current objective. 
 */
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public class PlayerData : NetworkBehaviour
{

    public Canvas canvas;
    public Text scoreField;
    public Text objectiveField;
    public SimpleHealthBar healthBar;


    [SyncVar] public string playerName = "default";
    [SyncVar] public Color playerColor;


    [SyncVar]public float score = 0f;
    [SyncVar]public string objectiveText = "objectiveText";
    [SyncVar]public bool hasDied = false;
    [SyncVar] public bool playerIsReady = false;
    [Header("total player stats")]
    [SyncVar] public int killedEntityCount = 0;
    [SyncVar] public int killedPlayerCount = 0;
    [SyncVar] public int deathCount = 0;
    [SyncVar] public int candyCount = 0;
    [SyncVar] public int jumpCount = 0;
    [Header("round player stats")]

    [SyncVar] public int roundKilledEntityCount = 0;
    [SyncVar] public int roundKilledPlayerCount = 0;
    [SyncVar] public int roundDeathCount = 0;
    [SyncVar] public int roundCandyCount = 0;
    [SyncVar] public int roundJumpCount=0;

    private PlayerConnectionObject PCO;
    private PlayerReceiveDamage PRD;
    private void Start() {
        PCO = GetComponent<PlayerConnectionObject>();
        PRD = GetComponent<PlayerReceiveDamage>();

        if (isServer && playerName == "default") { 
            playerName = "player " + new System.Random().Next(1,100) ;
        }
    }

    #region healthUpdate
    private void OnEnable() {
        InvokeRepeating("localUpdateHealth", 1f, 1f);
    }
    private void OnDisable() {
        CancelInvoke();
    }
    public void localUpdateHealth() {
        if (!isLocalPlayer)
            return;
        if (PRD) {
            healthBar.UpdateBar(PRD.currentHealth, PRD.maxHealth);
        } else {
            Debug.Log("cant find playerRecieveDamage");
        }
    }
    #endregion

    #region scoreDeathObjectiveRPC

    [ClientRpc]
    public void RpcUpdateScore(float s) {
        score = s;
        scoreField.text = "score: " + score.ToString();
         TextManager.instance.createTextOnLocalInstance(PCO.playerBoundingCollider.gameObject.transform.position,"+" + (int)(s - score));

    }
    [ClientRpc]
    public void RpcAddScore(float s) {
        score += s;
        scoreField.text = "score: " + score.ToString();
        //Debug.Log("score updated");
        if (!PCO.playerBoundingCollider) {
            Debug.Log("returning because there is no PBC");
            return;
        }
        //Debug.Log("displaying added score:"+s);
        if(s>0)
            TextManager.instance.createTextOnLocalInstance(PCO.playerBoundingCollider.gameObject.transform.position, "+" + (int)(s));
        else if(s<0)
            TextManager.instance.createRedTextOnLocalInstance(PCO.playerBoundingCollider.gameObject.transform.position,  ""+(int)(s));
    }
    [ClientRpc]public void RpcUpdateText(string text) {
        objectiveText = text;
        objectiveField.text = objectiveText;
    }




    [ClientRpc]
    public void RpchasDied(bool text) {
        hasDied = text;
    }
    #endregion

    #region buttons

    public PlayerReadyBtn PRB;
    public void playerReadyBtn() {
        if (!isLocalPlayer)
            return;

        CmdPlayerReadyBtn();
        PRB.gameObject.SetActive(false);
      //  Debug.Log("player ready and Btn disabled");
        
    }

    [Command] public void CmdPlayerReadyBtn() {
        playerIsReady = true;
    }



    public PlayerSkipBtn PSB;
    public bool playerWantsToSkip = false;
    public void playerSkipBtn() {
        if (!isLocalPlayer)
            return;
        if(PSB)
            PSB.gameObject.SetActive(false);
        CmdPlayerSkipBtn(true);

        //  Debug.Log("player ready and Btn disabled");
    }
    [Command]public void CmdPlayerSkipBtn(bool action) {
        playerWantsToSkip = action;
    }

    public PlayAgainBtn PAB;
    public RawImage FinalScoresImage;
    public bool playerWantsToPlayAgain = false;


    public void playerAgainBtn() {
        if (!isLocalPlayer)
            return;

        CmdPlayerAgainBtn();
      //  PAB.gameObject.SetActive(false);
       // FinalScoresImage.gameObject.SetActive(false);
        //  Debug.Log("player ready and Btn disabled");
    }
    [Command]
    public void CmdPlayerAgainBtn() {
        playerWantsToPlayAgain = true;
    }

    #endregion
    #region roundAndGameStartAndEnd
    [ClientRpc]
    public void RpcRoundHasEnded() {
        playerIsReady = false;
        playerWantsToSkip = false;
        if (isLocalPlayer) {//activate btn on local player
            if (PRB)
                PRB.gameObject.SetActive(true);
            if (PSB)
                PSB.gameObject.SetActive(true);
        }

    }
    [ClientRpc]
    public void RpcRoundHasStarted() {
        playerWantsToSkip = false;
        if (isLocalPlayer) {//deactivate btn on local player
            if (PRB)
                PRB.gameObject.SetActive(false);
            if (PSB)
                PSB.gameObject.SetActive(true);

            BackGroundManager BGM = GetComponent<BackGroundManager>();
            if (BGM) {
                BGM.nextImage();
            }
        }
    }
    [ClientRpc] public void RpcGameHasEnded() {
        playerIsReady = false;
        playerWantsToPlayAgain = false;
        if (isLocalPlayer) {//activate btn on local player
            if (PAB) {
                PAB.gameObject.SetActive(true);
                FinalScoresImage.gameObject.SetActive(true);
            }
            if (PRB)
                PRB.gameObject.SetActive(false);
            if (PSB)
                PSB.gameObject.SetActive(false);
        }

    }

    public void gameHasStarted() {//called when the game starts
        playerWantsToPlayAgain = false;

        if (!isServer) {
            Debug.Log("someone other than the server is trying to reset the game");
            return;
        }
        resetGameStats();
        RpcGameHasStarted();

    }
    [ClientRpc]public void RpcGameHasStarted() {
        if (isLocalPlayer) {//deactivate btn on local player
            if (PAB) {
                PAB.gameObject.SetActive(false);
                FinalScoresImage.gameObject.SetActive(false);
            }
            if (PRB)
                PRB.gameObject.SetActive(true);
            if (PSB)
                PSB.gameObject.SetActive(true);
        }
    }

    #endregion

    [Command]public void CmdplayerJumped() {
        jumpCount++;
        roundJumpCount++;

    }

    public float[] getStats() {
        float[] stats = new float[10];
        stats[0] = score;
        stats[1] = hasDied ? 0f : 1f;
        stats[2] = gameObject.GetComponent<PlayerReceiveDamage>().currentHealth;
        stats[3] = gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        return stats;
    }

    public void takenDamage(float currentHealth, float maxHealth) {
        //Debug.Log("updated health bar");
        //if (isServer) 
        //  Debug.Log("server taken damage");

        RpcTakenDamage(currentHealth, maxHealth);

        PCO.PC.RpcPlayerTakenDamage();//notify that was hurt
    }
    [ClientRpc]
    private void RpcTakenDamage(float currentHealth, float maxHealth) {
        healthBar.UpdateBar(currentHealth, maxHealth);//updates on attack
    }



    public void playerDied() {
        hasDied = true;
        deathCount++;
        roundDeathCount++;
        RpcAddScore(-100);//penalty for dying
        PCO.PC.playerDied();//make the player die
        //PCO.PC.RpcPlayerDied();//make the player die
    }
    [ClientRpc] public void RpcPlayerDied() {

    }

    public void playerKilledPlayer() {
        RpcAddScore(100);
        killedPlayerCount++;
        roundKilledPlayerCount++;
    }
    public void playerKilledEntity() {
        RpcAddScore(50);
        killedEntityCount++;
        roundKilledEntityCount++;
    }

    public void playerCollectedCandy() {
        RpcAddScore(10);
        candyCount++;
        roundCandyCount++;
    }



    public void resetRoundStats() {
        roundKilledEntityCount = 0;
        roundKilledPlayerCount = 0;
        roundDeathCount = 0;
        roundCandyCount = 0;
        roundJumpCount = 0;

    }
    public void resetGameStats() {
        resetRoundStats();

        score= 0;
        deathCount = 0;
        killedEntityCount = 0;
        killedPlayerCount = 0;
        candyCount = 0;
        jumpCount = 0;

    }

    [ClientRpc]public void RpcRoundSkipped() {
        playerWantsToSkip = false;
        PSB.gameObject.SetActive(true);
    }
}

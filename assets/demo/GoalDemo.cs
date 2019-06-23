using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalDemo : MonoBehaviour {
    public Text ObjTextBox;
    public GameObject fakePlayer;
    public GameObject fakePlayer2;
    
    public GameManager GM;
    public QuestManager QM;

    public Quest CurrQuest;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(CurrQuest!=null)
            ObjTextBox.text = CurrQuest.getMessage();


	}

    public void newQuest() {
        List<GameObject> golist = new List<GameObject>();
        golist.Add(fakePlayer);
        golist.Add(fakePlayer2);
        CurrQuest= QM.createRandomQuest(golist,GM);
    }
}

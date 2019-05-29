using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRank :MonoBehaviour {
    public Text nameReference;
    public Text scoreReference;
	

    public void setNameAndScore(string name,string score) {
        nameReference.text = name;
        scoreReference.text = score;
    }
    public Vector3 translateText(Vector3 amount) {
        Vector3 newPosition = scoreReference.transform.position- nameReference.transform.position + amount;
        nameReference.transform.Translate(amount);
        scoreReference.transform.Translate(amount);
        return newPosition;
    }

}


using System;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    public SpawnableObj[] spawnables;
    // Use this for initialization
    void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        foreach (SpawnableObj s in spawnables) {

        }
    }




    public void spawnObjOnLocalInstance(string name,Vector3 spawnPos) {
        SpawnableObj s = Array.Find(spawnables, obj => obj.name == name);
        if (s == null) {
            Debug.Log("effect " + name + " wasn't found");
            return;
        } else
            Instantiate(s.obj, spawnPos, Quaternion.identity);

    }
}

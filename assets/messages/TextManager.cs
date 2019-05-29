using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TextManager : NetworkBehaviour {
    [HideInInspector]
    public static TextManager instance;
    public XPTextMessage textPrefab;
    public XPTextMessage redTextPrefab;
    [SerializeField]
    private GameObject messageToAllPrefab;

    void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void createTextOnLocalInstance(Vector3 position,string text) {
        GameObject textObject = Instantiate(textPrefab.gameObject, position, Quaternion.identity);
        textObject.GetComponent<XPTextMessage>().updateTextOnLocalInstance(text);
    }
    public void createRedTextOnLocalInstance(Vector3 position, string text) {
        GameObject textObject = Instantiate(redTextPrefab.gameObject, position, Quaternion.identity);
        textObject.GetComponent<XPTextMessage>().updateTextOnLocalInstance(text);
    }

    public void createTextOnAll(Vector3 position, string text) {
        GameObject textObject = Instantiate(textPrefab.gameObject, position, Quaternion.identity);
        NetworkServer.Spawn(textObject);
        textObject.GetComponent<XPTextMessage>().updateTextOnAll(text);

    }

    public void displayMessageToAll(string m, float time = 3) {
        StartCoroutine(displayMessage(m, time));
    }
    private GameObject message;
    IEnumerator displayMessage(string m, float time) {
        if (message)
            NetworkServer.Destroy(message);
        message = Instantiate(messageToAllPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(message);
        message.GetComponent<GameMessage>().RpcUpdateText(m);
        yield return new WaitForSeconds(time);
        if (message)
            NetworkServer.Destroy(message);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class XPTextMessage : NetworkBehaviour {
    public TextMesh textReference;

    //public Text textReference;
    public float speed=3;
    public float lifeTime=2;
    public Vector3 updirection;

    public void Start() {
        updirection = GravitySystem.instance.getUpDirection(transform.position);
        StartCoroutine("fadeOut");
    }


    public void Update() {
        transform.Translate(updirection * speed * Time.deltaTime);
    }
    public IEnumerator fadeOut() {
        float startAlpha = textReference.color.a;
        float rate = 1.0f / lifeTime;
        float progress = 0.0f;
        while (progress < 1.0) {
            Color tmp = textReference.color;
            tmp.a = Mathf.Lerp(startAlpha, 0, progress);
            textReference.color = tmp;
            progress += rate*Time.deltaTime;
            yield return null;

        }
        Destroy(gameObject);
        
    }

    public void updateTextOnLocalInstance(string newText) {
        textReference.text = newText;
    }


    public void updateTextOnAll(string newText) {
        CmdUpdateTextOnAll(newText);
    }
    [Command]
    public void CmdUpdateTextOnAll(string newText) {
        textReference.text = newText;
        RpcUpdateTextOnAll(newText);
    }
    [ClientRpc]
    public void RpcUpdateTextOnAll(string newText) {
        textReference.text = newText;
    }


}

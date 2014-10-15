using UnityEngine;
using System.Collections;

public class debugAI_Instantiate : MonoBehaviour {
    public int count;
    public GameObject g;
	// Use this for initialization
	void Start () {
        GameObject inst;
        for (int i = 0; i < count; i++) { 
            inst = (GameObject)Instantiate(g);
            inst.transform.parent = gameObject.transform;
        }
	}

}

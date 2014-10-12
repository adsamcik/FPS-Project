using UnityEngine;
using System.Collections;

public class StartFinish : MonoBehaviour {
    void OnTriggerEnter() {
        transform.parent.GetComponent<RunGame>().StartOrFinish();
    }
}

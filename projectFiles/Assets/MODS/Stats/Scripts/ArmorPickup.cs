using UnityEngine;
using System.Collections;

public class ArmorPickup : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) other.GetComponent<stats>().SetArmor(100);
    }
}

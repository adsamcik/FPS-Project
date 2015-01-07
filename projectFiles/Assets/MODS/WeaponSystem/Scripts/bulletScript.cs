using UnityEngine;
using System.Collections;

public class bulletScript : MonoBehaviour {

    public DamageType DamageType;
    public GameObject damager;
    void Start() {
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger) return;
        if (other.CompareTag("Player") || other.CompareTag("AI")) other.GetComponent<Stats>().dealDamage(DamageType,damager);
        Destroy(gameObject);
    }
}

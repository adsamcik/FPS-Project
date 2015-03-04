using UnityEngine;
using System.Collections;

public class Dummy : MonoBehaviour {

    public float speed = 1500;
    public GameObject bullet;
	// Use this for initialization
	void Start () {
        StartCoroutine("shoot");
	}
	

    IEnumerator shoot() {
        while (true)
        {
            new Ray(transform.position, Vector3.forward);
            GameObject b = (GameObject)Instantiate(bullet, transform.position + transform.forward, transform.rotation);
            b.GetComponent<Rigidbody>().AddForce(transform.forward * speed);
            yield return new WaitForSeconds(1f);
        }
    }

}

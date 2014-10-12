using UnityEngine;
using System.Collections;

public class rotate : MonoBehaviour {

    public Vector3 Rotation;
	void Update () {
	    transform.Rotate(Rotation);
	}
}

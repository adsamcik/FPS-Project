using UnityEngine;
using System.Collections;

public class FPSLimiter : MonoBehaviour {
    public int TargetFrameRate = 100;
	void Start () {
        Application.targetFrameRate = TargetFrameRate;
	}
	
}

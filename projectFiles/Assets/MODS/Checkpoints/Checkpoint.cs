using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

    public Vector3 RespawnLocation;
    public float Direction;

    float maxLight;

    void Start() {
        collider.enabled = false;
        RespawnLocation = transform.Find("Respawn").position;
        if (particleSystem) particleSystem.enableEmission = false;
        if (light) { maxLight = light.intensity; light.intensity= 0; }
    }

    public void Activate()
    {
        StartCoroutine("FadeLight");
        if (particleSystem) particleSystem.enableEmission = true;
        collider.enabled = true;
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player")
        {
            transform.parent.GetComponent<CheckpointSystem>().CrossedCheckpoint();
            collider.enabled = false;
            if (particleSystem) particleSystem.enableEmission = false;
            StartCoroutine("FadeLight");
        }
    }

    IEnumerator FadeLight()
    {
        if (light != null)
        {
            if (maxLight == light.intensity)
            {
                while (light.intensity != 0)
                {
                    light.intensity -= maxLight * (Time.deltaTime / particleSystem.startLifetime*2);
                    if (light.intensity < 0) light.intensity = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (light.intensity != maxLight)
                {
                    light.intensity += maxLight * Time.deltaTime;
                    if (light.intensity > maxLight) light.intensity = maxLight;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

    }
}

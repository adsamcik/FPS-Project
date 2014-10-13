using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

    public Vector3 RespawnLocation;
    public float Direction;

    float maxLight;

    ParticleSystem.Particle[] ParticleList;
    Vector3 target;
    float fade;
    bool slow;

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
            StartCoroutine("ParticleAnimation",other.gameObject);
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

    IEnumerator ParticleAnimation(GameObject target)
    {
        fade = 0;
        ParticleList = new ParticleSystem.Particle[particleSystem.particleCount - 1];
        while (fade != 1)
        {
            particleSystem.GetParticles(ParticleList);
            fade = ((fade + Time.deltaTime) > 1) ? fade = 1f : fade += Time.deltaTime/(particleSystem.startLifetime/2);
            for (int i = 0; i < ParticleList.Length; ++i)
            {
                ParticleList[i].velocity = -(new Vector3(target.transform.position.x - ParticleList[i].position.x, ParticleList[i].velocity.y, target.transform.position.z - ParticleList[i].position.z));
                ParticleList[i].lifetime -= 2*Time.deltaTime;
            }

            particleSystem.SetParticles(ParticleList, particleSystem.particleCount);
            yield return new WaitForEndOfFrame();

        }

        ParticleList = null;
        particleSystem.SetParticles(ParticleList, 0);
    }

}

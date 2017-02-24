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
        GetComponent<Collider>().enabled = false;
        RespawnLocation = transform.Find("Respawn").position;
        if (GetComponent<ParticleSystem>()) GetComponent<ParticleSystem>().enableEmission = false;
        if (GetComponent<Light>()) { maxLight = GetComponent<Light>().intensity; GetComponent<Light>().intensity= 0; }
    }

    public void Activate()
    {
        StartCoroutine("FadeLight");
        if (GetComponent<ParticleSystem>()) GetComponent<ParticleSystem>().enableEmission = true;
        GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player")
        {
            transform.parent.GetComponent<CheckpointSystem>().CrossedCheckpoint();
            GetComponent<Collider>().enabled = false;
            if (GetComponent<ParticleSystem>()) GetComponent<ParticleSystem>().enableEmission = false;
            StartCoroutine("FadeLight");
            StartCoroutine("ParticleAnimation",other.gameObject);
        }
    }




    IEnumerator FadeLight()
    {
        if (GetComponent<Light>() != null)
        {
            if (maxLight == GetComponent<Light>().intensity)
            {
                while (GetComponent<Light>().intensity != 0)
                {
                    GetComponent<Light>().intensity -= maxLight * (Time.deltaTime / GetComponent<ParticleSystem>().startLifetime*2);
                    if (GetComponent<Light>().intensity < 0) GetComponent<Light>().intensity = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (GetComponent<Light>().intensity != maxLight)
                {
                    GetComponent<Light>().intensity += maxLight * Time.deltaTime;
                    if (GetComponent<Light>().intensity > maxLight) GetComponent<Light>().intensity = maxLight;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

    }

    IEnumerator ParticleAnimation(GameObject target)
    {
        fade = 0;
        ParticleList = new ParticleSystem.Particle[GetComponent<ParticleSystem>().particleCount - 1];
        while (fade != 1)
        {
            GetComponent<ParticleSystem>().GetParticles(ParticleList);
            fade = ((fade + Time.deltaTime) > 1) ? fade = 1f : fade += Time.deltaTime/(GetComponent<ParticleSystem>().startLifetime/2);
            for (int i = 0; i < ParticleList.Length; ++i)
            {
                ParticleList[i].velocity = -(new Vector3(target.transform.position.x - ParticleList[i].position.x, ParticleList[i].velocity.y, target.transform.position.z - ParticleList[i].position.z));
                ParticleList[i].remainingLifetime -= 2*Time.deltaTime;
            }

            GetComponent<ParticleSystem>().SetParticles(ParticleList, GetComponent<ParticleSystem>().particleCount);
            yield return new WaitForEndOfFrame();

        }

        ParticleList = null;
        GetComponent<ParticleSystem>().SetParticles(ParticleList, 0);
    }

}

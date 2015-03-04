using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointSystem : MonoBehaviour {

    List<Checkpoint> Checkpoints = new List<Checkpoint>();
    public int CurrentCheckpoint {get; private set;}
    Vector3 RespawnLocation;

	void Start () {

        if (GetComponent<RunGame>()) Checkpoints.Add(transform.Find("Start").GetComponent<Checkpoint>());
        int i = 1;
        while (true)
        {
            if (transform.Find(i.ToString()) == null) break;
            Checkpoints.Add(transform.Find(i.ToString()).GetComponent<Checkpoint>());
            i++;
        }

        if (GetComponent<RunGame>()) Checkpoints.Add(transform.Find("Finish").GetComponent<Checkpoint>());
        RespawnLocation = GameObject.FindGameObjectWithTag("Player").transform.position;

       Checkpoints[0].Activate();
	}

    public void RestartGame()
    {
        if (CurrentCheckpoint != Checkpoints.Count)
        {
            Checkpoints[CurrentCheckpoint].GetComponent<Collider>().enabled = false;
            Checkpoints[CurrentCheckpoint].GetComponent<ParticleSystem>().enableEmission = false;
        }
        CurrentCheckpoint = 0;
        Checkpoints[0].Activate();

        RespawnLocation = Checkpoints[0].RespawnLocation;
    }

    public void RestartCheckpoint()
    {
        Respawn(GameObject.Find("character"));
    }

    public void CrossedCheckpoint()
    {
        RespawnLocation = Checkpoints[CurrentCheckpoint].RespawnLocation;
        if (++CurrentCheckpoint == Checkpoints.Count) RestartGame();
        Checkpoints[CurrentCheckpoint].Activate();
    }

    public void Respawn(GameObject player)
    {
        player.transform.position = RespawnLocation;
        //player.GetComponent<SimpleMouseRotator>().setTargetAngle(Direction);
    }
}

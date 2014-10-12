using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RunGame : MonoBehaviour {
    public Text StopWatchUI, CTime, TAB, RTime, NewRecord;

    GameObject RecordTab;

    bool RunInProgress;

    float time;

    float rDown;

    float Highscore;

    

	void Start () {
        if (RecordTab = GameObject.Find("HUD/Records"))
        {
            CTime = GameObject.Find("HUD/Records/CTime").GetComponent<Text>();
            RTime = GameObject.Find("HUD/Records/RTime").GetComponent<Text>();
            TAB = GameObject.Find("HUD/Records/TAB").GetComponent<Text>();
            NewRecord = GameObject.Find("HUD/Records/New Record").GetComponent<Text>();
            TAB.gameObject.SetActive(false);
            NewRecord.gameObject.SetActive(false);
            CTime.text = "0";
            RTime.text = (Highscore.ToString().Length > 4) ? Highscore.ToString().Remove(4) : Highscore.ToString();
            RecordTab.SetActive(false);
        }
        StopWatchUI = GameObject.Find("HUD/Stopwatch").GetComponent<Text>();


        Highscore = LoadHighScore();

	}

    void Update()
    {
        if (RunInProgress)
        {
            time += Time.deltaTime;
            if (time.ToString().Length > 4) StopWatchUI.text = time.ToString().Remove(Mathf.RoundToInt(time).ToString().Length + 2);
            else StopWatchUI.text = time.ToString();
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (rDown < 2) { GetComponent<CheckpointSystem>().RestartCheckpoint(); rDown += Time.deltaTime; }
            else RestartGame();
        }
        else rDown = 0.00f;

        if (Input.GetKeyDown(KeyCode.Tab)) RecordTab.SetActive(true);
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            RecordTab.SetActive(false); 
            TAB.gameObject.SetActive(false);
            NewRecord.gameObject.SetActive(false);
        }
    }

    void RestartGame()
    {
        RunInProgress = false;
        time = 0;
        StopWatchUI.text = time.ToString();
        GetComponent<CheckpointSystem>().RestartGame();
        GetComponent<CheckpointSystem>().RestartCheckpoint();
    }

    float LoadHighScore()
    {
        return PlayerPrefs.GetFloat("RunGameTime1");
    }

    void SaveHighScore() {
        PlayerPrefs.SetFloat("RunGameTime1", this.Highscore);
        PlayerPrefs.Save();
    }

    public void StartOrFinish() {
        if (RunInProgress) Finished();
        RunInProgress = !RunInProgress;
    }

    void Finished() {
        if (time < Highscore || Highscore == 0) { 
            Highscore = time;
            SaveHighScore();
            NewRecord.gameObject.SetActive(true);
        }

        RTime.text = (Highscore.ToString().Length > 4) ? Highscore.ToString().Remove(4) : Highscore.ToString();
        CTime.text = (time.ToString().Length > 4) ? time.ToString().Remove(4) : time.ToString();

        TAB.gameObject.SetActive(true);

        RecordTab.SetActive(true);

        time = 0;

        GetComponent<CheckpointSystem>().RestartGame();
    }
}

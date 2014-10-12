using UnityEngine;
using System.Collections;

public class RealmSwitch : MonoBehaviour {

    bool knowsRed;
    bool knowsGreen;
    bool knowsBlue;

	void Start () {
        GameObject.Find("Map").GetComponent<RealmController>().Initialize(false, false, false);
	}

    public void LearnColor(Phase color)
    {
        switch ((int)color)
        {
            case 0:
                API.SetBool("red", true);
                GameObject.Find("Map").GetComponent<RealmController>().Initialize(API.GetBool("red"), API.GetBool("green"), API.GetBool("blue"));
                break;
            case 1:
                API.SetBool("green", true);
                GameObject.Find("Map").GetComponent<RealmController>().Initialize(API.GetBool("red"), API.GetBool("green"), API.GetBool("blue"));
                break;
            case 2:
                API.SetBool("blue", true);
                GameObject.Find("Map").GetComponent<RealmController>().Initialize(API.GetBool("red"), API.GetBool("green"), API.GetBool("blue"));
                break;
        }
        PlayerPrefs.Save();
    }
}

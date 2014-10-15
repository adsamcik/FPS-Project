using UnityEngine;
using System.Collections;


public delegate void voidSwitch();
public static class API {
    public static void SetBool(string name, bool booleanValue)
    {
        PlayerPrefs.SetInt(name, booleanValue ? 1 : 0);
    }

    public static bool GetBool(string name)
    {
        return PlayerPrefs.GetInt(name) == 1 ? true : false;
    }

    public static bool GetBool(string name, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(name))
        {
            return GetBool(name);
        }

        return defaultValue;
    }

    public static void savePosition(Vector3 position)
    {
        PlayerPrefs.SetFloat(Application.loadedLevelName + "x", position.x);
        PlayerPrefs.SetFloat(Application.loadedLevelName + "y", position.y);
        PlayerPrefs.SetFloat(Application.loadedLevelName + "z", position.z);
        PlayerPrefs.Save();
    }

    public static void saveCheckpoint(int checkpoint)
    {
        PlayerPrefs.SetInt(Application.loadedLevelName + "check", checkpoint);
        PlayerPrefs.Save();
    }

    public static int loadCheckpoint()
    {
        return PlayerPrefs.GetInt(Application.loadedLevelName + "check");
    }

    public static Vector3 loadPosition()
    {
        Vector3 position;
        position.x = PlayerPrefs.GetFloat(Application.loadedLevelName + "x");
        position.y = PlayerPrefs.GetFloat(Application.loadedLevelName + "y");
        position.z = PlayerPrefs.GetFloat(Application.loadedLevelName + "z");
        return position;
    }
}

public enum DamageType
{
    fall = 1,
    fire = 2,
    melee = 3,
    bullet = 4,
    piercingBullet = 5
}

public enum Phase
{
    Red = 0,
    Green = 1,
    Blue = 2
}

[System.Serializable]
public class CapacityOf {
    public int cur;
    public int max;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cur">current</param>
    /// <param name="max">max</param>
    public CapacityOf(int cur, int max) {
        if (cur > max) { Debug.LogWarning("You can't have more than max!"); cur = max; }
        this.cur = cur;
        this.max = max;
    }
}

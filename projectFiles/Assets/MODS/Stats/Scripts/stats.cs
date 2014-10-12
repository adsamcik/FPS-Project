using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class stats : MonoBehaviour
{
    public class pStats
    {
        public int threatValue { get; private set; }
        public int estHealth { get; private set; }
        public bool hasArmor { get; private set; }
        public pStats(float health, float armor)
        {
            updateStats(health, armor);
            estimateThreat();
        }
        public int estimateThreat()
        {
            threatValue += (hasArmor) ? 0 : +20;
            threatValue += 30 - (estHealth * 10);
            return threatValue;
        }

        public void updateStatsHealth(float health) { estHealth = estimateHealth(health); estimateThreat(); }
        public void updateStatsArmor(float armor) { hasArmor = checkForArmor(armor); estimateThreat(); }
        public void updateStats(float health, float armor) { estHealth = estimateHealth(health); hasArmor = checkForArmor(armor); estimateThreat(); }

        public int estimateHealth(float Health) //0 Dead, 1 low Health, 2 med Health, 3 high health, 4 extra high health
        {
            if (Health <= 0) return 0;
            else if (Health < 25) return 1;
            else if (Health < 60) return 2;
            else if (Health < 100) return 3;
            else return 4;
        }

        public bool checkForArmor(float Armor)
        {
            return (Armor > 0) ? true : false;
        }

    }

    public class Buff
    {
        byte MaxStacks;
        int HealthBoost;
        float length;

    }

    //public variables visible in Inspector
    public bool enableHealth, enableArmor, enableRegeneration;
    public int MaxHealth = 100;
    public int Regen = 1;
    public float LeaveCombatAfter = 10;
    public CheckpointSystem CheckSys;

    //public variables not visible in Inspector
    public pStats publicStats;
    public delegate void voidSwitch();

    //private variables
    bool inCombat;
    float lasthit, Health, Armor;

    //delegates
    voidSwitch updt;
    voidSwitch kill;

    //GUI
    Image HealthBar;
    Image ArmorBar;

    // PLAYER SPECIFIC FUNCTIONS
    void playerUpdate()
    {
        if (transform.position.y < -5) kill();
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (enableRegeneration && !inCombat && Health < MaxHealth) { health(Regen * Time.deltaTime); if (Health > MaxHealth) Health = MaxHealth; }
    }

    void playerKill()
    {
        CheckSys.Respawn(transform.gameObject);
        SetHealth(MaxHealth);
        SetArmor(0);
    }

    //AI SPECIFIC FUNCTIONS
    void aiUpdate()
    {
        if (transform.position.y < -5) kill();
        if (enableRegeneration && !inCombat && Health < MaxHealth) { Health += Regen * Time.deltaTime; if (Health > MaxHealth) Health = MaxHealth; }
    }

    void aiKill()
    {
        Destroy(transform.gameObject);
    }

    void aiHealthArmor(float hp, float arm = 0f)
    {
        if (hp != 0)
        {
            Health += hp;
            //HealthBar.rectTransform.localScale = new Vector3((float)Health / 100, 1, 1);
        }
        if (Armor != 0)
        {
            Armor += arm;
            //ArmorBar.rectTransform.localScale = new Vector3((float)Armor / 100, 1, 1);
        }
    }


    //UNIVERSAL FUNCTIONS
    void Start() //INITIALIZATION
    {
        Health = MaxHealth;
        publicStats = new pStats(Health, Armor);

        if (gameObject.CompareTag("Player"))
        {
            if (enableHealth) HealthBar = GameObject.Find("--HUD/Stats/healthBar").GetComponent<Image>();
            if (enableArmor) ArmorBar = GameObject.Find("--HUD/Stats/armorBar").GetComponent<Image>();
            HealthBar.rectTransform.localScale = new Vector3((float)Health / 100, 1, 1);
            ArmorBar.rectTransform.localScale = new Vector3((float)Armor / 100, 1, 1);
            kill = playerKill;
            updt = playerUpdate;
        }
        else
        {
            kill = aiKill;
            updt = aiUpdate;
        }
    }
    void Update() { updt(); }

    void leaveCombat()
    {
        inCombat = false;
    }

    /// <summary>
    /// add health and armor(optional and can only subtract)
    /// </summary>
    /// <param name="hp">health</param>
    /// <param name="arm">armor</param>
    void health(float hp)
    {
        if (hp != 0)
        {
            float targetHP = Health += hp;
            Health = (targetHP > MaxHealth) ? MaxHealth : targetHP;
            if (HealthBar != null) HealthBar.rectTransform.localScale = new Vector3((float)Health / 100, 1, 1);
        }
    }

    public void SetHealth(float hp)
    {
        Health = hp;
        if (HealthBar != null) HealthBar.rectTransform.localScale = new Vector3((float)Health / 100, 1, 1);
    }

    public void SetArmor(float armor)
    {
        Armor = armor;
        if (ArmorBar != null) ArmorBar.rectTransform.localScale = new Vector3((float)Armor / 100, 1, 1);
    }

    public void dealDamage(DamageType dtype)
    {
        inCombat = true;
        switch ((int)dtype)
        {
            case 1: //Fall
                health(-Mathf.RoundToInt(rigidbody.velocity.y * 2));
                break;
            case 2: //Fire
                health(-5);
                break;
            case 3: //Melee
                health(-25);
                break;
            case 4: //bullet
                health(-armorReduction(25));
                break;
            case 5: //Piercing bullet
                health(-armorReduction(30, 20));
                break;
        }
        CancelInvoke();
        if (Health <= 0) kill();
        else Invoke("leaveCombat", LeaveCombatAfter);
    }

    public void dealDamage(DamageType dtype, GameObject damager)
    {
        if (damager == gameObject) return; //temporary
        inCombat = true;
        switch ((int)dtype)
        {
            case 1: //Fall
                health(-Mathf.RoundToInt(rigidbody.velocity.y * 2));
                break;
            case 2: //Fire
                health(-5);
                break;
            case 3: //Melee
                health(-25);
                break;
            case 4: //bullet
                health(-armorReduction(25));
                break;
            case 5: //Piercing bullet
                health(-armorReduction(30, 20));
                break;
        }
        CancelInvoke();
        if (Health <= 0) kill();
        else Invoke("leaveCombat", LeaveCombatAfter);

        if (gameObject.CompareTag("AI")) GetComponent<AI>().shotMe(damager);
    }

    int armorReduction(int Base, int armorCost = 10)
    {
        if (!enableArmor || Armor == 0) return Base;

        Base -= Mathf.RoundToInt(Base * ((float)Armor / 100));

        Armor -= (Armor < armorCost) ? Armor : armorCost;
        if (ArmorBar != null) ArmorBar.rectTransform.localScale = new Vector3((float)Armor / 100, 1, 1);
        Debug.Log(ArmorBar);

        return Base;
    }

    public void CountFall() { 
    
    }

}

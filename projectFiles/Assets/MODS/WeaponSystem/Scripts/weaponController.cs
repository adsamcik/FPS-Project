using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class weaponController : MonoBehaviour
{
    List<WeaponInfo> weapons = new List<WeaponInfo>();

    public bool isPlayer {get; private set;}
    bool WeaponHidden;

    int cW; //Current weapon

    Text Text;

    public delegate Vector3 vectorSwitch();
    public vectorSwitch getAimPoint;

    void Awake()
    {
        GameObject Weapons;
        Transform checkChild;
        if ((checkChild = transform.FindChild("weapons")) == null)
        {
            Weapons = new GameObject();
            Weapons.transform.parent = transform;
            Weapons.name = "weapons";
        }
        else Weapons = checkChild.gameObject;

        Transform[] hasWeapons = Weapons.GetComponentsInChildren<Transform>();
        for (int i = 1; i < hasWeapons.Length; i++)
        {
            weapons.Add(new WeaponInfo(hasWeapons[i].gameObject));
        }

        //Important initialization. Defines how will weapon controller work
        if (gameObject.CompareTag("Player"))
        {
            isPlayer = true;
            getAimPoint = PlayerGetAimPoint;
        }
        else
        {
            isPlayer = false;
            getAimPoint = AIGetAimPoint;
        }

        ChangeWeapon(0);
    }

    public void newWeapon(GameObject w) {
        if (w.transform.parent == transform) {
            weapons.Add(new WeaponInfo(w));
            w.transform.parent = transform.FindChild("weapons");
            w.transform.localPosition = Vector3.zero;
        }
    }

    void ChangeWeapon(int id)
    {
        if(id < weapons.Count) cW = id;
    }

    void Update()
    {
        if (weapons.Count > 0)
        {
            playerInput();
            weapons[cW].weapon.Update();
        }
    }

    public void CurrentWeaponAttack()
    {
        if (weapons.Count > 0)
        {
            weapons[cW].weapon.Attack();
        }
    }

    void playerInput()
    {
        if (!isPlayer) return;
        if (Input.GetMouseButton(0)) CurrentWeaponAttack();
        else if (Input.GetKeyDown(KeyCode.R)) weapons[cW].weapon.RAction();
        else if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeWeapon(1);
    }

    public void displaytext(string text)
    {
        StopCoroutine("DisplayText");
        StartCoroutine("DisplayText", text);
    }

    IEnumerator DisplayText(string text)
    {
        //GUIText.text = text;
        yield return new WaitForSeconds(1f);
        //GUIText.text = "";
    }
    Vector3 PlayerGetAimPoint()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return ray.GetPoint(5);
    }

    Vector3 AIGetAimPoint()
    {
        return gameObject.GetComponent<AI>().threats[0].gameObject.transform.position - weapons[cW].weapon.hitPoint;
    }

    [System.Serializable]
    class WeaponInfo {
        public GameObject gameObject;
        public Weapon weapon;

        public WeaponInfo(GameObject g) {
            if ((weapon = g.GetComponent<melee>()) == null) weapon = g.GetComponent<ranged>();
            gameObject = g;
        }

        public void Update() {
            weapon.Update();
        }
    }
}

[System.Serializable]
public class Weapon : MonoBehaviour
{
    [HideInInspector]
    protected GameObject gameObject;
    [HideInInspector]
    protected Transform transform;
    [HideInInspector]
    protected weaponController wC;
    [HideInInspector]
    public Vector3 hitPoint;
    public GameObject model;

    public string name = "Weapon";
    public DamageType damageType;
    protected float weaponRange = -1;

    protected float Break;

    protected virtual void Start()
    {
        transform = GetComponent<Transform>();
        gameObject = transform.gameObject;

        transform.localPosition = Vector3.forward;
        gameObject.name = name;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && (other.CompareTag("Player") || other.CompareTag("AI")))
        {
            transform.parent = other.transform;
            wC = transform.parent.GetComponent<weaponController>();
            wC.newWeapon(gameObject);
            Destroy(GetComponent<BoxCollider>());
            GetComponent<Text>().text = "";
        }
    }

    public void Drop()
    {
        //gameObject.AddComponent<Rigidbody>();
        BoxCollider b = gameObject.AddComponent<BoxCollider>();
        RectTransform rt = GetComponent<RectTransform>();
        b.size = new Vector3(rt.sizeDelta.x, 1, rt.sizeDelta.y);
        b.isTrigger = true;
        GetComponent<Text>().text = name;
    }

    public virtual void Update()
    {
        Break -= (Break > 0) ? Time.deltaTime : 0;
    }

    public virtual void RAction() { }

    public virtual void Attack() { }
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class weaponController : MonoBehaviour {
    List<WeaponInfo> weapons = new List<WeaponInfo>();

    public bool isPlayer { get; private set; }
    bool WeaponHidden;

    int cW; //Current weapon

    Text Text;

    public delegate Vector3 vectorSwitch();
    public vectorSwitch getAimPoint;

    void Awake() {
        //Important initialization. Defines how will weapon controller work
        if (gameObject.CompareTag("Player")) {
            isPlayer = true;
            getAimPoint = PlayerGetAimPoint;
        }
        else {
            isPlayer = false;
            getAimPoint = AIGetAimPoint;
        }

        ChangeWeapon(0);
    }

    public void newWeapon(GameObject w) {
        weapons.Add(new WeaponInfo(w));
        w.transform.parent = transform.FindChild("Camera/weapons");
        w.transform.localPosition = Vector3.zero;
    }

    void ChangeWeapon(int id) {
        if (id < weapons.Count) {
            weapons[cW].weapon.model.SetActive(false);
            cW = id;
            weapons[id].weapon.Display();
            weapons[id].weapon.model.SetActive(true);
        }
    }

    void Update() {
        if (weapons.Count > 0) {
            playerInput();
            weapons[cW].weapon.Update();
        }
        Debug.Log(weapons.Count);
    }

    public void CurrentWeaponAttack() {
        if (weapons.Count > 0) {
            weapons[cW].weapon.Attack();
        }
    }

    void playerInput() {
        if (!isPlayer) return;
        if (Input.GetMouseButton(0)) CurrentWeaponAttack();
        else if (Input.GetKeyDown(KeyCode.R)) weapons[cW].weapon.RAction();
        else if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeWeapon(1);
    }

    public void displaytext(string text) {
        StopCoroutine("DisplayText");
        StartCoroutine("DisplayText", text);
    }

    IEnumerator DisplayText(string text) {
        //GUIText.text = text;
        yield return new WaitForSeconds(1f);
        //GUIText.text = "";
    }
    Vector3 PlayerGetAimPoint() {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit)) {
            return hit.point;
        }

        return ray.GetPoint(5);
    }

    Vector3 AIGetAimPoint() {
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
public class Weapon : MonoBehaviour {
    [HideInInspector]
    protected GameObject gameObject;
    [HideInInspector]
    protected Transform transform;
    [HideInInspector]
    protected weaponController wC;
    [HideInInspector]
    public Vector3 hitPoint;

    public GameObject model { get; private set; }
    protected Transform shootPoint;
    protected Text dropText;

    public string name = "Weapon";
    public DamageType damageType;
    public float weaponRange = -1;

    protected float Break;

    protected virtual void Awake() {
        transform = GetComponent<Transform>();
        gameObject = transform.gameObject;
        gameObject.name = name;
        dropText = GetComponentInChildren<Text>();

        model = transform.Find("model").gameObject;
        model.SetActive(false);

        shootPoint = transform.Find("model/shootPoint");

        try {
            PickUp(transform.parent.parent.parent);
        }
        catch {
            Drop();
        }
    }

    protected void OnTriggerEnter(Collider other) {
        if (!other.isTrigger) {
            PickUp(other.transform);
        }
    }

    protected void PickUp(Transform whom) {
        if (whom.CompareTag("AI") || whom.CompareTag("Player")) {
            transform.parent = whom.Find("Camera/weapons");
            wC = whom.GetComponent<weaponController>();
            wC.newWeapon(gameObject);
            Destroy(GetComponent<BoxCollider>());
            dropText.text = "";
        }
    }

    public void Drop() {
        //gameObject.AddComponent<Rigidbody>();
        BoxCollider b = gameObject.AddComponent<BoxCollider>();
        RectTransform rt = GetComponentInChildren<RectTransform>();
        b.size = new Vector3(0.1f, 1, 2);
        b.isTrigger = true;
        dropText.text = name;
        transform.parent = null;
    }

    public virtual void Update() {
        Break -= (Break > 0) ? Time.deltaTime : 0;
    }

    public virtual void Display() {

    }

    public virtual void RAction() { }

    public virtual void Attack() { }
}
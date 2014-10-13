using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class weaponController : MonoBehaviour {
    [System.Serializable]
    public class Weapon
    {
        public string name = "Weapon";
        public float speed = 1000;
        public int MagSize = 8;
        public int BulletCapacity = 40;
        public float ReloadSpeed = 1;
        public float RateOfFire = 3f;
        public int MagLeft;
        public int BulLeft;

        public bool physicalBullet; //Uses physical bullets? (more performance required)
        public GameObject bullet; //Required only if usesBullet is true
        public DamageType damageType;

        public Weapon() {
            if(physicalBullet && bullet == null) bullet = (GameObject)Resources.Load("Bullet");
        } 
    }

    public Weapon[] weapons = new Weapon[2];

    float Break;
    bool Reloading;
    bool WeaponHidden;

    int cW; //Current weapon

    Vector3 gunHead;
    Text Text;

    public delegate Vector3 vectorSwitch();
    public delegate void voidSwitch();
    vectorSwitch getAimPoint;
    voidSwitch input;
    voidSwitch fire;

	void Start () {
        //Important initialization. Defines how will weapon controller work
        if (gameObject.CompareTag("Player"))
        {
            getAimPoint = PlayerGetAimPoint;
            input = playerInput;
        }
        else {
            getAimPoint = AIGetAimPoint;
            input = aiInput;
        }
        ChangeWeapon(0);
        Transform gunHeadObject = transform.Find("Head Joint/First Person Camera/Gun/GunHead");
        gunHead = (gunHeadObject != null) ? gunHeadObject.localPosition : transform.forward;
	}

    void ChangeWeapon(int id) { 
        cW = id;
        if (weapons[cW].physicalBullet) fire = physicalBullet; else fire = bullet;
    }
	
	void Update () {
        Break -= (Break > 0) ? Time.deltaTime : 0;
        input(); 
	}

    void playerInput() {
        if (Input.GetMouseButton(0)) Shoot();
        else if (Input.GetKeyDown(KeyCode.R)) reload();
        else if (Input.GetKeyDown(KeyCode.T)) RefreshAmmo();
    }

    void aiInput()
    {
    }

    public void Shoot()
    {
        if (Reloading || Break > 0) return;
        if (weapons[cW].MagLeft == 0) {displaytext("Out of Ammo"); return;}

        Break = 1 / weapons[cW].RateOfFire;
        weapons[cW].MagLeft--;

        fire();

        if (weapons[cW].MagLeft == 0) reload();
    }

    void physicalBullet() {
        Vector3 aimTo;
        GameObject b = (GameObject)Instantiate(weapons[cW].bullet, transform.position + transform.forward, Quaternion.LookRotation(aimTo = getAimPoint() - transform.position));
        b.rigidbody.AddForce(aimTo.normalized * weapons[cW].speed);
        b.GetComponent<bulletScript>().damager = gameObject;
    }

    void bullet() {
        RaycastHit hit;
        Vector3 position = transform.position;
        //Vector3 position = transform.position + transform.forward; Currently causes issues with raycast due to raycast ignore
        Debug.DrawRay(position, getAimPoint() - position);
        Vector3 direction = getAimPoint() - position;

        int oldLayer = gameObject.layer;

        //Change object layer to a layer it will be alone
        gameObject.layer = LayerMask.NameToLayer("Shooter");

        int layerToIgnore = 1 << gameObject.layer;
        layerToIgnore = ~layerToIgnore;
        if (Physics.Raycast(position, direction, out hit, 100, layerToIgnore) && (hit.collider.CompareTag("Player") || hit.collider.CompareTag("AI"))) hit.transform.GetComponent<stats>().dealDamage(weapons[cW].damageType, gameObject);
        gameObject.layer = oldLayer;
        Debug.Log(hit.collider);
    }


    void reload() {
        if (weapons[cW].BulLeft > 0 && weapons[cW].MagLeft != weapons[cW].MagSize)
        {
            Break = weapons[cW].ReloadSpeed;
            int bNeed = weapons[cW].MagSize - weapons[cW].MagLeft;
            weapons[cW].MagLeft += (weapons[cW].BulLeft - bNeed > 0) ? bNeed : bNeed = weapons[cW].BulLeft;
            weapons[cW].BulLeft -= bNeed;
            //if (weapons[cW].BulLeft < 8) displaytext("Last Magazine");
        }
    }

    void displaytext(string text) {
        StopCoroutine("DisplayText");
        StartCoroutine("DisplayText", text);
    }

    IEnumerator DisplayText(string text) {
        //GUIText.text = text;
        yield return new WaitForSeconds(1f);
        //GUIText.text = "";
    }

    IEnumerator Reload() {
        displaytext("Reloading");
        Reloading = true;
        yield return new WaitForSeconds(weapons[cW].ReloadSpeed);
        Reloading = false;
        int bNeed = weapons[cW].MagSize - weapons[cW].MagLeft;
        weapons[cW].MagLeft += (weapons[cW].BulLeft - bNeed > 0) ? bNeed : bNeed = weapons[cW].BulLeft;
        weapons[cW].BulLeft -= bNeed;
        if (weapons[cW].BulLeft < 8) displaytext("Last Magazine");
    }

    void RefreshAmmo() { 
        //Debug only
        weapons[cW].BulLeft = weapons[cW].BulletCapacity;
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
        return GetComponent<AI>().threats[0].gameObject.transform.position - gunHead;
    }
}

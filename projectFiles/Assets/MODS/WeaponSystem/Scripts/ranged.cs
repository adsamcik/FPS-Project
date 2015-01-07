using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ranged : Weapon {
    bool dropped;
    public float speed = 1000;

    public float ReloadTime = 1;
    /// <summary>
    /// rounds per second
    /// </summary>
    public float RateOfFire = 3f;

    public CapacityOf magazine = new CapacityOf(8, 8);
    public CapacityOf bullets = new CapacityOf(8, 64);

    public bool physicalBullet; //Uses physical bullets? (more performance required)
    public GameObject bullet; //Required only if usesBullet is true

    voidSwitch bulletFunction;

    Text ammoText;

    override protected void Awake() {
        base.Awake();

        if (physicalBullet) bulletFunction = PhysicalBulletAttack; else bulletFunction = BulletAttack;

        ammoText = GameObject.Find("--HUD/Ammo/ammoText").GetComponent<Text>();
        Display();
    }

    public override void Attack() {
        if (Break > 0) return;
        if (!wC.isPlayer && magazine.cur == 0) RAction();
        else if (CheckBullets()) bulletFunction();
        Display();
    }

    bool CheckBullets() {
        if (magazine.cur == 0) {
            if (bullets.cur == 0) wC.displaytext("Out of Ammo");
            else wC.displaytext("press R to reload");
            return false;
        }

        Break = 1 / RateOfFire;
        magazine.cur--;
        Display();
        return true;
    }

    public override void Display() {

        if (wC != null && wC.isPlayer) ammoText.text = magazine.cur + "/" + (bullets.cur + magazine.cur);
    }

    public override void RAction() {
        if (Break > 0) return;
        if (bullets.cur > 0 && magazine.cur != magazine.max) {
            Break = ReloadTime;
            int bNeed = magazine.max - magazine.cur;
            magazine.cur += (bullets.cur - bNeed > 0) ? bNeed : bNeed = bullets.cur;
            bullets.cur -= bNeed;
            Display();
        }
    }

    void PhysicalBulletAttack() {
        Vector3 aimTo;
        GameObject b = (GameObject)Instantiate(bullet, shootPoint.position, Quaternion.LookRotation(aimTo = wC.getAimPoint() - transform.position));
        b.rigidbody.AddForce(aimTo.normalized * speed);
        b.GetComponent<bulletScript>().damager = gameObject;
    }

    void BulletAttack() {
        RaycastHit hit;
        //Vector3 position = transform.position + transform.forward; Currently causes issues with raycast due to raycast ignore
        Debug.DrawRay(shootPoint.position, wC.getAimPoint() - shootPoint.position);
        Vector3 direction = wC.getAimPoint() - shootPoint.position;

        int oldLayer = gameObject.layer;

        //Change object layer to a layer it will be alone
        gameObject.layer = LayerMask.NameToLayer("Shooter");

        int layerToIgnore = 1 << gameObject.layer;
        layerToIgnore = ~layerToIgnore;
        if (Physics.Raycast(shootPoint.position, direction, out hit, 100, layerToIgnore) && (hit.collider.CompareTag("Player") || hit.collider.CompareTag("AI"))) hit.transform.GetComponent<Stats>().dealDamage(damageType, gameObject);
        gameObject.layer = oldLayer;
    }

}

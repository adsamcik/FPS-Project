using UnityEngine;
using System.Collections;

public class ranged : Weapon
{
    public float speed = 1000;

    public float ReloadSpeed = 1;
    public float RateOfFire = 3f;

    public CapacityOf magazine = new CapacityOf(8, 8);
    public CapacityOf bullets = new CapacityOf(8, 32);

    public bool physicalBullet; //Uses physical bullets? (more performance required)
    public GameObject bullet; //Required only if usesBullet is true
    voidSwitch bulletFunction;

    bool Reloading;

    //public ranged(GameObject go)
    //{
    //    gameObject = go;
    //    transform = go.transform;
    //    wC = go.GetComponent<weaponController>();
    //    if (physicalBullet && bullet == null) bullet = (GameObject)Resources.Load("Bullet");
    //    Transform gunHeadObject = transform.Find("Head Joint/First Person Camera/Gun/GunHead");
    //    hitPoint = (gunHeadObject != null) ? gunHeadObject.localPosition : transform.forward;

    //    if (physicalBullet) bulletFunction = PhysicalBulletAttack; else bulletFunction = BulletAttack;
    //}

    //public ranged(GameObject go, GameObject bullet, CapacityOf magazine, CapacityOf bullets, float speed, float rateOfFire, float reloadSpeed, string name = "ranged")
    //{
    //    gameObject = go;
    //    transform = go.transform;
    //    wC = go.GetComponent<weaponController>();

    //    this.bullets = bullets;
    //    this.magazine = magazine;

    //    if (physicalBullet && bullet == null) bullet = (GameObject)Resources.Load("Bullet");
    //    Transform gunHeadObject = transform.Find("Head Joint/First Person Camera/Gun/GunHead");
    //    hitPoint = (gunHeadObject != null) ? gunHeadObject.localPosition : transform.forward;

    //    if (physicalBullet) bulletFunction = PhysicalBulletAttack; else bulletFunction = BulletAttack;
    //}

    void Start()
    {
        transform = GetComponent<Transform>();
        if (physicalBullet) bulletFunction = PhysicalBulletAttack; else bulletFunction = BulletAttack;
        wC = transform.parent.parent.GetComponent<weaponController>();
    }

    public override void Attack()
    {
        if (CheckBullets()) bulletFunction();

    }

    bool CheckBullets()
    {
        if (Reloading || Break > 0) return false;
        if (magazine.cur == 0)
        {
            if (bullets.cur == 0) wC.displaytext("Out of Ammo");
            else wC.displaytext("press R to reload");
            return false;
        }

        Break = 1 / RateOfFire;
        magazine.cur--;

        return true;
    }

    void RefreshAmmo()
    {
        //Debug only

    }

    public override void RAction()
    {
        if (bullets.cur > 0 && magazine.cur != magazine.max)
        {
            Break = ReloadSpeed;
            int bNeed = magazine.max - magazine.cur;
            magazine.cur += (bullets.cur - bNeed > 0) ? bNeed : bNeed = bullets.cur;
            bullets.cur -= bNeed;
            //if (bullets.x < 8) displaytext("Last Magazine");
        }
    }

    IEnumerator Reload()
    {
        wC.displaytext("Reloading");
        Reloading = true;
        yield return new WaitForSeconds(ReloadSpeed);
        Reloading = false;
        int bNeed = magazine.max - magazine.cur;
        magazine.cur += (bullets.cur - bNeed > 0) ? bNeed : bNeed = bullets.cur;
        bullets.cur -= bNeed;
        if (bullets.cur < 8) wC.displaytext("Last Magazine");
    }

    void PhysicalBulletAttack()
    {
        Vector3 aimTo;
        GameObject b = (GameObject)Instantiate(bullet, transform.position + transform.forward, Quaternion.LookRotation(aimTo = wC.getAimPoint() - transform.position));
        b.rigidbody.AddForce(aimTo.normalized * speed);
        b.GetComponent<bulletScript>().damager = gameObject;
    }

    void BulletAttack()
    {
        RaycastHit hit;
        Vector3 position = transform.position;
        //Vector3 position = transform.position + transform.forward; Currently causes issues with raycast due to raycast ignore
        Debug.DrawRay(position, wC.getAimPoint() - position);
        Vector3 direction = wC.getAimPoint() - position;

        int oldLayer = gameObject.layer;

        //Change object layer to a layer it will be alone
        gameObject.layer = LayerMask.NameToLayer("Shooter");

        int layerToIgnore = 1 << gameObject.layer;
        layerToIgnore = ~layerToIgnore;
        if (Physics.Raycast(position, direction, out hit, 100, layerToIgnore) && (hit.collider.CompareTag("Player") || hit.collider.CompareTag("AI"))) hit.transform.GetComponent<stats>().dealDamage(damageType, gameObject);
        gameObject.layer = oldLayer;
    }

}

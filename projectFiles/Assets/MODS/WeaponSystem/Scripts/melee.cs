using UnityEngine;
using System.Collections;

public class melee : Weapon {
    voidSwitch hitFunction;

    float hitDistance;


    //public melee(GameObject go)
    //{
    //    gameObject = go;
    //    transform = go.transform;
    //    wC = go.GetComponent<weaponController>();
    //    hitPoint = transform.forward;

    //    //if (physicalBullet) bulletFunction = PhysicalBulletAttack; else bulletFunction = BulletAttack;
    //}

    public override void Attack() {
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

using UnityEngine;
using System.Collections;

public class melee : Weapon
{
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

    public override void Attack()
    {
        hitFunction();
    }

}

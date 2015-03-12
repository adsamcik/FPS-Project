using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class WallRunAndClimbing : MonoBehaviour {
    Rigidbody rigidbody;
    CapsuleCollider capsule;
    Camera _camera;
    RigidbodyFirstPersonController rfpc;


    public LayerMask LedgeMask;
    Stats playerStats;

    bool wallRunEnabled, climbing;
    public float maxWallRunLength = 1;
    float wallRunLength;

    delegate void WallRun();
    WallRun wallRun;
    delegate bool WallRunSide();
    WallRunSide wallRunSide;

    Vector3 localLedgePos = new Vector3(0, 1.5f, 0.7f);

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        _camera = Camera.main;
        rfpc = GetComponent<RigidbodyFirstPersonController>();
    }

    bool LedgeCheck() {
        RaycastHit lhit;
        Vector3 transformPosition = transform.position;
        //Calculates ledge position based on where you are looking. Uses 2 axes (x,z) to keep ledge always above you.
        Vector3 Ledge = new Vector3(transformPosition.x + _camera.transform.forward.x / 1.5f, transformPosition.y + 1.5f, transformPosition.z + _camera.transform.forward.z);
        Ray LedgeRay = new Ray(Ledge, -Vector3.up);
        //First pass makes general raycast if there is ledge in front of the player
        if (Physics.SphereCast(LedgeRay, capsule.radius / 2, out lhit, 1.5f, LedgeMask)) {
            Ray CeilingRay = new Ray(transform.position, Vector3.up);
            //Second pass checks if there is no ceiling above the player or ledge so player can climb
            if (!Physics.SphereCast(CeilingRay, capsule.radius, 1.5f)) {
                //Third pass detects if the ledge is not too steep
                if (Vector3.Angle(lhit.normal, Vector3.up) < 25) {
                    Ray FrontRay = new Ray(new Vector3(transform.position.x, lhit.point.y + 1, transform.position.z), _camera.transform.forward);
                    //Fourth pass checks whether there is no wall that could block the climbing
                    if (!Physics.Raycast(FrontRay, 0.3f)) {
                        //Everything seems ok, we can climb
                        Climb(lhit.point + (lhit.normal / 4));
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //bool ObstacleCheck() { 

    //}

    void Climb(Vector3 position) //This function is there to enable us call coroutine like normal function
    {
        StartCoroutine("climb", position); // needs to be coroutine due to need of using yields
    }



    IEnumerator climb(Vector3 position) {
        climbing = true;
        Vector3 origpos = transform.position;
        Vector3 finalpos = new Vector3(transform.position.x, position.y, transform.position.z);
        float multiplier = finalpos.y - origpos.y;
        rigidbody.isKinematic = true;
        //yield return new WaitForSeconds(0.2f);
        if (multiplier > 0.4f) {
            for (float t = 0f; t < 1; t += (Time.deltaTime * 2) / multiplier) {
                transform.position = Vector3.Lerp(origpos, finalpos, t);
                yield return new WaitForEndOfFrame();
            }
        }
        origpos = transform.position;
        finalpos = new Vector3(position.x, position.y + (capsule.height / 2), position.z);
        capsule.enabled = false;
        for (float t = 0f; t < 1; t += Time.deltaTime * 3) {
            transform.position = Vector3.Lerp(origpos, finalpos, t);
            yield return new WaitForEndOfFrame();
        }

        rigidbody.isKinematic = false;
        climbing = false;
        capsule.enabled = true;
    }

    void LedgeMove(float y) {
        Vector3 desiredMove = _camera.transform.forward * y;
    }

    void wallRunCheck() {
        if (rigidbody.velocity.sqrMagnitude < 25 || !wallRunSide()) { DisableWallRun(); return; }
        else wallRunLength += Time.fixedDeltaTime;

        rigidbody.velocity = new Vector3(rigidbody.velocity.x * (maxWallRunLength - wallRunLength / 30), maxWallRunLength * 2 - wallRunLength, rigidbody.velocity.z * (maxWallRunLength - wallRunLength / 30));
    }

    void wallCheck() {
        if (wallRightCast()) //Check if there is wall in reach on the right side
            {
            wallRun = wallRunCheck;
            wallRunSide = wallRightCast;

            rigidbody.useGravity = false;
            rotateCamera(10);
        }
        else if (wallLeftCast()) //Check if there is wall in reach on the left side
            {
            wallRun = wallRunCheck;
            wallRunSide = wallLeftCast;

            rigidbody.useGravity = false;
            rotateCamera(-10);
        }
    }

    bool wallRightCast() {
        Ray WallRight = new Ray(transform.position, _camera.transform.right);
        return Physics.Raycast(WallRight, 0.75f, LedgeMask);
    }

    bool wallLeftCast() {
        Ray WallLeft = new Ray(transform.position, -_camera.transform.right);
        return Physics.Raycast(WallLeft, 0.75f, LedgeMask);
    }


    void DisableWallRun() {
        wallRun = wallCheck;
        rigidbody.useGravity = true;
        wallRunLength = 0;
        wallRunEnabled = false; //Wall run is disabled to ensure player won't be able to wall run until he hits the ground
        rotateCamera(0);
    }

    void rotateCamera(float toAngle) {
        StopCoroutine("RotateCamera");
        StartCoroutine("RotateCamera", toAngle); //Rotates the camera to horizontal position
    }

    IEnumerator RotateCamera(float toAngle) {
        float time = 0f;
        Vector3 original = (_camera.transform.localEulerAngles.z > 180) ? new Vector3(_camera.transform.localEulerAngles.x, _camera.transform.localEulerAngles.y, _camera.transform.localEulerAngles.z - 360) : _camera.transform.localEulerAngles;
        Debug.Log(original);
        //Unity convert negative angles to positive angles, but this is major problem while -10 is 350 degrees, but |-10| is not the same as |350|
        while (time != 1) {
            time += (time + Time.deltaTime < 1) ? Time.deltaTime * 2 : 1 - time; //if(time+Time.deltaTime < 1) {time += Time.deltaTime*2;} else {time += 1-time}
            _camera.transform.localEulerAngles = Vector3.Lerp(original, new Vector3(_camera.transform.localEulerAngles.x, _camera.transform.localEulerAngles.y, toAngle), time);
            yield return new WaitForEndOfFrame();
        }
    }
}

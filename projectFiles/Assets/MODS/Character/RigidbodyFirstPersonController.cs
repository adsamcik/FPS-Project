using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyFirstPersonController : MonoBehaviour {
    [Serializable]
    public class MovementSettings {
        public float ForwardSpeed = 8.0f;   // Speed when walking forward
        public float BackwardSpeed = 4.0f;  // Speed when walking backwards
        public float StrafeSpeed = 4.0f;    // Speed when walking sideways
        public float RunMultiplier = 2.0f;   // Speed when sprinting
        public KeyCode RunKey = KeyCode.LeftShift;
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector]
        public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
        private bool m_Running;
#endif

        public void UpdateDesiredTargetSpeed(Vector2 input) {
            if (input == Vector2.zero) return;
            if (input.x > 0 || input.x < 0) {
                //strafe
                CurrentTargetSpeed = StrafeSpeed;
            }
            if (input.y < 0) {
                //backwards
                CurrentTargetSpeed = BackwardSpeed;
            }
            if (input.y > 0) {
                //forwards
                //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                CurrentTargetSpeed = ForwardSpeed;
            }
#if !MOBILE_INPUT
            if (Input.GetKey(RunKey)) {
                CurrentTargetSpeed *= RunMultiplier;
                m_Running = true;
            }
            else {
                m_Running = false;
            }
#endif
        }

#if !MOBILE_INPUT
        public bool Running {
            get { return m_Running; }
        }
#endif
    }


    [Serializable]
    public class AdvancedSettings {
        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
        public bool airControl; // can the user control the direction that is being moved in the air
    }

    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLook mouseLook = new MouseLook();
    public AdvancedSettings advancedSettings = new AdvancedSettings();


    private Rigidbody m_RigidBody;
    private CapsuleCollider m_Capsule;
    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;


    public Vector3 Velocity {
        get { return m_RigidBody.velocity; }
    }

    public bool Grounded {
        get { return m_IsGrounded; }
    }

    public bool Jumping {
        get { return m_Jumping; }
    }

    public bool Running {
        get {
#if !MOBILE_INPUT
            return movementSettings.Running;
#else
	            return false;
#endif
        }
    }


    private void Start() {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        mouseLook.Init(transform, cam.transform);
        wallRun = wallCheck;
    }


    private void Update() {

        if (climbing) return;

        RotateView();

        if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump) {
            m_Jump = true;
        }

    }


    private void FixedUpdate() {
        if (climbing) return;

        GroundCheck();

        if (!m_IsGrounded && m_Jumping && LedgeCheck()) return;

        Vector2 input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded)) {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
            if (m_RigidBody.velocity.sqrMagnitude <
                (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)) {
                m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
            }
        }

        if (m_IsGrounded) {
            if (!wallRunEnabled) wallRunEnabled = true;

            m_RigidBody.drag = 5f;

            if (m_Jump) {
                m_RigidBody.drag = 0f;
                m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                m_Jumping = true;
            }

            if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f) {
                m_RigidBody.Sleep();
            }
        }
        else {
            m_RigidBody.drag = 0f;
            if (m_PreviouslyGrounded && !m_Jumping) {
                StickToGroundHelper();
            }
        }
        m_Jump = false;
        Debug.Log(wallRunLength);
        if ((m_Jumping || wallRunLength > 0) && wallRunEnabled) 
            wallRun();
    }


    private float SlopeMultiplier() {
        float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }


    private void StickToGroundHelper() {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo,
                               ((m_Capsule.height / 2f) - m_Capsule.radius) +
                               advancedSettings.stickToGroundHelperDistance)) {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f) {
                m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
            }
        }
    }


    private Vector2 GetInput() {

        Vector2 input = new Vector2 {
            x = CrossPlatformInputManager.GetAxis("Horizontal"),
            y = CrossPlatformInputManager.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }


    private void RotateView() {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        mouseLook.LookRotation(transform, cam.transform);

        if (m_IsGrounded || advancedSettings.airControl) {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
        }
    }


    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck() {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo,
                               ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance)) {
            m_IsGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
        }
        else {
            m_IsGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
        if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping) {
            m_Jumping = false;
        }
    }

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

        bool LedgeCheck() {
            RaycastHit lhit;
            Vector3 transformPosition = transform.position;
            //Calculates ledge position based on where you are looking. Uses 2 axes (x,z) to keep ledge always above you.
            Vector3 Ledge = new Vector3(transformPosition.x + cam.transform.forward.x / 1.5f, transformPosition.y + 1.5f, transformPosition.z + cam.transform.forward.z);
            Ray LedgeRay = new Ray(Ledge, -Vector3.up);
            //First pass makes general raycast if there is ledge in front of the player
            if (Physics.SphereCast(LedgeRay, m_Capsule.radius / 2, out lhit, 1.5f, LedgeMask)) {
                Ray CeilingRay = new Ray(transform.position, Vector3.up);
                //Second pass checks if there is no ceiling above the player or ledge so player can climb
                if (!Physics.SphereCast(CeilingRay, m_Capsule.radius, 1.5f)) {
                    //Third pass detects if the ledge is not too steep
                    if (Vector3.Angle(lhit.normal, Vector3.up) < 25) {
                        Ray FrontRay = new Ray(new Vector3(transform.position.x, lhit.point.y + 1, transform.position.z), cam.transform.forward);
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
            m_RigidBody.isKinematic = true;
            //yield return new WaitForSeconds(0.2f);
            if (multiplier > 0.4f) {
                for (float t = 0f; t < 1; t += (Time.deltaTime * 2) / multiplier) {
                    transform.position = Vector3.Lerp(origpos, finalpos, t);
                    yield return new WaitForEndOfFrame();
                }
            }
            origpos = transform.position;
            finalpos = new Vector3(position.x, position.y + (m_Capsule.height / 2), position.z);
            m_Capsule.enabled = false;
            for (float t = 0f; t < 1; t += Time.deltaTime * 3) {
                transform.position = Vector3.Lerp(origpos, finalpos, t);
                yield return new WaitForEndOfFrame();
            }
            m_RigidBody.isKinematic = false;
            climbing = false;
            m_Capsule.enabled = true;
        }

        void LedgeMove(float y) {
            Vector3 desiredMove = cam.transform.forward * y;
        }

        void wallRunCheck() {
            if (m_RigidBody.velocity.sqrMagnitude < 25 || !wallRunSide()) { DisableWallRun(); return; }
            else wallRunLength += Time.fixedDeltaTime;

            //m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x * (wallRunLength / maxWallRunLength), wallRunLength / maxWallRunLength * 5, m_RigidBody.velocity.z * (wallRunLength / maxWallRunLength));
            //m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, wallRunLength / maxWallRunLength * 5, m_RigidBody.velocity.z);
        }

        void wallCheck() {
            if (wallRightCast()) //Check if there is wall in reach on the right side
            {
                wallRun = wallRunCheck;
                wallRunSide = wallRightCast;

                m_RigidBody.useGravity = false;
                rotateCamera(10);
            }
            else if (wallLeftCast()) //Check if there is wall in reach on the left side
            {
                wallRun = wallRunCheck;
                wallRunSide = wallLeftCast;

                m_RigidBody.useGravity = false;
                rotateCamera(-10);
            }
        }

        bool wallRightCast() {
            Ray WallRight = new Ray(transform.position, cam.transform.right);
            return Physics.Raycast(WallRight, 0.75f, LedgeMask);
        }

        bool wallLeftCast() {
            Ray WallLeft = new Ray(transform.position, -cam.transform.right);
            return Physics.Raycast(WallLeft, 0.75f, LedgeMask);
        }


        void DisableWallRun() {
            wallRun = wallCheck;
            m_RigidBody.useGravity = true;
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
            Vector3 original = (cam.transform.localEulerAngles.z > 180) ? new Vector3(cam.transform.localEulerAngles.x, cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z - 360) : cam.transform.localEulerAngles;
            Debug.Log(original);
            //Unity convert negative angles to positive angles, but this is major problem while -10 is 350 degrees, but |-10| is not the same as |350|
            while (time != 1) {
                time += (time + Time.deltaTime < 1) ? Time.deltaTime * 2 : 1 - time; //if(time+Time.deltaTime < 1) {time += Time.deltaTime*2;} else {time += 1-time}
                cam.transform.localEulerAngles = Vector3.Lerp(original, new Vector3(cam.transform.localEulerAngles.x, cam.transform.localEulerAngles.y, toAngle), time);
                yield return new WaitForEndOfFrame();
            }
    }

}


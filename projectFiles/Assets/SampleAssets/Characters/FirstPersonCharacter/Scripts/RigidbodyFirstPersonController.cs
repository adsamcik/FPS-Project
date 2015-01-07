using System.Collections;
using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;

namespace UnitySampleAssets.Characters.FirstPerson {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour {
        [System.Serializable]
        public class MovementSettings {
            public float ForwardSpeed = 8.0f; // Speed when walking forward
            public float BackwardSpeed = 4.0f; // Speed when walking backwards
            public float StrafeSpeed = 4.0f; // Speed when walking sideways
            public float SprintSpeed = 10.0f; // Speed when sprinting
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector]
            public float CurrentTargetSpeed = 8f;
            private bool running;

            public void UpdateDesiredTargetSpeed() {
                if (CrossPlatformInputManager.GetButton("Fire1")) {
                    CurrentTargetSpeed = SprintSpeed;
                    running = true;
                    return;
                }
                CurrentTargetSpeed = ForwardSpeed;
                running = false;
            }


            public bool Running {
                get { return running; }
            }
        }

        [System.Serializable]
        public class AdvancedSettings {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character 
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
        }

        public Camera _camera;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();

        private Rigidbody RigidBody;
        private CapsuleCollider Capsule;
        private float yRotation;
        private Vector3 groundContactNormal;
        private bool jump, previouslyGrounded, jumping, isGrounded;


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

        public AnimationCurve SlopeCurveModifier2 = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1f, 0.0f));


        public Vector3 Velocity {
            get { return RigidBody.velocity; }
        }

        public bool Grounded {
            get { return isGrounded; }
        }

        public bool Jumping {
            get { return jumping; }
        }

        public bool Running {
            get { return movementSettings.Running; }
        }


        private void Start() {
            if (!Application.isEditor) {
                Screen.showCursor = true;
                Screen.lockCursor = true;
            }
            RigidBody = GetComponent<Rigidbody>();
            Capsule = GetComponent<CapsuleCollider>();
            wallRun = wallCheck;
        }


        private void Update() {
            if (climbing) return;
            RotateView();

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !jump) {
                jump = true;
            }
        }


        private void FixedUpdate() {
            if (climbing) return;
            GroundCheck();
            Vector2 input = GetInput();

            if (!isGrounded && jumping && LedgeCheck()) return;

            if ((input.x != 0 || input.y != 0) && (advancedSettings.airControl || isGrounded)) {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = _camera.transform.forward * input.y + _camera.transform.right * input.x;
                desiredMove = (desiredMove - Vector3.Project(desiredMove, groundContactNormal)).normalized;

                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                //if seems to be useless, but left commented in case there were ever issues with something. (Might be used for control during jumps which is disabled)
                //if (RigidBody.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)){     
                RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                //}


            }

            if (isGrounded) {
                RigidBody.drag = 5f;

                if (!wallRunEnabled) wallRunEnabled = true;

                if (jump) {
                    RigidBody.drag = 0f;
                    RigidBody.velocity = new Vector3(RigidBody.velocity.x, 0f, RigidBody.velocity.z);
                    RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    jumping = true;
                }

                if (!jumping && input.x == 0f && input.y == 0f && RigidBody.velocity.magnitude < 1f) {
                    RigidBody.Sleep();
                }
            }
            else {
                RigidBody.drag = 0f;
                if (previouslyGrounded && !jumping) {
                    StickToGroundHelper();
                }
            }
            jump = false;

            if ((jumping || wallRunLength > 0) && wallRunEnabled) wallRun();

            // if (walkOrRun) GetComponent<Weapon_Controller>();
        }


        private float SlopeMultiplier() {
            float angle = Vector3.Angle(groundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper() {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, Capsule.radius, Vector3.down, out hitInfo,
                                   ((Capsule.height / 2f) - Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance)) {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f) {
                    RigidBody.velocity = RigidBody.velocity - Vector3.Project(RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput() {
            movementSettings.UpdateDesiredTargetSpeed();
            Vector2 input = new Vector2 {
                x = CrossPlatformInputManager.GetAxis("Horizontal"),
                y = CrossPlatformInputManager.GetAxis("Vertical")
            };
            return input;
        }


        private void RotateView() {
            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;
            Vector2 mouseInput = mouseLook.Clamped(yRotation, transform.localEulerAngles.y);

            // handle the rotation round the x axis on the camera
            _camera.transform.localEulerAngles = new Vector3(-mouseInput.y, _camera.transform.localEulerAngles.y, _camera.transform.localEulerAngles.z);
            yRotation = mouseInput.y;
            transform.localEulerAngles = new Vector3(0, mouseInput.x, 0);

            if (isGrounded || advancedSettings.airControl) {
                // Rotate the rigidbody velocity to match the new direction that the character is looking 
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                RigidBody.velocity = velRotation * RigidBody.velocity;
            }
        }


        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck() {
            previouslyGrounded = isGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, Capsule.radius, Vector3.down, out hitInfo,
                                   ((Capsule.height / 2f) - Capsule.radius) + advancedSettings.groundCheckDistance)) {
                isGrounded = true;
                groundContactNormal = hitInfo.normal;
                //if (Velocity.y > 5) playerStats.DealDamage(DamageType.fall);
            }
            else {
                isGrounded = false;
                groundContactNormal = Vector3.up;
            }
            if (!previouslyGrounded && isGrounded && jumping) {
                jumping = false;
            }
        }



        bool LedgeCheck() {
            RaycastHit lhit;
            Vector3 transformPosition = transform.position;
            //Calculates ledge position based on where you are looking. Uses 2 axes (x,z) to keep ledge always above you.
            Vector3 Ledge = new Vector3(transformPosition.x + _camera.transform.forward.x / 1.5f, transformPosition.y + 1.5f, transformPosition.z + _camera.transform.forward.z);
            Ray LedgeRay = new Ray(Ledge, -Vector3.up);
            //First pass makes general raycast if there is ledge in front of the player
            if (Physics.SphereCast(LedgeRay, Capsule.radius / 2, out lhit, 1.5f, LedgeMask)) {
                Ray CeilingRay = new Ray(transform.position, Vector3.up);
                //Second pass checks if there is no ceiling above the player or ledge so player can climb
                if (!Physics.SphereCast(CeilingRay, Capsule.radius, 1.5f)) {
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
            finalpos = new Vector3(position.x, position.y + (Capsule.height / 2), position.z);
            Capsule.enabled = false;
            for (float t = 0f; t < 1; t += Time.deltaTime * 3) {
                transform.position = Vector3.Lerp(origpos, finalpos, t);
                yield return new WaitForEndOfFrame();
            }

            rigidbody.isKinematic = false;
            climbing = false;
            Capsule.enabled = true;
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
}
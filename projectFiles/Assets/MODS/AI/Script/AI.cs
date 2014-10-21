using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(stats))]
public class AI : MonoBehaviour {
    public bool isFriendly = false;
    public List<Threat> threats = new List<Threat>();

    [HideInInspector]
    public bool threatChanged;

    NavMeshAgent agent;

    public delegate void BehaviorSwitch();
    BehaviorSwitch activeBehavior;

    bool canShoot;


    //Behaviors
    public Inquire inquire;

    void Start() {
        //Initialize behaviors
        inquire = new Inquire(this);

        StartCoroutine("threatUpdate");
        canShoot = (gameObject.GetComponent<weaponController>()) ? true : false;
        agent = GetComponent<NavMeshAgent>();
        activeBehavior = Idle;
    }

    void Update() {
        activeBehavior();
        //Debug.Log(activeBehavior.Method);
    }

    public void Idle() {
        if (threats[0].inSight && threats[0].threatValue > 600) activeBehavior = Attack;
    }

    public void Attack() {
        if (!checkThreats()) return;

        if (threats[0].inSight) {
            if (canShoot && threats[0].threatValue > 600) {
                transform.LookAt(new Vector3(threats[0].transform.position.x, transform.position.y, threats[0].transform.position.y));
                //rigidbody.MoveRotation(Quaternion.Euler(new Vector3(0, threats[0].transform.position.y, 0)));
                GetComponent<weaponController>().CurrentWeaponAttack();
            }
        }
        else {
            activeBehavior = inquire.InitialPhase;
        }
    }

    public void Aware() {

    }




    public enum behavior {
        Idle = 0,
        Attack = 1,
        Inquire = 2,
        Patrol = 3,
        Flee = 4,
        Help = 5,
        Follow = 6,
        Seek = 7,
        Defend = 8,
        Dead = 9
    }

    public bool checkThreats() {
        bool isOk = true;
        if (threats[0].gameObject == null) { threats.RemoveAt(0); isOk = checkThreats(); }
        if (!isOk || threats.Count == 0) return false;
        return true;
    }

    IEnumerator threatUpdate() {
        while (true) {
            for (int i = 0; i < threats.Count; i++) {
                if (!threats[i].gameObject) threats.RemoveAt(i);
                else threats[i].estimateThreat();
                yield return new WaitForEndOfFrame();
            }
            orderThreats();
            yield return new WaitForSeconds(0.250f);
        }
    }

    public void shotMe(GameObject thatOne) {
        for (int i = 0; i < threats.Count; i++) {
            if (threats[i].gameObject == thatOne) { threats[i].setAgressive(); break; }
        }
    }


    //Trigger functions

    void OnTriggerEnter(Collider other) {
        if (!other.isTrigger && (other.CompareTag("Player") || other.CompareTag("AI"))) {
            if (!other.GetComponent<stats>()) { Debug.LogError("Object does not contain required component PlayerStats! Aborting."); return; }
            foreach (Threat go in threats) if (go.gameObject == other.gameObject) return;
            threats.Add(new Threat(other.gameObject, isFriendly, gameObject));
        }
    }


    //other functions
    /// <summary>
    /// Uses Selection Sort.
    /// Optimalization not yet needed.
    /// </summary>
    void orderThreats() {
        if (!threatChanged) return;
        for (int y = 0; y < threats.Count; y++) {
            Vector2 highestInSight = Vector2.zero;
            Vector2 highest = Vector2.zero;
            for (int i = y; i < threats.Count; i++) {
                if (threats[i].inSight && threats[i].threatValue > highestInSight.y) highestInSight = new Vector2(i, threats[i].threatValue);
                else if (highest.y < threats[i].threatValue) highest = new Vector2(i, threats[i].threatValue);
            }

            if (highestInSight != Vector2.zero) {
                Threat cache = threats[y];
                threats[y] = threats[(int)highestInSight.x];
                threats[(int)highestInSight.x] = cache;
            }
            else if (highest != Vector2.zero) {
                Threat cache = threats[y];
                threats[y] = threats[(int)highest.x];
                threats[(int)highest.x] = cache;
            }
            else break;
        }
        threatChanged = false;
    }

    public class AIpath {
        public Vector3 to;
        public Vector3[] colliders;

    }

    //[System.Serializable]
    public class Inquire {
        AI ai;
        Transform transform;

        float distanceWalked;
        Vector3 prevPosition;

        List<Vector3> checkedPositions;
        int posCount;

        int layerMask = 1 << 31;

        float radius = 20;

        public Inquire(AI ai) {
            this.ai = ai;
            transform = ai.GetComponent<Transform>();
        }
        public void InitialPhase() {
            checkedPositions = new List<Vector3>();
            prevPosition = ai.transform.position;
            distanceWalked = 0;

            ai.agent.SetDestination(ai.threats[0].lastSeenWhere);
            ai.activeBehavior = Update;
        }

        //void EnvironmentAnalyzation() {
        //    RaycastHit[] hitArray = RayCastAround(360, 18, 2);
        //    if (hitArray.Length > 0) {
        //        AIPathPoint[] pointCache = new AIPathPoint[hitArray.Length]; //x - index, y - value
        //        List<AIPathPoint> pathPointCache = new List<AIPathPoint>();
        //        int maxIndex = -1;
        //        for (int i = 0; i < hitArray.Length; i++) {
        //            bool found = false;
        //            for (int y = 0; y <= maxIndex; y++) if (pathPointCache[y].collider == hitArray[i].collider) { found = true; break; }
        //            if (!found) { pathPointCache.Add(new AIPathPoint(hitArray[i])); }
        //        }

        //        pointCache = pathPointCache.ToArray();

        //        Vector3 selected;
        //        //Vector3 estDirection = threats[0].lastSeen + threats[0].lastSeenVelocity;
        //        if (posCount < 2) {
        //            selected = new Vector2(0, Mathf.Abs(Vector3.Angle(pointCache[0].point - transform.position, ai.threats[0].lastSeenVelocity)));
        //            for (int i = 1; i < pointCache.Length; i++) {
        //                float angle;
        //                angle = Mathf.Abs(Vector3.Angle(pointCache[0].point - transform.position, ai.threats[0].lastSeenVelocity));
        //                if (angle < selected.y) { selected.y = angle; selected.x = i; }
        //            }
        //        }
        //        else {
        //            selected = new Vector3(0, 0);
        //            for (int i = 0; i < pointCache.Length; i++) {
        //                int value = 0;
        //                if (pointCache[i].isPath) value++;
        //                if (CheckIfBeen(checkedPositions, pointCache[i].point, 10)) value++;
        //                if (value > selected.y) { selected.x = i; selected.y = value; }
        //            }
        //        }
        //        ai.agent.SetDestination(hitArray[(int)selected.x].point);
        //        posCount++;
        //    }
        //}

        float PathLength(NavMeshPath path) {
            if (path.corners.Length == 0) return 0;
            if (path.corners.Length < 2) return Vector3.Distance(transform.position, path.corners[0]);

            Vector3 previousCorner = path.corners[0];
            float lengthSoFar = 0.0F;
            for (int i = 1; i < path.corners.Length; i++ ) {
                Vector3 currentCorner = path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
            }

            return lengthSoFar;
        }

        NavMeshPath FindSpot(Vector3 iAmHere, float distance) {
            NavMeshPath path = new NavMeshPath();
            NavMeshHit navPos;

            float angle = Random.Range(0, 360);
            Vector3 position = iAmHere + new Vector3(distance * Mathf.Cos(angle), 0, distance * Mathf.Sin(angle));
            position.y = Random.Range(0, 200);
            NavMesh.SamplePosition(position, out navPos, radius, -1);

            if (!CheckIfBeen(navPos.position, 5f) && ai.agent.CalculatePath(navPos.position, path)) return path;
            else FindSpot(iAmHere, distance);

            return path;
        } 

        void EnvironmentAnalyzation() {
            checkedPositions.Add(transform.position);
            Debug.DrawRay(transform.position, Vector3.up * 100, Color.red, 10000000f);
            ai.agent.path = FindSpot(transform.position, radius);   
        }

        class AIPathPoint {
            public Collider collider;
            public Vector3 point;
            public float distance;
            public bool isPath;
            public float value;

            public AIPathPoint(RaycastHit hit) {
                this.collider = hit.collider;
                this.distance = hit.distance;
                this.point = hit.point;
                isPath = (distance > 10) ? true : false;
            }


        }

        public void Update() {
            if (ai.threats[0].inSight) { ai.activeBehavior = ai.Attack; ai.agent.Stop(false); }
            else if (ai.agent.remainingDistance < 1) EnvironmentAnalyzation();
            else if (distanceWalked > 500) ai.activeBehavior = ai.Idle;

            distanceWalked += (transform.position - prevPosition).magnitude;
            prevPosition = transform.position;
        }

        RaycastHit[] RayCastAround(float degrees, int count, float threshold = 0, bool checkPos = false) {
            bool thEnabled = (threshold > 0) ? true : false;
            float distance = (threshold > 5) ? threshold * 2 : 10;
            List<RaycastHit> hitList = new List<RaycastHit>();

            for (int i = 0; i < count; i++) {
                float rad = (((degrees / count) * i) - transform.rotation.eulerAngles.y) * Mathf.Deg2Rad;
                RaycastHit hit;

                Ray ray = new Ray(transform.position + (transform.up / 2), new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)));
                if (Physics.Raycast(ray, out hit, distance, layerMask)) {
                    if (thEnabled) { if (CheckIfNotTooClose(hit, threshold)) hitList.Add(hit); }
                    else hitList.Add(hit);
                }
                else {
                    hit.point = ray.GetPoint(distance);
                    hitList.Add(hit);
                }
                Debug.DrawLine(transform.position + (transform.up / 2), hit.point, Color.blue, 1f);
            }
            Debug.Log(hitList.Count);
            return hitList.ToArray();
        }

        bool CheckIfNotTooClose(RaycastHit hit, float threshold) {
            if (hit.distance < threshold) return false;
            return true;
        }

        bool CheckIfBeen(Vector3 value, float distance) {
            distance *= distance;
            for (int i = 0; i < checkedPositions.Count; i++) {
                if (Mathf.Abs(value.sqrMagnitude - checkedPositions[i].sqrMagnitude) < distance) return true;
            }
            return false;
        }

    }

    //public interface Behavior {
    //    public void Update() { }
    //}


    //Threat object
    [System.Serializable]
    public class Threat {
        [HideInInspector]
        GameObject me;
        [HideInInspector]
        AI meAi;
        public GameObject gameObject;
        [HideInInspector]
        public Transform transform;
        public bool inSight { get; private set; }
        public bool isAgressive { get; private set; }
        public bool isEnemy { get; private set; }
        public int threatValue { get; private set; }
        public Vector3 lastSeenWhere { get; private set; }
        public Vector3 lastSeenVelocity { get; private set; }
        public float lastSeenTime { get; private set; }
        stats s;

        public Threat(GameObject gameObject, bool isFriendly, GameObject me) {
            if (gameObject.CompareTag("Player")) isEnemy = (isFriendly) ? false : true;
            else isEnemy = (gameObject.GetComponent<AI>().isFriendly == isFriendly) ? false : true;
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
            s = gameObject.GetComponent<stats>();
            this.me = me;
            meAi = me.GetComponent<AI>();
            CheckSight();
        }

        public int estimateThreat() {
            int prevThreat = threatValue;
            CheckSight();
            threatValue = s.publicStats.threatValue;
            threatValue += (inSight) ? 500 : 0;
            float distance = (transform.position - me.transform.position).magnitude;
            threatValue += (distance < 30) ? (30 - (int)distance) * 2 : 0;
            threatValue += (isAgressive) ? 100 : 0;
            threatValue += (isEnemy) ? 100 : 0;
            if (prevThreat != threatValue) meAi.threatChanged = true;
            return threatValue;
        }

        public void setAgressive() {
            isAgressive = true;
        }

        public bool CheckSight() {
            RaycastHit hit;
            if (Physics.Raycast(me.transform.position, gameObject.transform.position - me.transform.position, out hit)) {
                if (hit.collider.CompareTag(gameObject.tag)) inSight = true;
                else {
                    if (inSight) {
                        lastSeenWhere = transform.position;
                        lastSeenVelocity = gameObject.rigidbody.velocity;
                        lastSeenTime = Time.time;
                    }
                    inSight = false;
                }
            }
            else inSight = false;
            return inSight;
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI : MonoBehaviour {
    public bool isFriendly = false;
    public List<Threat> threats = new List<Threat>();

    [HideInInspector]
    public bool threatChanged;

    NavMeshAgent agent;

    public delegate void BehaviorSwitch();
    BehaviorSwitch activeBehavior;

    bool canShoot;

    float distanceWalked;
    Vector3 prevPosition;
    

    void Start()
    {
        StartCoroutine("threatUpdate");
        canShoot = (gameObject.GetComponent<weaponController>()) ? true : false;
        agent = GetComponent<NavMeshAgent>();
        activeBehavior = Idle;
    }

    void Update()
    {
        activeBehavior();
    }

    public void changeBehavior(behavior b)
    {
        switch ((int)b)
        {
            case 0:
                activeBehavior = Idle;
                break;
            case 1:
                activeBehavior = Attack;
                break;
            case 2:
                activeBehavior = Inquire;
                break;

        }

    }

    public void Idle()
    {
        if (threats[0].inSight && threats[0].threatValue > 600) activeBehavior = Attack;
    }

    public void Attack()
    {
        if (!checkThreats()) return;

        if (threats[0].inSight)
        {
            if (canShoot && threats[0].threatValue > 600)
            {
                rigidbody.MoveRotation(Quaternion.Euler(new Vector3(0, threats[0].transform.position.y, 0)));
                GetComponent<weaponController>().Shoot();
            }
        }
        else {
            activeBehavior = Inquire;
        }
    }

    public void Aware()
    {

    }

    void Inquire()
    {
        if (!agent.hasPath) agent.SetDestination(threats[0].lastSeen);
        if ((transform.position - threats[0].lastSeen).sqrMagnitude < 2) { activeBehavior = InquireWait; distanceWalked = 0; }
    }

    void InquireAround() {
        RaycastHit[] hitArray = RayCastAround(180, 8, 5);
        if (hitArray.Length > 0)
        {
            agent.SetDestination(hitArray[Random.Range(0, hitArray.Length)].point);
        }
    }

    void InquireWait() {
        if (agent.remainingDistance < 1) InquireAround();
        else if (distanceWalked > 100) activeBehavior = Idle;

        distanceWalked += (transform.position - prevPosition).magnitude;
        prevPosition = transform.position;
    }

    RaycastHit[] RayCastAround(float degrees, int count, float threshold = 0)
    {
        bool thEnabled = (threshold > 0) ? true : false;
        List<RaycastHit> hitList = new List<RaycastHit>();

        for (int i = 0; i < count+1; i++)
        {
            float rad = (((degrees / count) * i) - transform.rotation.eulerAngles.y) * Mathf.Deg2Rad;
            RaycastHit hit;
            Debug.DrawRay(transform.position, new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)), Color.red, 0.5f);
            Ray ray = new Ray(transform.position, new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)));
            if (Physics.Raycast(ray, out hit, threshold + 50))
            {
                if (thEnabled) { if (hit.distance > threshold) hitList.Add(hit); }
                else hitList.Add(hit);
            }
            else
            {
                hit.point = ray.GetPoint(threshold);
                hitList.Add(hit);
            }
        }

        return hitList.ToArray();
    }

    public enum behavior
    {
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

    IEnumerator threatUpdate()
    {
        while (true)
        {
            for (int i = 0; i < threats.Count; i++)
            {
                if (!threats[i].gameObject) threats.RemoveAt(i);
                else threats[i].estimateThreat();
                yield return new WaitForEndOfFrame();
            }
            orderThreats();
            yield return new WaitForSeconds(0.250f);
        }
    }

    public void shotMe(GameObject thatOne)
    {
        for (int i = 0; i < threats.Count; i++) {
            if (threats[i].gameObject == thatOne) { threats[i].setAgressive(); break; }
        }
    }


    //Trigger functions

    void OnTriggerEnter(Collider other) {
        if (!other.isTrigger && (other.CompareTag("Player") || other.CompareTag("AI")))
        {
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
    void orderThreats()
    {
        if (!threatChanged) return;
        for (int y = 0; y < threats.Count; y++)
        {
            Vector2 highestInSight = Vector2.zero;
            Vector2 highest = Vector2.zero;
            for (int i = y; i < threats.Count; i++)
            {
                if (threats[i].inSight && threats[i].threatValue > highestInSight.y) highestInSight = new Vector2(i,threats[i].threatValue);
                else if (highest.y < threats[i].threatValue) highest = new Vector2(i,threats[i].threatValue);
            }

            if (highestInSight != Vector2.zero)
            {
                Threat cache = threats[y];
                threats[y] = threats[(int)highestInSight.x];
                threats[(int)highestInSight.x] = cache;
            }
            else if (highest != Vector2.zero)
            {
                Threat cache = threats[y];
                threats[y] = threats[(int)highest.x];
                threats[(int)highest.x] = cache;
            }
            else break;
        }
        threatChanged = false;
    }


    //Threat object
    [System.Serializable]
    public class Threat
    {
        GameObject me;
        AI meAi;
        public GameObject gameObject;
        public Transform transform;
        public bool inSight { get; private set; }
        public bool isAgressive { get; private set; }
        public bool isEnemy { get; private set; }
        public int threatValue { get; private set; }
        public Vector3 lastSeen { get; private set; }
        stats s;

        public Threat(GameObject gameObject, bool isFriendly, GameObject me)
        {
            if (gameObject.CompareTag("Player")) isEnemy = (isFriendly) ? false : true;
            else isEnemy = (gameObject.GetComponent<AI>().isFriendly == isFriendly) ? false : true;
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
            s = gameObject.GetComponent<stats>();
            this.me = me;
            meAi = me.GetComponent<AI>();
            CheckSight();
        }

        public int estimateThreat()
        {
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

        public void setAgressive()
        {
            isAgressive = true;
        }

        public bool CheckSight()
        {
            RaycastHit hit;
            if (Physics.Raycast(me.transform.position, gameObject.transform.position - me.transform.position, out hit))
            {
                if (hit.collider.CompareTag(gameObject.tag)) inSight = true;
                else
                {
                    if (inSight) lastSeen = gameObject.transform.position;
                    inSight = false;
                }
            }
            else inSight = false;
            return inSight;
        }
    }
}

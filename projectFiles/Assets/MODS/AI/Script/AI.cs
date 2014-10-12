using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI : MonoBehaviour {
    //Threat object
    [System.Serializable]
    public class Threat {
        GameObject me;
        AI meAi;
        public GameObject gameObject;
        public bool inSight { get; private set; }
        public bool isAgressive { get; private set; }
        public bool isEnemy { get; private set; }
        public int threatValue { get; private set; }
        public Vector3 lastSeen { get; private set; }
        stats s;

        public Threat(GameObject gameObject, bool isFriendly, GameObject me) {
            if (gameObject.CompareTag("Player")) isEnemy = (isFriendly) ? false : true;
            else isEnemy = (gameObject.GetComponent<AI>().isFriendly == isFriendly) ? false : true;
            this.gameObject = gameObject;
            s = gameObject.GetComponent<stats>();
            this.me = me;
            meAi = me.GetComponent<AI>();
            CheckSight();
        }

        public int estimateThreat()
        {
            int prevThreat = threatValue;
            CheckSight();
            //isAgressive = (ps.publicStats.shotLately) ? true : false;
            threatValue = s.publicStats.threatValue;
            threatValue += (inSight) ? 500 : 0;
            threatValue += (isAgressive) ? 100 : 0;
            threatValue += (isEnemy) ? 100 : 0;
            if (prevThreat != threatValue) meAi.threatChanged = true;
            //Debug.Log(threatValue);
            return threatValue;
        }

        public void setAgressive() {
            isAgressive = true;
        }

        public bool CheckSight() {
            RaycastHit hit;
            if (Physics.Raycast(me.transform.position, gameObject.transform.position - me.transform.position, out hit))
            {
                if (hit.collider.CompareTag(gameObject.tag)) inSight = true;
                else {
                    if (inSight) lastSeen = gameObject.transform.position;
                    inSight = false;
                }
            }
            else inSight = false;
            return inSight;
        }
    }


    public bool isFriendly = false;
    public List<Threat> threats = new List<Threat>();

    [HideInInspector]
    public bool threatChanged;

    bool canShoot;

    void Start() {
        canShoot = (gameObject.GetComponent<weaponController>()) ? true : false;
        StartCoroutine("threatUpdate");
    }

    void Update()
    {
        if (!checkThreats()) return ;    
        
        if (threats[0].threatValue > 550 && canShoot)
        {
            if (threats[0].inSight) GetComponent<weaponController>().Shoot();
        }
    }

    bool checkThreats() {
        bool isOk = true;
        if (threats[0].gameObject == null) { threats.RemoveAt(0); isOk = checkThreats(); }
        if (!isOk || threats.Count == 0) return false;
        return true;
    }

    void Move() {
        rigidbody.AddForce((threats[0].gameObject.transform.position - transform.position) * Time.deltaTime,ForceMode.VelocityChange);
    }

    IEnumerator threatUpdate()
    {
        while (true)
        {
            for (int i = 0; i < threats.Count; i++)
            {
                if (!threats[i].gameObject) threats.RemoveAt(i);
                else threats[i].estimateThreat();
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

    /** 
 * Razeni slevanim (od nejvyssiho)
 * @param array pole k serazeni
 * @param aux pomocne pole stejne delky jako array
 * @param left prvni index na ktery se smi sahnout
 * @param right posledni index, na ktery se smi sahnout
 */
    public void MergeSort(Threat[] array, Threat[] aux, int left, int right)
    {
        if (left == right) return;
        int middleIndex = (left + right) / 2;
        MergeSort(array, aux, left, middleIndex);
        MergeSort(array, aux, middleIndex + 1, right);
        Merge(array, aux, left, right);

        for (int i = left; i <= right; i++)
        {
            array[i] = aux[i];
        }
    }

    /**
     * Slevani pro Merge sort 
     * @param array pole k serazeni
     * @param aux pomocne pole (stejne velikosti jako razene)
     * @param left prvni index, na ktery smim sahnout
     * @param right posledni index, na ktery smim sahnout
     */
    private void Merge(Threat[] array, Threat[] aux, int left, int right)
    {
        int middleIndex = (left + right) / 2;
        int leftIndex = left;
        int rightIndex = middleIndex + 1;
        int auxIndex = left;
        while (leftIndex <= middleIndex && rightIndex <= right)
        {
            if (array[leftIndex].threatValue >= array[rightIndex].threatValue)
            {
                aux[auxIndex] = array[leftIndex++];
            }
            else
            {
                aux[auxIndex] = array[rightIndex++];
            }
            auxIndex++;
        }
        while (leftIndex <= middleIndex)
        {
            aux[auxIndex] = array[leftIndex++];
            auxIndex++;
        }
        while (rightIndex <= right)
        {
            aux[auxIndex] = array[rightIndex++];
            auxIndex++;
        }
    } 
}

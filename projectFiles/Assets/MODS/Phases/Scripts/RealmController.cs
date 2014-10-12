using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RealmController : MonoBehaviour {
    Phase phase;

    List<PhaseData> PD;

    public GUIText ColorUI;

    int[] PhaseEnabled = new int[3];


	void Start () {
        //Initialize();

        //phase = Phase.Red;
        //PD[(int)phase].SetColliders(true);
	}

    public void Initialize(bool r, bool g, bool b) {
        PD = new List<PhaseData>();
        Transform[] red = transform.Find("R").GetComponentsInChildren<Transform>();
        Transform[] green = transform.Find("G").GetComponentsInChildren<Transform>();
        Transform[] blue = transform.Find("B").GetComponentsInChildren<Transform>();
        if (r)
        {
            if (red.Length > 0)
            {
                PD.Add(new PhaseData());
                PD[PD.Count - 1].SetMaterial(red[1].renderer.material);
                for (int i = 1; i < red.Length; i++) if (red[i].collider != null) PD[PD.Count - 1].AddCollider(red[i].collider);
                PhaseEnabled[0] = PD.Count - 1;
            }
        }
        else
        {
            for (int i = 1; i < red.Length; i++) if (red[i].collider != null) red[i].collider.enabled = false;
            PhaseEnabled[0] = -1;
        }

        if (g)
        {
            if (green.Length > 0)
            {
                PD.Add(new PhaseData());
                PD[PD.Count - 1].SetMaterial(green[1].renderer.material);
                for (int i = 1; i < green.Length; i++) if (green[i].collider != null) PD[PD.Count - 1].AddCollider(green[i].collider);
                PhaseEnabled[1] = PD.Count - 1;
            }
        }
        else
        {
            for (int i = 1; i < green.Length; i++) if (green[i].collider != null) green[i].collider.enabled = false;
            PhaseEnabled[1] = -1;
        }

        if (b)
        {
            if (blue.Length > 0)
            {
                PD.Add(new PhaseData());
                PD[PD.Count - 1].SetMaterial(blue[1].renderer.material);
                for (int i = 1; i < blue.Length; i++) if (blue[i].collider != null) PD[PD.Count - 1].AddCollider(blue[i].collider);
                PhaseEnabled[2] = PD.Count - 1;
            }
        }
        else
        {
            for (int i = 1; i < blue.Length; i++) if (blue[i].collider != null) blue[i].collider.enabled = false;
            PhaseEnabled[2] = - 1;
        }

        if (PhaseEnabled[0] > -1) ActivatePhase(Phase.Red);
        else if (PhaseEnabled[1] > -1) ActivatePhase(Phase.Green);
        else if (PhaseEnabled[2] > -1) ActivatePhase(Phase.Blue);

        int ce = 0;
        ce += (PhaseEnabled[0] > -1) ? 1 : 0;
        ce += (PhaseEnabled[1] > -1) ? 1 : 0;
        ce += (PhaseEnabled[2] > -1) ? 1 : 0;
        if (ce < 2) ColorUI.enabled = false;
        
    }

    public int ActivePhase() {
        return PhaseData.active;
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q)) switchPhase(false);
        else if (Input.GetKeyDown(KeyCode.E)) switchPhase(true);
	}

    public void ActivatePhase(Phase newphase) {

        PD[PhaseData.active].SetColliders(false);

        switch ((int)newphase)
        {
            case 0:
                ColorUI.text = "Red";
                break;
            case 1:
                ColorUI.text = "Green";
                break;
            case 2:
                ColorUI.text = "Blue";
                break;
        }

        ColorUI.color = PD[PhaseData.active].activeColor;


        PD[PhaseData.active].SetColliders(true);
    }

    void switchPhase(bool forward) {
        if (PD.Count == 0) return;

        PD[PhaseData.active].SetColliders(false);

        while (true)
        {
            if (forward) phase += ((int)phase == 2) ? -2 : 1;
            else phase -= ((int)phase == 0) ? -2 : 1;

            if (PhaseEnabled[(int)phase] > -1) break;
        }

        PhaseData.active = PhaseEnabled[(int)phase];

        switch ((int)phase) { 
            case 0:
                ColorUI.text = "Red";
                break;
            case 1:
                ColorUI.text = "Green";
                break;
            case 2:
                ColorUI.text = "Blue";
                break;        
        }
        ColorUI.color = PD[PhaseData.active].activeColor;


        PD[PhaseData.active].SetColliders(true);
    }

    class PhaseData {
        List<Collider> colliders = new List<Collider>();
        public Color activeColor;
        public Color inactiveColor;
        public Material material;
        public static int active;

        public void SetMaterial(Material material) {
            this.material = material;
            inactiveColor = material.color;
            activeColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 1f);
        }

        public void AddCollider(Collider collider) {
            collider.renderer.material = material;
            colliders.Add(collider);
            collider.enabled = false;
        }

        public void SetColliders(bool enable) {
            material.color = (enable) ? activeColor : inactiveColor;
            for (int i = 0; i < colliders.Count; i++) {
                colliders[i].enabled = enable;
            }
        }
    }
}

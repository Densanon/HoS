using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbital : MonoBehaviour
{
    public Transform[] objectsToOrbit;
    public Transform[] pivots;
    public float[] orbitSpeed;
    public Vector3 theRotation;
    bool ready = false;

    void Update()
    {
        if(ready)
            UpdateTheLocations();
    }

    private void UpdateTheLocations()
    {
        float time = Time.deltaTime;
        for (int i = 0; i < pivots.Length; i++)
        {
            float angle = orbitSpeed[i] * time/1.25f;
            pivots[i].Rotate(new Vector3(0,0,1),angle);
            objectsToOrbit[i].rotation = Quaternion.identity;
        }
    }

    public void SetOrbitalLocations(Transform[] orbitals)
    {
        objectsToOrbit = orbitals;
        theRotation = new Vector3(0f, 0f, Random.Range(0f, 2f));
        List<Transform> pivot = new();
        List<float> speed = new();
        float lstGap =0f;

        for(int i = 0; i < objectsToOrbit.Length; i++)
        {
            float gap;
            GameObject go = new("Pivot");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            pivot.Add(go.transform);
            if (i == 0) { gap = 1f; }
            else { gap = .4f; }
            objectsToOrbit[i].SetParent(pivot[i]);
            objectsToOrbit[i].localPosition = new Vector3(1, 0) * (lstGap + gap);
            speed.Add(objectsToOrbit.Length - i);
            lstGap += gap;
        }

        pivots = pivot.ToArray();
        orbitSpeed = speed.ToArray();

        for (int i = 0; i < pivots.Length; i++)
        {
            pivots[i].rotation = Quaternion.Euler(45f,0f,Random.Range(0f,360f));
            objectsToOrbit[i].rotation = Quaternion.identity;
        }
        ready = true;
    }
}

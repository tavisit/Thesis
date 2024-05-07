using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public class Blackhole : Body
{
    protected override void SpecificStart()
    {
        // usually for stellar-mass black holes which can be a few times to tens of times the mass of the Sun,
        // the photon ring is expected to be much smaller and harder to observe
        if (mass < 1e+2f) 
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name == "Plane" || transform.GetChild(i).name == "SwirlParticles")
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
    }
    protected override void SpecificUpdate()
    {
    }
}

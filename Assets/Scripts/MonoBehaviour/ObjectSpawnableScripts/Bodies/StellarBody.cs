using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public class StellarBody : Body
{
    public float starTemperature;
    public float relativeLuminousity;
    public bool setColor = false;

    protected override void SpecificStart()
    {
    }
    protected override void SpecificUpdate()
    {
        if (setColor == false)
        {
            Material emission = GetComponent<Renderer>().material;
            emission.SetFloat("_Temperature", starTemperature);
            emission.SetFloat("_CellDensity", emission.GetFloat("_CellDensity") + Random.Range(-10, 10));
            emission.SetFloat("_SolarPower", emission.GetFloat("_SolarPower") + Random.Range(-1, 1) * relativeLuminousity);
            setColor = true;
        }
    }
}

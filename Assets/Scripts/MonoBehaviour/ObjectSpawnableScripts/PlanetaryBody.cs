using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetaryBody : Body
{
    // Start is called before the first frame update
    new void Start()
    {
        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();
    }

    // Update is called once per frame
    new void Update()
    {
        UpdatePhysics();
    }
}

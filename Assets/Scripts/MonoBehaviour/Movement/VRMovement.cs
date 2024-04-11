using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRMovement : MonoBehaviour
{

    public Slider timeDilationSlider;
    private float timeDilationValue;


    // Start is called before the first frame update
    void Start()
    {
        timeDilationSlider.onValueChanged.AddListener(delegate {
            timeDilationValue = timeDilationSlider.value;
        });

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

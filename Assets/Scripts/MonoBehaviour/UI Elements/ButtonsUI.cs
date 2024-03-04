using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickResetSlider()
    {
        Slider slider = GameObject.Find("TimeDillationSlider").GetComponent<Slider>();
        if (slider != null)
        {
            slider.value = 0;
        }
    }
}

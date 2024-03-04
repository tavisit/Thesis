using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Slider slider = GameObject.Find("TimeDillationSlider").GetComponent<Slider>(); 
        if (slider != null)
        {
            GetComponent<Text>().text = slider.value.ToString();
        }

    }
}

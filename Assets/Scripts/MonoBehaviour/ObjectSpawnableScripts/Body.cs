using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Body : MonoBehaviour
{
    public float mass;
    public Vector3 velocity;
    public Vector3 acceleration;


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
            if(System.Math.Abs(slider.value) > Mathf.Epsilon)
            {
                velocity += acceleration * slider.value * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;
            }
        }
    }
}

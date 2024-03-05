using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Body : MonoBehaviour
{
    public float mass;
    public Vector3 velocity;
    public Vector3 acceleration;
    private Slider timeDilationSlider;

    private float maxVelocity = 299792458; // Maximum velocity
    private float velocityDamping = 0.99f;

    private void Start()
    {
        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();
    }

    private void Update()
    {
        UpdatePhysics();
    }

    private void UpdatePhysics()
    {
        if (timeDilationSlider != null && Mathf.Abs(timeDilationSlider.value) > Mathf.Epsilon)
        {
            float timeScale = timeDilationSlider.value;
            float deltaTime = Time.fixedDeltaTime * timeScale;

            velocity = velocity * deltaTime + acceleration * deltaTime * deltaTime;
            transform.position += velocity;

            velocity *= Mathf.Clamp01(velocityDamping * timeScale); // Apply damping scaled by time
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity); // Cap velocity
        }
        else
        {
            timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();
        }
    }
}

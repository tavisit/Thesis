using UnityEngine;
using UnityEngine.UI;

public class Body : MonoBehaviour
{
    public float mass;
    public Vector3 velocity;
    public Vector3 acceleration;
    protected Slider timeDilationSlider;

    private readonly float maxVelocity = 299792458; // Maximum velocity

    protected void Start()
    {
        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();
    }

    protected void Update()
    {
        UpdatePhysics();
    }

    protected void UpdatePhysics()
    {
        if (timeDilationSlider == null)
        {
            timeDilationSlider = GameObject.Find("TimeDilationSlider")?.GetComponent<Slider>();
        }

        if (timeDilationSlider != null && Mathf.Abs(timeDilationSlider.value) > Mathf.Epsilon)
        {
            // v = v0 + a*t
            // x = x0 + v0*t + 0.5*a*t^2
            // t=deltaTime
            float deltaTime = Time.fixedDeltaTime * timeDilationSlider.value;

            Vector3 oldVelocity = velocity;
            // Update velocity
            velocity += acceleration * deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

            // Then update position
            transform.position += oldVelocity * deltaTime + 0.5f * acceleration * deltaTime * deltaTime;
        }
    }
}

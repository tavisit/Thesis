using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public abstract class Body : MonoBehaviour
{
    public Vector3 oldPosition;
    public Vector3 oldVelocity;


    public float mass;
    public Vector3 velocity;
    public Vector3 acceleration;
    protected Slider timeDilationSlider;

    private readonly float maxVelocity = 299792458; // Maximum velocity


    protected abstract void SpecificStart();
    protected abstract void SpecificUpdate();

    protected void Start()
    {
        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();
        SpecificStart();
    }

    protected void Update()
    {
        UpdatePhysics();
        SpecificUpdate();
    }

    protected void UpdatePhysics()
    {
        oldPosition = transform.position;
        oldVelocity = velocity;

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

            // Update velocity
            velocity += acceleration * deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

            // Then update position
            transform.position += oldVelocity * deltaTime + 0.5f * acceleration * deltaTime * deltaTime;
        }
    }

    public bool objectChangedState()
    {
        if(oldVelocity != velocity || oldPosition != transform.position)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

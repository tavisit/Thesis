// For simulations with many bodies, consider more sophisticated 
// algorithms like Barnes-Hut, which reduce computational complexity at the cost of some accuracy.

__kernel void universal_attraction_force(__global float3* acceleration, __global const float* flattenedData, int length, int offset)
{
    int i = get_global_id(0);

    float G = 6.67430e-11f;

    if (i < length) {
        float3 totalForce = (float3)(0.0f, 0.0f, 0.0f);
        float3 position = { flattenedData[i*offset], flattenedData[i*offset + 1], flattenedData[i*offset + 2] };
        float mass = flattenedData[i*offset+3];

        for (int j = 0; j < length; j++) {
            if (i != j) {
                float3 position_other = { flattenedData[j*offset], flattenedData[j*offset + 1], flattenedData[j*offset + 2] };
                float mass_other = flattenedData[j*offset+3];
                float3 diff = position_other - position;
                float distSquared = dot(diff, diff) + 1e-10f;

                float forceMagnitude = G * (mass * mass_other) / distSquared;
                totalForce += normalize(diff) * forceMagnitude;
            }
        }
        acceleration[i] = totalForce / mass;
    }
}
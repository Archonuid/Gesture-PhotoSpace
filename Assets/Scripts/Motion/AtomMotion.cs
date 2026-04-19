using UnityEngine;

public class AtomMotion : MonoBehaviour
{
    public float sphereRadius = 10f;
    public float speed = 1.5f;
    public float steering = 2f;

    public bool ribbonMode = false;

    Vector3 velocity;

    void Start()
    {
        velocity = Random.onUnitSphere * speed;
    }

    void Update()
    {
        if (ribbonMode)
            return;

        float dt = Time.deltaTime;

        // random steering
        velocity += Random.insideUnitSphere * steering * dt;
        velocity = velocity.normalized * speed;

        transform.position += velocity * dt;

        // boundary check
        float dist = transform.position.magnitude;

        if (dist > sphereRadius)
        {
            Vector3 pushBack = -transform.position.normalized;
            velocity += pushBack * steering * dt;
        }
    }
}
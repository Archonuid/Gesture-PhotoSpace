using UnityEngine;

public class FabricShellController : MonoBehaviour
{
    public LayoutController layoutController;

    public int latitudeLines = 14;
    public int longitudeLines = 20;

    public float radius = 12f;

    public Material lineMaterial;

    LineRenderer[] latLines;
    LineRenderer[] lonLines;

    Vector3[] velocities;

    void Start()
    {
        GenerateShell();

        velocities = new Vector3[latitudeLines * longitudeLines];
    }

    void Update()
    {
        if (layoutController == null)
            return;

        UpdateShell(layoutController.morph);
    }

    void GenerateShell()
    {
        latLines = new LineRenderer[latitudeLines];
        lonLines = new LineRenderer[longitudeLines];

        for (int i = 0; i < latitudeLines; i++)
        {
            GameObject g = new GameObject("Lat_" + i);
            g.transform.parent = transform;

            LineRenderer lr = g.AddComponent<LineRenderer>();

            lr.useWorldSpace = false;
            lr.loop = true;
            lr.widthMultiplier = 0.03f;
            lr.positionCount = longitudeLines;
            lr.material = lineMaterial;

            latLines[i] = lr;
        }

        for (int i = 0; i < longitudeLines; i++)
        {
            GameObject g = new GameObject("Lon_" + i);
            g.transform.parent = transform;

            LineRenderer lr = g.AddComponent<LineRenderer>();

            lr.useWorldSpace = false;
            lr.loop = false;
            lr.widthMultiplier = 0.03f;
            lr.positionCount = latitudeLines;
            lr.material = lineMaterial;

            lonLines[i] = lr;
        }
    }

    void UpdateShell(float morph)
    {
        int index = 0;

        float stiffness = 35f;
        float damping = 7f;

        for (int lat = 0; lat < latitudeLines; lat++)
        {
            float v = (float)lat / (latitudeLines - 1);
            float phi = Mathf.PI * v;

            for (int lon = 0; lon < longitudeLines; lon++)
            {
                float u = (float)lon / longitudeLines;
                float theta = u * Mathf.PI * 2f;

                Vector3 sphere = new Vector3(
                    Mathf.Sin(phi) * Mathf.Cos(theta),
                    Mathf.Cos(phi),
                    Mathf.Sin(phi) * Mathf.Sin(theta)
                ) * radius;

                Vector3 cube = sphere.normalized * radius;

                Vector3 cylinder = new Vector3(
                    Mathf.Cos(theta) * radius,
                    Mathf.Lerp(-radius, radius, v),
                    Mathf.Sin(theta) * radius
                );

                Vector3 cone = new Vector3(
                    Mathf.Cos(theta) * (1 - v) * radius,
                    Mathf.Lerp(-radius, radius, v),
                    Mathf.Sin(theta) * (1 - v) * radius
                );

                Vector3 torus = new Vector3(
                    (radius + 3 * Mathf.Cos(phi)) * Mathf.Cos(theta),
                    3 * Mathf.Sin(phi),
                    (radius + 3 * Mathf.Cos(phi)) * Mathf.Sin(theta)
                );

                Vector3 capsule = new Vector3(
                    Mathf.Cos(theta) * radius * 0.4f,
                    Mathf.Lerp(-radius, radius, v),
                    Mathf.Sin(theta) * radius * 0.4f
                );

                Vector3 tetra = sphere.normalized * radius;
                Vector3 icosa = sphere.normalized * radius;

                Vector3 target;

                if (morph < 1)
                    target = Vector3.Lerp(sphere, cube, morph);

                else if (morph < 2)
                    target = Vector3.Lerp(cube, cylinder, morph - 1);

                else if (morph < 3)
                    target = Vector3.Lerp(cylinder, cone, morph - 2);

                else if (morph < 4)
                    target = Vector3.Lerp(cone, torus, morph - 3);

                else if (morph < 5)
                    target = Vector3.Lerp(torus, capsule, morph - 4);

                else if (morph < 6)
                    target = Vector3.Lerp(capsule, tetra, morph - 5);

                else
                    target = Vector3.Lerp(tetra, icosa, morph - 6);

                Vector3 current = latLines[lat].GetPosition(lon);

                Vector3 force = (target - current) * stiffness;

                velocities[index] += force * Time.deltaTime;
                velocities[index] *= Mathf.Exp(-damping * Time.deltaTime);

                Vector3 next = current + velocities[index] * Time.deltaTime;

                latLines[lat].SetPosition(lon, next);
                lonLines[lon].SetPosition(lat, next);

                index++;
            }
        }
    }
}
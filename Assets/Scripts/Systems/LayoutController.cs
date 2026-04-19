using UnityEngine;

public class LayoutController : MonoBehaviour
{
    public Transform photoContainer;
    public UDPReceiver receiver;

    public float morph = 0f;
    public bool active = false;

    float smoothedAngle = 0f;
    float angleVelocity;

    Transform[] photos;

    Vector3[] targets;
    Vector3[] velocities;

    // Cached shape positions
    Vector3[] sphere;
    Vector3[] cube;
    Vector3[] cylinder;
    Vector3[] cone;
    Vector3[] torus;
    Vector3[] capsule;
    Vector3[] tetra;
    Vector3[] icosa;

    void Start()
    {
        RefreshPhotos();
    }

    public void RefreshPhotos()
    {
        photos = photoContainer.GetComponentsInChildren<Transform>(true);
        photos = System.Array.FindAll(photos, t => t != photoContainer);

        int count = photos.Length;

        targets = new Vector3[count];
        velocities = new Vector3[count];

        sphere = new Vector3[count];
        cube = new Vector3[count];
        cylinder = new Vector3[count];
        cone = new Vector3[count];
        torus = new Vector3[count];
        capsule = new Vector3[count];
        tetra = new Vector3[count];
        icosa = new Vector3[count];

        PrecomputeShapes();
    }

    void Update()
    {
        if (!active)
            return;

        if (photos == null || photos.Length == 0)
        {
            RefreshPhotos();
            return;
        }

        smoothedAngle = Mathf.SmoothDamp(
            smoothedAngle,
            receiver.receivedAngle,
            ref angleVelocity,
            0.07f
        );

        float normalized = Mathf.InverseLerp(-180f, 180f, smoothedAngle);
        float targetMorph = normalized * 7f;

        morph = Mathf.Lerp(morph, targetMorph, Time.deltaTime * 4f);

        UpdateLayout();
    }

    void UpdateLayout()
    {
        int count = photos.Length;

        Vector3[] a;
        Vector3[] b;
        float t;

        if (morph < 1)
        {
            a = sphere;
            b = cube;
            t = morph;
        }
        else if (morph < 2)
        {
            a = cube;
            b = cylinder;
            t = morph - 1;
        }
        else if (morph < 3)
        {
            a = cylinder;
            b = cone;
            t = morph - 2;
        }
        else if (morph < 4)
        {
            a = cone;
            b = torus;
            t = morph - 3;
        }
        else if (morph < 5)
        {
            a = torus;
            b = capsule;
            t = morph - 4;
        }
        else if (morph < 6)
        {
            a = capsule;
            b = tetra;
            t = morph - 5;
        }
        else
        {
            a = tetra;
            b = icosa;
            t = morph - 6;
        }

        for (int i = 0; i < count; i++)
        {
            targets[i] = Vector3.Lerp(a[i], b[i], t);
        }

        // SPRING MOTION (visual smoothing)
        float stiffness = 40f;
        float damping = 8f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = photos[i].localPosition;
            Vector3 force = (targets[i] - pos) * stiffness;

            velocities[i] += force * Time.deltaTime;
            velocities[i] *= Mathf.Exp(-damping * Time.deltaTime);

            photos[i].localPosition += velocities[i] * Time.deltaTime;
        }
    }

    void PrecomputeShapes()
    {
        int count = photos.Length;

        float r = 10f;

        int grid = Mathf.CeilToInt(Mathf.Pow(count, 1f / 3f));
        float spacing = 6f;

        int columns = 12;
        float height = 12f;

        float R = 10f;
        float rTorus = 3f;

        float capsuleRadius = 4f;

        Vector3[] tetraVerts =
        {
            new Vector3(1,1,1),
            new Vector3(-1,-1,1),
            new Vector3(-1,1,-1),
            new Vector3(1,-1,-1)
        };

        float phi = (1 + Mathf.Sqrt(5)) / 2;

        Vector3[] icosaVerts =
        {
            new Vector3(-1,phi,0),
            new Vector3(1,phi,0),
            new Vector3(-1,-phi,0),
            new Vector3(1,-phi,0),
            new Vector3(0,-1,phi),
            new Vector3(0,1,phi),
            new Vector3(0,-1,-phi),
            new Vector3(0,1,-phi),
            new Vector3(phi,0,-1),
            new Vector3(phi,0,1),
            new Vector3(-phi,0,-1),
            new Vector3(-phi,0,1)
        };

        for (int i = 0; i < count; i++)
        {
            float sphi = Mathf.Acos(1 - 2 * (i + 0.5f) / count);
            float stheta = Mathf.PI * (1 + Mathf.Sqrt(5)) * i;

            sphere[i] = new Vector3(
                r * Mathf.Cos(stheta) * Mathf.Sin(sphi),
                r * Mathf.Sin(stheta) * Mathf.Sin(sphi),
                r * Mathf.Cos(sphi)
            );

            int x = i % grid;
            int y = (i / grid) % grid;
            int z = i / (grid * grid);

            cube[i] = new Vector3(
                (x - grid / 2f) * spacing,
                (y - grid / 2f) * spacing,
                (z - grid / 2f) * spacing
            );

            float angle = (i % columns) * Mathf.PI * 2f / columns;

            cylinder[i] = new Vector3(
                Mathf.Cos(angle) * r,
                (i / columns) * 2f - 10f,
                Mathf.Sin(angle) * r
            );

            float t = (float)i / count;

            float coneAngle = t * Mathf.PI * 6f;
            float coneRadius = (1f - t) * r;

            cone[i] = new Vector3(
                Mathf.Cos(coneAngle) * coneRadius,
                t * height - height * 0.5f,
                Mathf.Sin(coneAngle) * coneRadius
            );

            float u = (float)i / count;

            float theta = u * Mathf.PI * 2f * 4f;
            float phiT = u * Mathf.PI * 2f;

            torus[i] = new Vector3(
                (R + rTorus * Mathf.Cos(phiT)) * Mathf.Cos(theta),
                rTorus * Mathf.Sin(phiT),
                (R + rTorus * Mathf.Cos(phiT)) * Mathf.Sin(theta)
            );

            float capAngle = t * Mathf.PI * 6f;

            capsule[i] = new Vector3(
                Mathf.Cos(capAngle) * capsuleRadius,
                Mathf.Lerp(-height / 2, height / 2, t),
                Mathf.Sin(capAngle) * capsuleRadius
            );

            tetra[i] = tetraVerts[i % 4].normalized * r;
            icosa[i] = icosaVerts[i % 12].normalized * r;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class RibbonLayout : MonoBehaviour
{
    public Transform photoContainer;
    public UDPReceiver receiver;

    public int rows = 3;
    public int visibleColumns = 8;

    public float spacingX = 2.0f;
    public float spacingY = 2.6f;

    public float radius = 20f;

    public float distanceFromCamera = 10f;
    public float verticalOffset = -2f;

    public float normalScale = 2.2f;
    public float selectedScale = 2.7f;

    public bool navigationLocked = false;

    List<Transform> photos = new List<Transform>();

    int selectedRow = 1;
    int selectedColumn = 0;

    float scrollOffset = 0f;
    float scrollVelocity = 0f;

    int photosPerRow;

    float gestureCooldown = 0.20f;
    float lastGestureTime;

    void Start()
    {
        InitializeRibbon();
    }

    void OnEnable()
    {
        InitializeRibbon();
    }

    void Update()
    {
        if (!receiver.rightHandDetected)
            return;

        // prevent scrolling while image viewer is active
        if (!navigationLocked && !receiver.imageOpen)
        {
            HandleGestureInput();
        }

        UpdateRibbon();
    }

    void HandleGestureInput()
    {
        if (receiver == null)
            return;

        if (Time.time - lastGestureTime < gestureCooldown)
            return;

        float threshold = 0.035f;

        if (receiver.rightDX > threshold)
        {
            selectedColumn++;
            lastGestureTime = Time.time;
        }

        if (receiver.rightDX < -threshold)
        {
            selectedColumn--;
            lastGestureTime = Time.time;
        }

        if (receiver.rightDY > threshold)
        {
            selectedRow++;
            lastGestureTime = Time.time;
        }

        if (receiver.rightDY < -threshold)
        {
            selectedRow--;
            lastGestureTime = Time.time;
        }

        selectedRow = Mathf.Clamp(selectedRow, 0, rows - 1);
        selectedColumn = Mathf.Clamp(selectedColumn, 0, photosPerRow - 1);
    }

    void UpdateRibbon()
    {
        Transform cam = Camera.main.transform;

        Vector3 center =
            cam.position +
            cam.forward * distanceFromCamera +
            cam.up * verticalOffset;

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int i = 0; i < photosPerRow; i++)
            {
                int wrappedIndex =
                    ((index % photos.Count) + photos.Count) % photos.Count;

                Transform photo = photos[wrappedIndex];

                float column = i - scrollOffset;

                float angle = column * spacingX / radius;

                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;

                float y = (rows / 2f - r) * spacingY;

                Vector3 target =
                    center +
                    cam.right * x +
                    cam.up * y -
                    cam.forward * (radius - z);

                photo.position = Vector3.Lerp(
                    photo.position,
                    target,
                    Time.deltaTime * 6f
                );

                photo.rotation =
                    Quaternion.LookRotation(photo.position - cam.position);

                bool isSelected =
                    r == selectedRow &&
                    i == selectedColumn;

                Renderer renderer = photo.GetComponent<Renderer>();

                if (isSelected)
                {
                    photo.localScale = Vector3.Lerp(
                        photo.localScale,
                        new Vector3(selectedScale, selectedScale, 1f),
                        Time.deltaTime * 8f
                    );

                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", Color.yellow * 2f);
                }
                else
                {
                    photo.localScale = Vector3.Lerp(
                        photo.localScale,
                        new Vector3(normalScale, normalScale, 1f),
                        Time.deltaTime * 8f
                    );

                    renderer.material.DisableKeyword("_EMISSION");
                }

                index++;
            }
        }

        float targetScroll = selectedColumn;

        scrollOffset = Mathf.SmoothDamp(
            scrollOffset,
            targetScroll,
            ref scrollVelocity,
            0.2f
        );
    }

    void InitializeRibbon()
    {
        photos.Clear();

        foreach (Transform child in photoContainer)
            photos.Add(child);

        photosPerRow = Mathf.CeilToInt((float)photos.Count / rows);

        if (photosPerRow == 0)
            return;

        selectedRow = Mathf.Clamp(selectedRow, 0, rows - 1);
        selectedColumn = Mathf.Clamp(selectedColumn, 0, photosPerRow - 1);

        scrollOffset = selectedColumn;
    }

    public Transform GetSelectedPhoto()
    {
        if (photos == null || photos.Count == 0 || photosPerRow == 0)
            return null;

        int index = selectedRow * photosPerRow + selectedColumn;

        index = ((index % photos.Count) + photos.Count) % photos.Count;

        return photos[index];
    }

    public void LockNavigation()
    {
        navigationLocked = true;
    }

    public void UnlockNavigation()
    {
        navigationLocked = false;
    }
}
using UnityEngine;

public class PinchZoom : MonoBehaviour
{
    public RibbonLayout ribbonLayout;
    public UDPReceiver receiver;
    public float viewDistance = 2.8f;
    Transform activePhoto;
    bool viewingImage = false;
    Vector3 baseScale;
    Vector3 panOffset = Vector3.zero;
    public float panSensitivity = 8f;
    public float maxPan = 6f;
    float currentZoom = 1f;
    public float minZoom = 0.6f;
    public float maxZoom = 25f;

    void Update()
    {
        if (receiver == null || ribbonLayout == null)
            return;

        if (!receiver.rightHandDetected && viewingImage)
        {
            ExitImageView();
            return;
        }

        if (receiver.imageOpen && !viewingImage)
        {
            EnterImageView();
        }

        if (!receiver.imageOpen && viewingImage)
        {
            ExitImageView();
        }

        if (!viewingImage)
            return;

        UpdateZoom();
    }

    void EnterImageView()
    {
        activePhoto = ribbonLayout.GetSelectedPhoto();

        if (activePhoto == null)
            return;

        viewingImage = true;

        ribbonLayout.LockNavigation();

        Renderer r = activePhoto.GetComponent<Renderer>();

        Texture tex = r.material.mainTexture;

        if (tex == null)
            return;

        float aspect = (float)tex.width / tex.height;

        // Base viewing size
        float targetHeight = 6f;
        float targetWidth = targetHeight * aspect;

        baseScale = new Vector3(targetWidth, targetHeight, 1f);

        currentZoom = 1f;

        panOffset = Vector3.zero;

        activePhoto.localScale = baseScale;
    }

    void ExitImageView()
    {
        viewingImage = false;

        ribbonLayout.UnlockNavigation();

        panOffset = Vector3.zero;

        if (activePhoto != null)
        {
            activePhoto.localScale =
                new Vector3(2.2f, 2.2f, 1f);
        }

        activePhoto = null;
    }

    void UpdateZoom()
    {
        if (activePhoto == null)
            return;

        Transform cam = Camera.main.transform;

        float pinch = receiver.pinchDistance;

        float normalizedPinch = Mathf.InverseLerp(
            0.02f,  
            0.25f,  
            pinch
        );

        float targetZoom =
            Mathf.Lerp(
                minZoom,
                maxZoom,
                Mathf.Pow(normalizedPinch, 1.5f)
            );

        currentZoom = Mathf.Lerp(
            currentZoom,
            targetZoom,
            Time.deltaTime * 8f
        );

        activePhoto.localScale =
            baseScale * currentZoom;

        if (currentZoom > 1.05f)
        {
            float moveX = receiver.rightDX;
            float moveY = receiver.rightDY;

            float deadzone = 0.006f;

            if (Mathf.Abs(moveX) < deadzone)
                moveX = 0f;

            if (Mathf.Abs(moveY) < deadzone)
                moveY = 0f;

            Vector3 drag =
                (cam.right * moveX +
                cam.up * moveY)
                * panSensitivity;

            drag *= currentZoom * 0.15f;

            panOffset += drag;

            float dynamicPan =
                Mathf.Lerp(2f, 20f, currentZoom / maxZoom);

            panOffset.x = Mathf.Clamp(
                panOffset.x,
                -dynamicPan,
                dynamicPan
            );

            panOffset.y = Mathf.Clamp(
                panOffset.y,
                -dynamicPan,
                dynamicPan
            );
        }

        Vector3 basePos =
            cam.position +
            cam.forward * viewDistance;

        Vector3 targetPos =
            basePos + panOffset;

        activePhoto.position = Vector3.Lerp(
            activePhoto.position,
            targetPos,
            Time.deltaTime * 8f
        );

        activePhoto.rotation = cam.rotation;
    }
}
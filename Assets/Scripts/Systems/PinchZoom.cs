using UnityEngine;

public class PinchZoom : MonoBehaviour
{
    public RibbonLayout ribbonLayout;
    public UDPReceiver receiver;
    public float viewDistance = 2.8f;
    public float minScale = 8f;
    public float maxScale = 18f;
    Transform activePhoto;
    bool viewingImage = false;
    Vector3 baseScale;
    float currentZoom = 1f;
    public float minZoom = 0.6f;
    public float maxZoom = 2.5f;

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
        if (activePhoto == null) return;

        viewingImage = true;
        ribbonLayout.LockNavigation();

        Renderer r = activePhoto.GetComponent<Renderer>();
        Texture tex = r.material.mainTexture;
        if (tex == null) return;

        float aspect = (float)tex.width / tex.height;

        // FIXED VIEW SIZE (no FOV math, no growth)
        float targetHeight = 6f;
        float targetWidth  = targetHeight * aspect;

        baseScale = new Vector3(targetWidth, targetHeight, 1f);

        currentZoom = 1f;
        activePhoto.localScale = baseScale;

        // keep same distance behavior as before
    }

    void ExitImageView()
    {
        viewingImage = false;
        ribbonLayout.UnlockNavigation();

        if (activePhoto != null)
        {
            // Reset cleanly so ribbon takes over
            activePhoto.localScale = new Vector3(2.2f, 2.2f, 1f);
        }

        activePhoto = null;
    }

    void UpdateZoom()
    {
        if (activePhoto == null) return;

        Transform cam = Camera.main.transform;

        Vector3 targetPos =
            cam.position + cam.forward * viewDistance;

        activePhoto.position = Vector3.Lerp(
            activePhoto.position,
            targetPos,
            Time.deltaTime * 8f
        );

        activePhoto.rotation =
            Quaternion.LookRotation(activePhoto.position - cam.position);

        // ----- FIXED ZOOM MODEL -----

        float pinch = receiver.pinchDistance;

        // Map pinch → zoom (NOT using current scale)
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, pinch);

        // Smooth zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * 8f);

        // Apply safely (no compounding)
        activePhoto.localScale = baseScale * currentZoom;
    }
}
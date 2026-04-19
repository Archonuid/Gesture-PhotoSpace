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

        Renderer renderer = activePhoto.GetComponent<Renderer>();

        if (renderer == null)
            return;

        Texture tex = renderer.material.mainTexture;

        if (tex == null)
            return;

        float width = tex.width;
        float height = tex.height;

        float aspect = width / height;

        float baseSize = 10f;

        float scaleX;
        float scaleY;

        if (aspect >= 1f)
        {
            scaleX = baseSize;
            scaleY = baseSize / aspect;
        }
        else
        {
            scaleX = baseSize * aspect;
            scaleY = baseSize;
        }

        activePhoto.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    void ExitImageView()
    {
        viewingImage = false;

        ribbonLayout.UnlockNavigation();

        activePhoto = null;
    }

    void UpdateZoom()
    {
        if (activePhoto == null)
            return;

        Transform cam = Camera.main.transform;

        Vector3 targetPos =
            cam.position +
            cam.forward * viewDistance;

        activePhoto.position = Vector3.Lerp(
            activePhoto.position,
            targetPos,
            Time.deltaTime * 8f
        );

        activePhoto.rotation =
            Quaternion.LookRotation(activePhoto.position - cam.position);

        float pinch = receiver.pinchDistance;

        float zoom = Mathf.Lerp(1f, 2.2f, receiver.pinchDistance);

        Vector3 targetScale = activePhoto.localScale.normalized * zoom * activePhoto.localScale.magnitude;

        activePhoto.localScale = Vector3.Lerp(
            activePhoto.localScale,
            targetScale,
            Time.deltaTime * 6f
        );
    }
}
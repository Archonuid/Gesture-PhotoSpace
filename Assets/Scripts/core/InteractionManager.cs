using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public UDPReceiver receiver;
    public LayoutController layoutController;
    public RibbonLayout ribbonLayout;
    public PinchZoom pinchZoom;

    AtomMotion[] atomMotions;
    int leftFrames = 0;
    int rightFrames = 0;
    int detectionThreshold = 10;

    enum State
    {
        Idle,
        Morph,
        Ribbon
    }

    State currentState = State.Idle;

    void Start()
    {
        atomMotions = FindObjectsByType<AtomMotion>(FindObjectsSortMode.None);

        ribbonLayout.enabled = false;
        pinchZoom.enabled = false;

        SetState(State.Idle);
    }

    void Update()
    {
        bool left = receiver.leftHandDetected;
        bool right = receiver.rightHandDetected;

        if (left)
            leftFrames++;
        else
            leftFrames = 0;

        if (right)
            rightFrames++;
        else
            rightFrames = 0;

        bool leftStable = leftFrames > detectionThreshold;
        bool rightStable = rightFrames > detectionThreshold;

        if ((leftStable && rightStable) || (!leftStable && !rightStable))
        {
            SetState(State.Idle);
        }
        else if (leftStable && !rightStable)
        {
            SetState(State.Morph);
        }
        else if (!leftStable && rightStable)
        {
            SetState(State.Ribbon);
        }
    }

    void SetState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (newState == State.Idle)
            ActivateIdle();

        if (newState == State.Morph)
            ActivateMorph();

        if (newState == State.Ribbon)
            ActivateRibbon();
    }

    void ActivateIdle()
    {
        layoutController.active = false;
        layoutController.enabled = false;

        ribbonLayout.enabled = false;
        pinchZoom.enabled = false;

        foreach (var atom in atomMotions)
            atom.enabled = true;

        ribbonLayout.UnlockNavigation();
    }

    void ActivateMorph()
    {
        layoutController.RefreshPhotos();

        layoutController.active = true;
        layoutController.enabled = true;

        ribbonLayout.enabled = false;
        pinchZoom.enabled = false;

        foreach (var atom in atomMotions)
            atom.enabled = false;

        ribbonLayout.UnlockNavigation();
    }

    void ActivateRibbon()
    {
        layoutController.active = false;
        layoutController.enabled = false;

        ribbonLayout.enabled = true;
        pinchZoom.enabled = true;

        foreach (var atom in atomMotions)
            atom.enabled = false;
    }
}
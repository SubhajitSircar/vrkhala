using UnityEngine;
using UnityEngine.XR;

public class SitController : MonoBehaviour
{
    [Header("References")]
    public Transform sitPoint;
    public Transform xrOrigin;

    private float originalY;
    private bool isSitting = false;
    private bool wasPressedLastFrame = false;

    void Start()
    {
        // Save original height
        originalY = xrOrigin.position.y;
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed))
        {
            if (pressed && !wasPressedLastFrame)
            {
                ToggleSitStand();
            }

            wasPressedLastFrame = pressed;
        }
    }

    void ToggleSitStand()
    {
        if (!isSitting)
            Sit();
        else
            Stand();

        isSitting = !isSitting;
    }

    void Sit()
    {
        // Move player to sofa (keep correct height)
        Vector3 newPos = sitPoint.position;
        newPos.y = xrOrigin.position.y; // DON'T change height
        xrOrigin.position = newPos;

        // Rotate player to face same direction as sitPoint (towards TV)
        Vector3 forward = sitPoint.forward;
        forward.y = 0;

        xrOrigin.rotation = Quaternion.LookRotation(forward);
    }

    void Stand()
    {
        // Only restore Y position (height)
        Vector3 pos = xrOrigin.position;
        pos.y = originalY;
        xrOrigin.position = pos;
    }
}
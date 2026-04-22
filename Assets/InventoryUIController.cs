using UnityEngine;
using UnityEngine.XR;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryUI;
    public Transform playerCamera;

    [Header("Position Settings")]
    public float distanceFromPlayer = 2f;
    public float heightOffset = -0.1f;

    [Header("Follow Settings")]
    public float followSpeed = 5f;

    private bool isOpen = false;
    private bool wasPressedLastFrame = false;

    void Start()
    {
        // Hide UI at start
        inventoryUI.SetActive(false);
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // Toggle inventory (B button recommended)
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressed))
        {
            if (pressed && !wasPressedLastFrame)
            {
                ToggleInventory();
            }

            wasPressedLastFrame = pressed;
        }

        // Smooth follow when open
        if (isOpen)
        {
            SmoothFollowUI();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryUI.SetActive(isOpen);
    }

    void SmoothFollowUI()
    {
        // Get forward direction (ignore vertical tilt)
        Vector3 forward = playerCamera.forward;
        forward.y = 0;
        forward.Normalize();

        // Target position in front of player
        Vector3 targetPosition = playerCamera.position + forward * distanceFromPlayer;

        // Keep UI at comfortable height (slightly below eyes)
        targetPosition.y = playerCamera.position.y + heightOffset;

        // Smooth movement
        inventoryUI.transform.position = Vector3.Lerp(
            inventoryUI.transform.position,
            targetPosition,
            Time.deltaTime * followSpeed
        );

        // FIX: Face player without tilting
        Vector3 direction = playerCamera.position - inventoryUI.transform.position;
        direction.y = 0;

        inventoryUI.transform.rotation = Quaternion.LookRotation(direction);
    }
}
using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory;
    public LayerMask groundLayer;

    [Header("Preview Materials")]
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Placement Settings")]
    public float distance = 5f;

    private GameObject previewObject;
    private float currentRotationY = 0f;
    private bool wasPressedLastFrame = false;
    private bool isValidPlacement = false;

    void Update()
    {
        HandlePreview();
        HandleRotation();
        HandlePlacement();
    }

    // ================= PREVIEW =================
    void HandlePreview()
    {
        GameObject prefab = inventory.GetSelectedItem();

        // Create preview
        if (prefab != null && previewObject == null)
        {
            previewObject = Instantiate(prefab);
            DisableColliders(previewObject); // IMPORTANT
        }

        // Destroy preview if deselected
        if (prefab == null && previewObject != null)
        {
            Destroy(previewObject);
            return;
        }

        if (previewObject == null) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, groundLayer))
        {
            Vector3 pos = hit.point + Vector3.up * 0.05f;

            previewObject.transform.position = pos;
            previewObject.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);

            // Check placement
            isValidPlacement = CheckValidPlacement();

            // Apply color
            ApplyMaterial(previewObject, isValidPlacement ? validMaterial : invalidMaterial);
        }
    }

    // ================= VALIDATION =================
    bool CheckValidPlacement()
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Collider[] colliders = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            previewObject.transform.rotation
        );

        foreach (Collider col in colliders)
        {
            // Ignore preview object itself
            if (col.transform.IsChildOf(previewObject.transform))
                continue;

            // Ignore ground
            if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
                continue;

            if (!col.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    // ================= ROTATION =================
    void HandleRotation()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
        {
            float rotateInput = axis.x;

            if (Mathf.Abs(rotateInput) > 0.2f)
            {
                currentRotationY += rotateInput * 100f * Time.deltaTime;
            }
        }
    }

    // ================= PLACEMENT =================
    void HandlePlacement()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            if (pressed && !wasPressedLastFrame)
            {
                if (isValidPlacement)
                {
                    PlaceObject();
                }
                else
                {
                    Debug.Log("Invalid placement!");
                }
            }

            wasPressedLastFrame = pressed;
        }
    }

    void PlaceObject()
    {
        GameObject prefab = inventory.GetSelectedItem();

        GameObject obj = Instantiate(prefab,
            previewObject.transform.position,
            previewObject.transform.rotation);

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            StartCoroutine(EnablePhysicsAfterDelay(rb));
        }

        Destroy(previewObject);
        inventory.ClearSelection();
    }

    // ================= HELPERS =================
    void ApplyMaterial(GameObject obj, Material mat)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
    }

    void DisableColliders(GameObject obj)
    {
        Collider[] cols = obj.GetComponentsInChildren<Collider>();

        foreach (Collider c in cols)
        {
            c.enabled = false;
        }
    }

    IEnumerator EnablePhysicsAfterDelay(Rigidbody rb)
    {
        rb.isKinematic = true;
        yield return new WaitForSeconds(0.1f);
        rb.isKinematic = false;
    }
}
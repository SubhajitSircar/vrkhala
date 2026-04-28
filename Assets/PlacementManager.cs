using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory;

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
            DisableColliders(previewObject);
        }

        // Destroy preview if deselected
        if (prefab == null && previewObject != null)
        {
            Destroy(previewObject);
            return;
        }

        if (previewObject == null) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            PlaceableObject placeable = previewObject.GetComponent<PlaceableObject>();

            if (placeable == null)
            {
                Debug.LogError("Missing PlaceableObject script!");
                return;
            }

            int hitLayer = hit.collider.gameObject.layer;
            bool canPlace = false;

            // ================= LAYER BASED SURFACE CHECK =================
            if (placeable.placementType == PlacementType.Floor &&
                hitLayer == LayerMask.NameToLayer("Floor"))
                canPlace = true;

            else if (placeable.placementType == PlacementType.Wall &&
                     hitLayer == LayerMask.NameToLayer("Wall"))
                canPlace = true;

            else if (placeable.placementType == PlacementType.Surface &&
                     hitLayer == LayerMask.NameToLayer("Surface"))
                canPlace = true;

            // ================= POSITION =================
            previewObject.transform.position = hit.point + hit.normal * 0.02f;

            // ================= ROTATION =================
            if (placeable.placementType == PlacementType.Wall)
            {
                previewObject.transform.rotation = Quaternion.LookRotation(-hit.normal);
            }
            else
            {
                previewObject.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
            }

            // ================= VALIDATION =================
            isValidPlacement = canPlace && CheckValidPlacement();

            // ================= COLOR =================
            ApplyMaterial(previewObject, isValidPlacement ? validMaterial : invalidMaterial);
        }
    }

    // ================= VALIDATION =================
    bool CheckValidPlacement()
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) return false;

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
            // Ignore preview itself
            if (col.transform.IsChildOf(previewObject.transform))
                continue;

            // ❗ Block only other furniture
            // Allow placement on top of surfaces (like TV stand)
            if (!col.isTrigger && col.CompareTag("Placeable"))
            {
                // Check if collider is BELOW the object
                if (col.bounds.max.y > previewObject.transform.position.y - 0.01f)
                {
                    return false; // overlapping from side or inside
                }
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

    // ================= MATERIAL =================
    void ApplyMaterial(GameObject obj, Material mat)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                {
                    m.color = mat.color;
                }
            }
        }
    }

    // ================= HELPERS =================
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
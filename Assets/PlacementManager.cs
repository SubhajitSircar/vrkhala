using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory;
    public LayerMask groundLayer;

    [Header("Preview Settings")]
    public Material previewMaterial;

    [Header("Placement Settings")]
    public float distance = 5f;

    private GameObject previewObject;
    private float currentRotationY = 0f;
    private bool wasPressedLastFrame = false;

    void Update()
    {
        HandlePreview();
        HandleRotation();
        HandlePlacement();
    }

    void HandlePreview()
    {
        GameObject prefab = inventory.GetSelectedItem();

        // Create preview if not exists
        if (prefab != null && previewObject == null)
        {
            previewObject = Instantiate(prefab);
            ApplyPreviewMaterial(previewObject);
        }

        // Destroy preview if no item selected
        if (prefab == null && previewObject != null)
        {
            Destroy(previewObject);
            return;
        }

        if (previewObject == null) return;

        // Move preview to hit point
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, groundLayer))
        {
            Vector3 pos = hit.point + Vector3.up * 0.05f;
            previewObject.transform.position = pos;

            previewObject.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
        }
    }

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

    void HandlePlacement()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            if (pressed && !wasPressedLastFrame)
            {
                PlaceObject();
            }

            wasPressedLastFrame = pressed;
        }
    }

    void PlaceObject()
    {
        GameObject prefab = inventory.GetSelectedItem();

        if (prefab == null || previewObject == null) return;

        Vector3 position = previewObject.transform.position;

        // ✅ Check overlap (collision)
        Collider[] colliders = Physics.OverlapBox(
            position,
            previewObject.transform.localScale / 2f,
            previewObject.transform.rotation
        );

        // If something is already there → don't place
        foreach (Collider col in colliders)
        {
            if (!col.isTrigger && col.gameObject != previewObject)
            {
                Debug.Log("Cannot place here!");
                return;
            }
        }

        // Place object
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

    void ApplyPreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.material = previewMaterial;
        }
    }

    IEnumerator EnablePhysicsAfterDelay(Rigidbody rb)
    {
        rb.isKinematic = true;
        yield return new WaitForSeconds(0.1f);
        rb.isKinematic = false;
    }
}
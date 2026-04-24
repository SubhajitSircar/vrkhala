using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRRayDelete : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public float holdTime = 0.5f;

    private float holdTimer = 0f;
    private GameObject currentTarget;
    private InputDevice leftDevice;

    void Start()
    {
        // Get LEFT controller
        leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        // Reconnect if needed
        if (!leftDevice.isValid)
        {
            leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        DetectObject();

        // 🔘 Y button = secondaryButton
        if (leftDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yPressed))
        {
            Debug.Log("Y pressed: " + yPressed);

            if (yPressed && currentTarget != null)
            {
                holdTimer += Time.deltaTime;

                Debug.Log("Holding delete...");

                if (holdTimer >= holdTime)
                {
                    Debug.Log("Deleted: " + currentTarget.name);
                    Destroy(currentTarget);

                    holdTimer = 0f;
                    currentTarget = null;
                }
            }
            else
            {
                holdTimer = 0f;
            }
        }
    }

    void DetectObject()
    {
        currentTarget = null;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log("Ray Hit: " + hit.collider.name);

            GameObject obj = hit.collider.transform.root.gameObject;

            if (obj.CompareTag("Placeable"))
            {
                currentTarget = obj;
            }
        }
    }
}
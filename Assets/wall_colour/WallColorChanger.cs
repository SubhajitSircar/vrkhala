using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRWallColorChanger : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public List<Material> wallMaterials;

    private int currentIndex = 0;

    void Update()
    {
        // 🎮 LEFT controller button (you can change)
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        bool buttonPressed;
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed) && buttonPressed)
        {
            TryChangeWallColor();
        }
    }

    void TryChangeWallColor()
    {
        RaycastHit hit;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();

                if (renderer != null)
                {
                    currentIndex = (currentIndex + 1) % wallMaterials.Count;
                    renderer.material = wallMaterials[currentIndex];
                }
            }
        }
    }
}
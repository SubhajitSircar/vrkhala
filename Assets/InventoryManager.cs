using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject[] furniturePrefabs;

    private GameObject selectedPrefab;

    public void SelectItem(int index)
    {
        selectedPrefab = furniturePrefabs[index];
        Debug.Log("Selected: " + selectedPrefab.name);
    }

    public GameObject GetSelectedItem()
    {
        return selectedPrefab;
    }

    // NEW: Clear selection after placing
    public void ClearSelection()
    {
        selectedPrefab = null;
        Debug.Log("Selection Cleared");
    }
}
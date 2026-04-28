using UnityEngine;

public enum PlacementType
{
    Floor,
    Wall,
    Surface
}

public class PlaceableObject : MonoBehaviour
{
    public PlacementType placementType;
}
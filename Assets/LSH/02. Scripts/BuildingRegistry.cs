using System.Collections.Generic;
using System;
using UnityEngine;

public class BuildingRegistry : MonoBehaviour
{
    public static BuildingRegistry Instance { get; private set; }

    private Dictionary<BuildingType, List<BuildingInstance>> buildingsByType
        = new Dictionary<BuildingType, List<BuildingInstance>>();

    public event Action<BuildingInstance> OnBuildingRegistered;
    public event Action<BuildingInstance> OnBuildingRemoved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(BuildingInstance building)
    {
        if (building == null || building.data == null)
            return;

        BuildingType type = building.data.buildingType;

        if (!buildingsByType.ContainsKey(type))
            buildingsByType[type] = new List<BuildingInstance>();

        if (!buildingsByType[type].Contains(building))
            buildingsByType[type].Add(building);

        OnBuildingRegistered?.Invoke(building);
    }

    public void Remove(BuildingInstance building)
    {
        if (building == null || building.data == null)
            return;

        BuildingType type = building.data.buildingType;

        if (buildingsByType.ContainsKey(type))
            buildingsByType[type].Remove(building);

        OnBuildingRemoved?.Invoke(building);
    }

    public List<BuildingInstance> GetBuildings(BuildingType type)
    {
        if (buildingsByType.TryGetValue(type, out var list))
            return list;

        return new List<BuildingInstance>();
    }
}
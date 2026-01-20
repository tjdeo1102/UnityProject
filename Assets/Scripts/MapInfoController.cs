using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MapSlopeSettings
{
    public float MaxSlopeAngle;
    public List<Material> TargetMaterialList;
}

public class MapInfoController : MonoBehaviour
{
    [Header("Map Slope Settings")]
    [SerializeField] List<MapSlopeSettings> mapSlopeSettingsList;
    [SerializeField] Mesh mapMesh;
    [SerializeField] Renderer mapRenderer;
    Dictionary<Material, float> materialToMaxSlopeAngle;
    Dictionary<int, int> triangleToSubMesh = new();

    void Awake()
    {
        SlotInit();
    }

    void SlotInit()
    {
        materialToMaxSlopeAngle = new Dictionary<Material, float>();
        foreach (var settings in mapSlopeSettingsList)
        {
            foreach (var mat in settings.TargetMaterialList)
            {
                if (!materialToMaxSlopeAngle.ContainsKey(mat))
                {
                    materialToMaxSlopeAngle.Add(mat, settings.MaxSlopeAngle);
                }
            }
        }

        // Cache triangleMesh
        for (int i = 0; i < mapMesh.subMeshCount; i++)
        {
            var mesh = mapMesh.GetSubMesh(i);
            for (int j = mesh.indexStart; j < mesh.indexStart + mesh.indexCount; j++ )
            {
                int triangleIndex = j / 3;
                triangleToSubMesh[triangleIndex] = i;
            }
        }
    }
    public float GetMaxSlopeAngleByHitInfo(RaycastHit hitInfo)
    {
        if (triangleToSubMesh.TryGetValue(hitInfo.triangleIndex, out int subMesh))
        {   
            Material hitMaterial = mapRenderer.sharedMaterials[subMesh];
            if (materialToMaxSlopeAngle.TryGetValue(hitMaterial, out float maxSlopeAngle))
            {
                return maxSlopeAngle;
            }
        }
        return 45f; // default value
    }
}

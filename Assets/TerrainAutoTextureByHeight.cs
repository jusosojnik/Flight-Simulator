using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TerrainAutoTextureByHeight : MonoBehaviour
{
    [Header("Target")]
    public Terrain terrain;

    [Header("Height Layers (world meters)")]
    public List<HeightLayerRule> heightRules = new List<HeightLayerRule>()
    {
        new HeightLayerRule { layerIndex = 0, centerHeight = 0f,   blendRange = 80f,  strength = 1f },
        new HeightLayerRule { layerIndex = 1, centerHeight = 120f, blendRange = 100f, strength = 1f },
        new HeightLayerRule { layerIndex = 2, centerHeight = 260f, blendRange = 120f, strength = 1f },
        new HeightLayerRule { layerIndex = 3, centerHeight = 420f, blendRange = 140f, strength = 1f },
    };

    [Header("Optional: Slope Rock Override")]
    public bool useSlopeRock = false;
    [Tooltip("Terrain layer index used for steep slopes (e.g. rock/cliff).")]
    public int rockLayerIndex = 4;
    [Range(0f, 90f)] public float rockSlopeStart = 35f;
    [Range(0f, 90f)] public float rockSlopeEnd = 55f;
    [Range(0f, 1f)] public float rockStrength = 1f;

    [Header("Options")]
    public bool normalizeAllLayers = true;
    public bool fillFallbackIfNoRuleHits = true;
    public int fallbackLayerIndex = 0;

    [Serializable]
    public class HeightLayerRule
    {
        [Tooltip("Index into TerrainData.terrainLayers")]
        public int layerIndex = 0;

        [Tooltip("World-space height (Y meters) where this layer is strongest")]
        public float centerHeight = 0f;

        [Tooltip("Blend width around center. Larger = softer transition")]
        public float blendRange = 100f;

        [Tooltip("Multiplier for this layer weight")]
        public float strength = 1f;
    }

    [ContextMenu("Apply Textures Now")]
    public void ApplyTexturesNow()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        if (!terrain)
        {
            Debug.LogError("No Terrain assigned/found.");
            return;
        }

        TerrainData td = terrain.terrainData;
        if (td == null)
        {
            Debug.LogError("Terrain has no TerrainData.");
            return;
        }

        int layers = td.alphamapLayers;
        if (layers <= 0 || td.terrainLayers == null || td.terrainLayers.Length == 0)
        {
            Debug.LogError("Terrain needs Terrain Layers assigned first.");
            return;
        }

        if (heightRules == null || heightRules.Count == 0)
        {
            Debug.LogError("No height rules configured.");
            return;
        }

        int w = td.alphamapWidth;
        int h = td.alphamapHeight;

        float[,,] alpha = new float[h, w, layers];

        Vector3 tPos = terrain.transform.position;

        for (int y = 0; y < h; y++)
        {
            float v = (h > 1) ? y / (float)(h - 1) : 0f;

            for (int x = 0; x < w; x++)
            {
                float u = (w > 1) ? x / (float)(w - 1) : 0f;

                // Clear
                for (int li = 0; li < layers; li++)
                    alpha[y, x, li] = 0f;

                // World height at alphamap sample
                float worldY = tPos.y + td.GetInterpolatedHeight(u, v);

                // 1) Height-based weights (supports arbitrary number of layers)
                float totalHeightWeight = 0f;

                for (int i = 0; i < heightRules.Count; i++)
                {
                    HeightLayerRule rule = heightRules[i];
                    if (rule == null) continue;

                    int li = rule.layerIndex;
                    if (li < 0 || li >= layers) continue;

                    float range = Mathf.Max(0.0001f, rule.blendRange);
                    float strength = Mathf.Max(0f, rule.strength);

                    // Bell-shaped weight around centerHeight:
                    // 1.0 at center, fading to 0 outside +/- blendRange
                    float dist = Mathf.Abs(worldY - rule.centerHeight);
                    float t = Mathf.Clamp01(1f - (dist / range));
                    float wRule = Mathf.SmoothStep(0f, 1f, t) * strength;

                    alpha[y, x, li] += wRule;
                    totalHeightWeight += wRule;
                }

                // Fallback if no height rule contributed
                if (fillFallbackIfNoRuleHits && totalHeightWeight <= 0.00001f)
                {
                    if (fallbackLayerIndex >= 0 && fallbackLayerIndex < layers)
                    {
                        alpha[y, x, fallbackLayerIndex] = 1f;
                        totalHeightWeight = 1f;
                    }
                }

                // Normalize height weights before slope override (optional, but cleaner)
                if (normalizeAllLayers && totalHeightWeight > 0.00001f)
                {
                    for (int li = 0; li < layers; li++)
                        alpha[y, x, li] /= totalHeightWeight;
                }

                // 2) Optional slope rock override
                if (useSlopeRock && rockLayerIndex >= 0 && rockLayerIndex < layers)
                {
                    float slopeDeg = td.GetSteepness(u, v); // 0..90
                    float s = Mathf.InverseLerp(rockSlopeStart, rockSlopeEnd, slopeDeg);
                    float wRock = Mathf.SmoothStep(0f, 1f, s) * rockStrength;
                    wRock = Mathf.Clamp01(wRock);

                    if (wRock > 0f)
                    {
                        // Reduce all current layers proportionally
                        float baseRemain = 1f - wRock;
                        for (int li = 0; li < layers; li++)
                            alpha[y, x, li] *= baseRemain;

                        // Add rock layer
                        alpha[y, x, rockLayerIndex] += wRock;
                    }
                }

                // Final normalize
                float sum = 0f;
                for (int li = 0; li < layers; li++)
                    sum += alpha[y, x, li];

                if (sum > 0.00001f)
                {
                    for (int li = 0; li < layers; li++)
                        alpha[y, x, li] /= sum;
                }
                else
                {
                    int fallback = Mathf.Clamp(fallbackLayerIndex, 0, layers - 1);
                    alpha[y, x, fallback] = 1f;
                }
            }
        }

        td.SetAlphamaps(0, 0, alpha);
        Debug.Log("Terrain textures applied (multi-layer).");
    }
}
using UnityEngine;
using System.Collections.Generic;

public class ScatterObjects : MonoBehaviour
{
    [Header("Spawner Settings")]
    public List<GameObject> prefabs = new List<GameObject>();
    public int amount = 10;
    public Collider2D areaCollider;
    public bool parentToSpawner = true;

    [ContextMenu("Scatter")]
    private void Scatter()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomPos = GetRandomPointInCollider(areaCollider);
            GameObject chosenPrefab = prefabs[Random.Range(0, prefabs.Count)];
            GameObject obj = Instantiate(chosenPrefab, randomPos, Quaternion.identity);

            if (parentToSpawner)
                obj.transform.SetParent(transform);
        }
    }

    private Vector2 GetRandomPointInCollider(Collider2D col)
    {
        // BoxCollider2D
        if (col is BoxCollider2D box)
        {
            Vector2 size = box.size;
            Vector2 center = box.offset;
            Vector2 randomPoint = new Vector2(
                Random.Range(-size.x / 2f, size.x / 2f),
                Random.Range(-size.y / 2f, size.y / 2f)
            );
            return box.transform.TransformPoint(center + randomPoint);
        }

        // CircleCollider2D
        if (col is CircleCollider2D circle)
        {
            Vector2 center = circle.offset;
            float radius = circle.radius;
            Vector2 randomPoint = Random.insideUnitCircle * radius;
            return circle.transform.TransformPoint(center + randomPoint);
        }

        // PolygonCollider2D
        if (col is PolygonCollider2D poly)
        {
            Bounds bounds = poly.bounds;
            for (int i = 0; i < 30; i++)
            {
                Vector2 testPoint = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
                if (poly.OverlapPoint(testPoint))
                    return testPoint;
            }
            Debug.LogWarning("Could not find a valid point inside polygon; returning center.");
            return bounds.center;
        }

        return col.bounds.center;
    }
}

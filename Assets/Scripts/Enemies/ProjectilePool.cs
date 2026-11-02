using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    private readonly List<HomingProjectile> items = new List<HomingProjectile>();
    private Material ownedMaterial;
    public void Initialize(int count, Sprite sprite, Material material, Color color, float radius, float trailTime)
    {
        if (items.Count != 0) return;
        if (material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) material = ownedMaterial = new Material(shader);
        }
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Homing Projectile " + (i + 1));
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            int layer = LayerMask.NameToLayer("EnemyProjectile");
            if (layer >= 0) go.layer = layer;
            HomingProjectile projectile = go.AddComponent<HomingProjectile>();
            projectile.Initialize(this, sprite, material, color, Mathf.Max(0.01f, radius), trailTime);
            items.Add(projectile);
        }
    }
    public HomingProjectile Rent()
    {
        foreach (HomingProjectile item in items)
            if (!item.InUse) { item.Reserve(); return item; }
        return null;
    }
    public void Return(HomingProjectile projectile) { if (projectile != null) projectile.ResetForPool(); }
    public void ReturnAll() { foreach (HomingProjectile item in items) if (item != null) item.ResetForPool(); }
    private void OnDestroy() { if (ownedMaterial != null) Destroy(ownedMaterial); }
}

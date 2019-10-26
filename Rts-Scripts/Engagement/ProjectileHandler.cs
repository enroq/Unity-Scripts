using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    static List<BaseProjectile> m_ProjectileCache = new List<BaseProjectile>();

    void Update()
    {
        BaseProjectile[] projectiles = m_ProjectileCache.ToArray();
        for (int i = projectiles.Length - 1; i >= 0; i--)
        {
            if (projectiles[i] != null && projectiles[i].gameObject.activeInHierarchy)
                projectiles[i].ProcessProjectileMovement(Time.deltaTime);
            else
                m_ProjectileCache.Remove(projectiles[i]);
        }
    }

    internal void AddProjectileToCache(BaseProjectile projectile)
    {
        if (projectile != null && !m_ProjectileCache.Contains(projectile))
            m_ProjectileCache.Add(projectile);
    }
}

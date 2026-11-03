using System.Collections.Generic;
using UnityEngine;

// Bắn đạn ở mốc trúng của đòn có EnemyAttackSO.projectile: từ child "muzzle" theo hướng mặt (tỏa đều spreadAngle),
// hoặc rải từ trên cao xuống quanh mục tiêu (mưa tên). Gắn trên enemy cạnh EnemyController.
public class EnemyShooter : MonoBehaviour, IEnemyAttackHandler
{
    [Tooltip("Điểm ra đạn mặc định (EnemyAttackSO.muzzle trống)")]
    [SerializeField] private Transform defaultMuzzle;

    private readonly Dictionary<string, Transform> muzzles = new Dictionary<string, Transform>();

    public void OnAttackHit(EnemyController enemy, EnemyAttackSO attack, int hitIndex)
    {
        if (attack.projectile == null || attack.projectileDamage == null) return;

        int count = Mathf.Max(1, attack.projectileCount);
        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1); // 0..1 trên dải tỏa
            Vector2 pos, dir;
            if (attack.projectileOrigin == ProjectileOrigin.AboveTarget && enemy.HasTarget)
            {
                Vector2 target = enemy.Target.position;
                pos = new Vector2(target.x + (t - 0.5f) * attack.rainWidth, target.y + attack.rainHeight);
                dir = Vector2.down;
            }
            else
            {
                pos = GetMuzzle(attack.muzzle).position;
                float angle = (t - 0.5f) * attack.spreadAngle * Mathf.Deg2Rad;
                dir = new Vector2(Mathf.Cos(angle) * enemy.FacingDirection, Mathf.Sin(angle));
            }
            EnemyProjectile p = ObjectPool.Spawn(attack.projectile, pos, Quaternion.identity);
            p.Launch(dir, enemy.CreateAttackData(attack.projectileDamage));
        }
    }

    private Transform GetMuzzle(string muzzleName)
    {
        if (string.IsNullOrEmpty(muzzleName))
            return defaultMuzzle != null ? defaultMuzzle : transform;
        if (!muzzles.TryGetValue(muzzleName, out Transform t) || t == null)
        {
            t = FindChild(transform, muzzleName);
            if (t == null)
            {
                Debug.LogWarning($"[EnemyShooter] {name} không có child '{muzzleName}'", this);
                t = defaultMuzzle != null ? defaultMuzzle : transform;
            }
            muzzles[muzzleName] = t;
        }
        return t;
    }

    private static Transform FindChild(Transform root, string childName)
    {
        foreach (Transform c in root)
        {
            if (c.name == childName) return c;
            Transform deeper = FindChild(c, childName);
            if (deeper != null) return deeper;
        }
        return null;
    }
}

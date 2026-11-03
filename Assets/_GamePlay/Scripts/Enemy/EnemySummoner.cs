using System.Collections.Generic;
using UnityEngine;

// Triệu hồi minion ở mốc trúng của đòn có EnemyAttackSO.summon (qua ObjectPool), xen kẽ 2 bên thân.
// Giới hạn số minion còn sống (maxSummons) để boss không đẻ vô hạn.
public class EnemySummoner : MonoBehaviour, IEnemyAttackHandler
{
    private readonly List<EnemyController> alive = new List<EnemyController>();

    public int AliveCount
    {
        get
        {
            alive.RemoveAll(m => m == null || !m.gameObject.activeInHierarchy || !m.Health.IsAlive);
            return alive.Count;
        }
    }

    public void OnAttackHit(EnemyController enemy, EnemyAttackSO attack, int hitIndex)
    {
        if (attack.summon == null) return;

        int room = attack.maxSummons - AliveCount;
        int count = Mathf.Min(Mathf.Max(1, attack.summonCount), room);
        for (int i = 0; i < count; i++)
        {
            float side = i % 2 == 0 ? enemy.FacingDirection : -enemy.FacingDirection;
            float dist = attack.summonDistance * (1 + i / 2);
            Vector3 pos = enemy.transform.position + Vector3.right * (side * dist);
            EnemyController minion = ObjectPool.Spawn(attack.summon, pos, Quaternion.identity);
            alive.Add(minion);
        }
    }

    // Boss chết / encounter reset => dọn minion
    public void DespawnAll()
    {
        foreach (EnemyController m in alive)
        {
            if (m == null || !m.gameObject.activeInHierarchy) continue;
            if (ObjectPool.IsSpawned(m.gameObject)) ObjectPool.Recycle(m.gameObject);
            else m.gameObject.SetActive(false);
        }
        alive.Clear();
    }
}

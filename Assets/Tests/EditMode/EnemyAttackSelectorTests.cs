using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Chọn đòn enemy (tầm / phase / cooldown / weight), tầm tiếp cận, phase theo % máu, độ dài pha ra đòn.
public class EnemyAttackSelectorTests
{
    private readonly List<Object> created = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (var o in created)
            if (o != null) Object.DestroyImmediate(o);
        created.Clear();
    }

    private EnemyAttackSO Attack(float minRange, float maxRange, float weight = 1f, int minPhase = 0, int maxPhase = 99,
                                 EnemyAttackMovement movement = EnemyAttackMovement.None)
    {
        var a = ScriptableObject.CreateInstance<EnemyAttackSO>();
        a.minRange = minRange; a.maxRange = maxRange; a.weight = weight;
        a.minPhase = minPhase; a.maxPhase = maxPhase; a.movement = movement;
        created.Add(a);
        return a;
    }

    [Test]
    public void Choose_OnlyAttacksInRange()
    {
        var close = Attack(0f, 1.5f);
        var far = Attack(3f, 8f);
        var list = new List<EnemyAttackSO> { close, far };

        Assert.AreSame(close, EnemyAttackSelector.Choose(list, 1f, 0, null, 0.99f));
        Assert.AreSame(far, EnemyAttackSelector.Choose(list, 5f, 0, null, 0f));
        Assert.IsNull(EnemyAttackSelector.Choose(list, 2f, 0, null, 0.5f), "khoảng 1.5..3 không đòn nào tới");
    }

    [Test]
    public void Choose_RespectsPhaseWindow()
    {
        var basic = Attack(0f, 2f, maxPhase: 0);
        var rage = Attack(0f, 2f, minPhase: 1);
        var list = new List<EnemyAttackSO> { basic, rage };

        Assert.AreSame(basic, EnemyAttackSelector.Choose(list, 1f, 0, null, 0.99f));
        Assert.AreSame(rage, EnemyAttackSelector.Choose(list, 1f, 1, null, 0f));
    }

    [Test]
    public void Choose_SkipsAttacksNotReady()
    {
        var a = Attack(0f, 2f);
        var b = Attack(0f, 2f);
        var list = new List<EnemyAttackSO> { a, b };

        Assert.AreSame(b, EnemyAttackSelector.Choose(list, 1f, 0, x => x != a, 0f));
        Assert.IsNull(EnemyAttackSelector.Choose(list, 1f, 0, x => false, 0f));
    }

    [Test]
    public void Choose_PicksByWeight()
    {
        var light = Attack(0f, 2f, weight: 1f);
        var heavy = Attack(0f, 2f, weight: 3f);
        var zero = Attack(0f, 2f, weight: 0f);
        var list = new List<EnemyAttackSO> { light, heavy, zero };

        // tổng 4: [0, 1) -> light, [1, 4) -> heavy; weight 0 không bao giờ được chọn
        Assert.AreSame(light, EnemyAttackSelector.Choose(list, 1f, 0, null, 0.2f));
        Assert.AreSame(heavy, EnemyAttackSelector.Choose(list, 1f, 0, null, 0.3f));
        Assert.AreSame(heavy, EnemyAttackSelector.Choose(list, 1f, 0, null, 0.9999f));
    }

    [Test]
    public void MaxRange_IgnoresRetreatAndOtherPhases()
    {
        var melee = Attack(0f, 1.2f);
        var dashBack = Attack(0f, 5f, movement: EnemyAttackMovement.Retreat);
        var rageOnly = Attack(0f, 9f, minPhase: 1);
        var list = new List<EnemyAttackSO> { melee, dashBack, rageOnly };

        Assert.AreEqual(1.2f, EnemyAttackSelector.MaxRange(list, 0), 1e-5f);
        Assert.AreEqual(9f, EnemyAttackSelector.MaxRange(list, 1), 1e-5f);
        Assert.AreEqual(0f, EnemyAttackSelector.MaxRange(null, 0));
    }

    [Test]
    public void PhaseForHealth_CountsCrossedThresholds()
    {
        var data = ScriptableObject.CreateInstance<EnemyDataSO>();
        created.Add(data);
        data.phases = new List<EnemyPhase>
        {
            new EnemyPhase { healthFraction = 0.7f },
            new EnemyPhase { healthFraction = 0.35f },
        };

        Assert.AreEqual(0, data.PhaseForHealth(1f));
        Assert.AreEqual(0, data.PhaseForHealth(0.71f));
        Assert.AreEqual(1, data.PhaseForHealth(0.7f));
        Assert.AreEqual(1, data.PhaseForHealth(0.5f));
        Assert.AreEqual(2, data.PhaseForHealth(0.2f));
        Assert.IsNull(data.GetPhase(0));
        Assert.AreSame(data.phases[1], data.GetPhase(2));
        Assert.IsNull(data.GetPhase(3));
    }

    [Test]
    public void ResolveActiveDuration_ClipOrOverride_NeverBeforeLastHit()
    {
        var a = Attack(0f, 1f);
        a.hitTimes = new[] { 0.1f, 0.6f };

        a.activeDuration = 0f;
        Assert.AreEqual(0.8f, a.ResolveActiveDuration(0.8f), 1e-5f, "0 = theo clip");
        Assert.AreEqual(0.6f, a.ResolveActiveDuration(0f), 1e-5f, "không có clip => mốc trúng cuối");

        a.activeDuration = 0.3f;
        Assert.AreEqual(0.6f, a.ResolveActiveDuration(1f), 1e-5f, "không kết thúc trước mốc trúng cuối");
        a.activeDuration = 1.5f;
        Assert.AreEqual(1.5f, a.ResolveActiveDuration(0.8f), 1e-5f);
    }
}

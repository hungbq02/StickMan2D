using NUnit.Framework;
using UnityEngine;

public class AttackDataTests
{
    private static AttackData Make(int damage, float critChance, float critMultiplier)
    {
        return new AttackData(damage, 0f, critChance, critMultiplier, 0f, 0f, AttackEffect.None, Vector2.zero);
    }

    [Test]
    public void ZeroCritChance_NeverCrits()
    {
        for (int i = 0; i < 200; i++)
        {
            var data = Make(10, 0f, 2f);
            Assert.IsFalse(data.isCritical);
            Assert.AreEqual(10, data.FinalDamage);
        }
    }

    [Test]
    public void FullCritChance_AlwaysCrits_AndMultipliesDamage()
    {
        var data = Make(10, 1f, 1.5f);

        Assert.IsTrue(data.isCritical);
        Assert.AreEqual(15, data.FinalDamage);
    }

    [Test]
    public void DefaultTeam_IsNeutral()
    {
        Assert.AreEqual(Team.Neutral, Make(1, 0f, 1f).sourceTeam);
    }

    [Test]
    public void FromSO_CopiesFields()
    {
        var so = ScriptableObject.CreateInstance<AttackDataSO>();
        so.damage = 7;
        so.knockbackForce = 3f;
        so.attackEffect = AttackEffect.Burn;
        so.effectDuration = 2f;

        var data = new AttackData(so, new Vector2(1f, 2f));

        Assert.AreEqual(7, data.damage);
        Assert.AreEqual(3f, data.knockbackForce);
        Assert.AreEqual(AttackEffect.Burn, data.effectType);
        Assert.AreEqual(2f, data.effectDuration);
        Assert.AreEqual(new Vector2(1f, 2f), data.attackerPosition);
        Object.DestroyImmediate(so);
    }
}

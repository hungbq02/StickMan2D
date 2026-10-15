using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Health / Damageable / Shield. EditMode không gọi Awake/OnEnable => khởi tạo bằng SetMaxHealth và gọi Awake qua reflection.
public class HealthDamageTests
{
    private readonly List<GameObject> created = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (var go in created)
            if (go != null) Object.DestroyImmediate(go);
        created.Clear();
    }

    private static void CallAwake(Object target)
    {
        target.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?.Invoke(target, null);
    }

    private Damageable CreateDamageable(int maxHealth, Team team)
    {
        var go = new GameObject("Target");
        created.Add(go);
        var damageable = go.AddComponent<Damageable>(); // RequireComponent thêm Health + Death
        go.GetComponent<Health>().SetMaxHealth(maxHealth);

        var field = typeof(Damageable).GetField("team", BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(damageable, team);
        CallAwake(damageable);
        return damageable;
    }

    private static AttackData Hit(int damage, Team from, Vector2 attackerPos = default)
    {
        var data = new AttackData(damage, 0f, 0f, 1f, 0f, 0f, AttackEffect.None, attackerPos);
        data.sourceTeam = from;
        return data;
    }

    [Test]
    public void Health_TakeDamage_ClampsAtZero_AndFiresDeathOnce()
    {
        var go = new GameObject("H"); created.Add(go);
        var health = go.AddComponent<Health>();
        health.SetMaxHealth(20);
        int deaths = 0;
        health.OnDeath += () => deaths++;

        health.TakeDamage(15);
        health.TakeDamage(15);
        health.TakeDamage(15);

        Assert.AreEqual(0, health.Current);
        Assert.IsFalse(health.IsAlive);
        Assert.AreEqual(1, deaths);
    }

    [Test]
    public void Health_Heal_CappedAtMax_AndIgnoresNonPositive()
    {
        var go = new GameObject("H"); created.Add(go);
        var health = go.AddComponent<Health>();
        health.SetMaxHealth(50);
        health.TakeDamage(30);

        health.Heal(-5);
        Assert.AreEqual(20, health.Current);
        health.Heal(100);
        Assert.AreEqual(50, health.Current);
    }

    [Test]
    public void Damageable_DifferentTeam_TakesFinalDamage_AndRaisesOnDamaged()
    {
        var target = CreateDamageable(100, Team.Enemy);
        int reported = -1;
        target.OnDamaged += (dmg, data) => reported = dmg;

        target.TakeDamage(Hit(12, Team.Player));

        Assert.AreEqual(88, target.GetComponent<Health>().Current);
        Assert.AreEqual(12, reported);
    }

    [Test]
    public void Damageable_SameTeam_IsIgnored()
    {
        var target = CreateDamageable(100, Team.Enemy);

        target.TakeDamage(Hit(12, Team.Enemy));

        Assert.AreEqual(100, target.GetComponent<Health>().Current);
    }

    [Test]
    public void Damageable_Neutral_ReceivesFromEveryone()
    {
        var target = CreateDamageable(100, Team.Neutral);

        target.TakeDamage(Hit(5, Team.Neutral));
        target.TakeDamage(Hit(5, Team.Player));

        Assert.AreEqual(90, target.GetComponent<Health>().Current);
    }

    [Test]
    public void Damageable_Invulnerable_IgnoresHits()
    {
        var target = CreateDamageable(100, Team.Player);
        target.SetInvulnerable(10f);

        target.TakeDamage(Hit(30, Team.Enemy));

        Assert.IsTrue(target.IsInvulnerable);
        Assert.AreEqual(100, target.GetComponent<Health>().Current);
    }

    [Test]
    public void Shield_BlocksFromItsSide_NotFromBehind()
    {
        var owner = CreateDamageable(100, Team.Enemy);
        var shieldGo = new GameObject("Shield"); created.Add(shieldGo);
        shieldGo.transform.SetParent(owner.transform, false);
        shieldGo.transform.localPosition = new Vector3(1f, 0f, 0f); // khiên bên phải
        var shield = shieldGo.AddComponent<Shield>();
        shieldGo.GetComponent<Health>().SetMaxHealth(25);
        CallAwake(shield);
        owner.AddBlocker(shield);
        int blocked = 0;
        owner.OnBlocked += _ => blocked++;
        var health = owner.GetComponent<Health>();

        owner.TakeDamage(Hit(10, Team.Player, new Vector2(5f, 0f)));  // từ phải => chặn
        Assert.AreEqual(100, health.Current);
        Assert.AreEqual(1, blocked);

        owner.TakeDamage(Hit(10, Team.Player, new Vector2(-5f, 0f))); // từ trái => trúng
        Assert.AreEqual(90, health.Current);
    }
}

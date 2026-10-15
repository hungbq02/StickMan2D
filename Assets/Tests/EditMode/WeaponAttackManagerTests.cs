using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WeaponAttackManagerTests
{
    private GameObject go;
    private WeaponAttackManager manager;
    private readonly List<Object> assets = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("WAM");
        manager = go.AddComponent<WeaponAttackManager>();

        var sword = ScriptableObject.CreateInstance<WeaponDataSO>();
        sword.weaponType = WeaponType.Sword;
        sword.comboSteps = new List<AttackDataSO> { Attack("s1"), Attack("s2"), Attack("s3") };
        sword.airAttack = Attack("sAir");
        assets.Add(sword);

        typeof(WeaponAttackManager).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(manager, new List<WeaponDataSO> { sword });
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        foreach (var a in assets) Object.DestroyImmediate(a);
        assets.Clear();
    }

    private AttackDataSO Attack(string name)
    {
        var so = ScriptableObject.CreateInstance<AttackDataSO>();
        so.name = name;
        assets.Add(so);
        return so;
    }

    [Test]
    public void ComboSteps_AreOneBased()
    {
        Assert.AreEqual("s1", manager.GetAttackData(WeaponType.Sword, 1).name);
        Assert.AreEqual("s3", manager.GetAttackData(WeaponType.Sword, 3).name);
    }

    [Test]
    public void StepZero_IsAirAttack()
    {
        Assert.AreEqual("sAir", manager.GetAttackData(WeaponType.Sword, WeaponAttackManager.AirAttackStep).name);
    }

    [Test]
    public void OutOfRangeStep_ReturnsNull()
    {
        Assert.IsNull(manager.GetAttackData(WeaponType.Sword, 4));
        Assert.IsNull(manager.GetAttackData(WeaponType.Sword, -1));
    }

    [Test]
    public void UnknownWeapon_ReturnsNull_AndZeroCombo()
    {
        Assert.IsNull(manager.GetAttackData(WeaponType.Gun, 1));
        Assert.AreEqual(0, manager.GetComboStepCount(WeaponType.Gun));
        Assert.AreEqual(3, manager.GetComboStepCount(WeaponType.Sword));
    }
}

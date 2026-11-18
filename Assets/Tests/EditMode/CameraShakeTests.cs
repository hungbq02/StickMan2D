using NUnit.Framework;
using UnityEngine;

// Rung màn hình kiểu "trauma": cộng dồn có trần, giảm dần theo thời gian, hết thì offset = 0.
public class CameraShakeTests
{
    [SetUp]
    public void SetUp() => CameraShake.Reset();

    [TearDown]
    public void TearDown() => CameraShake.Reset();

    [Test]
    public void Add_StacksButClampsToOne()
    {
        CameraShake.Add(0.6f);
        CameraShake.Add(0.6f);
        Assert.AreEqual(1f, CameraShake.Trauma, 1e-4f);
    }

    [Test]
    public void NonPositive_IsIgnored()
    {
        CameraShake.Add(0f);
        CameraShake.Add(-1f);
        Assert.AreEqual(0f, CameraShake.Trauma);
        Assert.AreEqual(Vector3.zero, CameraShake.Sample(0.016f));
    }

    [Test]
    public void Trauma_DecaysToZero_AndOffsetStops()
    {
        CameraShake.Add(1f);
        Vector3 first = CameraShake.Sample(0.016f);
        Assert.Greater(first.magnitude, 0f);
        for (int i = 0; i < 300; i++) CameraShake.Sample(0.016f); // ~5s
        Assert.AreEqual(0f, CameraShake.Trauma);
        Assert.AreEqual(Vector3.zero, CameraShake.Sample(0.016f));
    }

    [Test]
    public void Offset_NeverExceedsMaxOffset()
    {
        CameraShake.Add(1f);
        for (int i = 0; i < 60; i++)
        {
            Vector3 o = CameraShake.Sample(0.016f);
            Assert.LessOrEqual(Mathf.Abs(o.x), CameraShake.MaxOffset + 1e-4f);
            Assert.LessOrEqual(Mathf.Abs(o.y), CameraShake.MaxOffset + 1e-4f);
            Assert.AreEqual(0f, o.z);
        }
    }
}

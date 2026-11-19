using NUnit.Framework;

// Bước tới / trần tốc độ / hãm khi ra đòn (AttackMotion) + nối combo sau khi rời đòn (MeleeAttackHandler.NextComboStep)
public class AttackMotionTests
{
    private const float Dt = 0.02f;
    private const float Decel = 30f;
    private const float Cap = 1.75f; // 0.35 × moveSpeed 5

    [Test]
    public void Lunge_MovesForwardAtDistanceOverDuration_ThenBrakes()
    {
        var m = new AttackMotion();
        m.Begin(0.12f, 0.08f); // 1.5 u/s < trần
        float vx = 0f, travelled = 0f;
        for (int i = 0; i < 4; i++) { vx = m.Step(vx, 1, Dt, Decel, Cap); travelled += vx * Dt; }
        Assert.AreEqual(0.12f, travelled, 1e-4f);
        Assert.IsFalse(m.IsLunging);
        Assert.AreEqual(1.5f - Decel * Dt, m.Step(vx, 1, Dt, Decel, Cap), 1e-4f);
    }

    [Test]
    public void Lunge_FacingLeft_GoesNegative()
    {
        var m = new AttackMotion();
        m.Begin(0.12f, 0.08f);
        Assert.AreEqual(-1.5f, m.Step(0f, -1, Dt, Decel, Cap), 1e-4f);
    }

    [Test]
    public void Lunge_FasterThanCap_IsCapped()
    {
        var m = new AttackMotion();
        m.Begin(1f, 0.1f); // 10 u/s
        Assert.AreEqual(Cap, m.Step(0f, 1, Dt, Decel, Cap), 1e-4f);
    }

    [Test]
    public void Lunge_DoesNotCarryRunSpeed()
    {
        var m = new AttackMotion();
        m.Begin(0.12f, 0.08f);
        Assert.AreEqual(1.5f, m.Step(5f, 1, Dt, Decel, Cap), 1e-4f);
    }

    [Test]
    public void NoLunge_EnteringFromRun_DropsToCapThenBrakesToZero()
    {
        var m = new AttackMotion();
        m.Begin(0f, 0.08f);
        Assert.IsFalse(m.IsLunging);
        float vx = m.Step(5f, 1, Dt, Decel, Cap);
        Assert.AreEqual(Cap - Decel * Dt, vx, 1e-4f);
        for (int i = 0; i < 20; i++) vx = m.Step(vx, 1, Dt, Decel, Cap);
        Assert.AreEqual(0f, vx);
    }

    [Test]
    public void Stop_EndsLunge()
    {
        var m = new AttackMotion();
        m.Begin(1f, 0.2f);
        m.Stop();
        Assert.IsFalse(m.IsLunging);
        Assert.AreEqual(0f, m.Step(0f, 1, Dt, Decel, Cap));
    }

    [TestCase(0, 0.1f, 1)] // chưa đánh
    [TestCase(1, 0.3f, 2)] // vừa rời bước 1, còn cửa sổ
    [TestCase(2, 0.4f, 3)] // đúng mép cửa sổ
    [TestCase(1, 0.41f, 1)] // hết cửa sổ
    [TestCase(3, 0.1f, 1)] // đã hết combo => lại từ đầu
    public void NextComboStep(int lastStep, float since, int expected)
    {
        Assert.AreEqual(expected, MeleeAttackHandler.NextComboStep(lastStep, since, 0.4f, 3));
    }
}

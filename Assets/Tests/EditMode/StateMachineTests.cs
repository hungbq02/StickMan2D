using System.Collections.Generic;
using NUnit.Framework;

public class StateMachineTests
{
    private class FakeState : IState
    {
        private readonly string name;
        private readonly List<string> log;
        public System.Action OnEnter;

        public FakeState(string name, List<string> log)
        {
            this.name = name;
            this.log = log;
        }

        public void Enter() { log.Add(name + ".Enter"); OnEnter?.Invoke(); }
        public void Update() { log.Add(name + ".Update"); }
        public void FixedUpdate() { }
        public void Exit() { log.Add(name + ".Exit"); }
    }

    [Test]
    public void ChangeState_ExitsOldThenEntersNew()
    {
        var log = new List<string>();
        var a = new FakeState("A", log);
        var b = new FakeState("B", log);
        var machine = new StateMachine<FakeState>();

        machine.Initialize(a);
        machine.ChangeState(b);

        Assert.AreEqual(new[] { "A.Enter", "A.Exit", "B.Enter" }, log);
        Assert.AreSame(b, machine.CurrentState);
    }

    [Test]
    public void ChangeState_InsideEnter_IsIgnored()
    {
        var log = new List<string>();
        var a = new FakeState("A", log);
        var b = new FakeState("B", log);
        var c = new FakeState("C", log);
        var machine = new StateMachine<FakeState>();
        machine.Initialize(a);
        b.OnEnter = () => machine.ChangeState(c);

        UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("Bỏ qua ChangeState"));
        machine.ChangeState(b);

        Assert.AreSame(b, machine.CurrentState);
        CollectionAssert.DoesNotContain(log, "C.Enter");
    }

    [Test]
    public void ChangeState_Null_KeepsCurrent()
    {
        var log = new List<string>();
        var a = new FakeState("A", log);
        var machine = new StateMachine<FakeState>();
        machine.Initialize(a);

        machine.ChangeState(null);

        Assert.AreSame(a, machine.CurrentState);
    }

    [Test]
    public void Stop_ExitsCurrentAndClears()
    {
        var log = new List<string>();
        var a = new FakeState("A", log);
        var machine = new StateMachine<FakeState>();
        machine.Initialize(a);

        machine.Stop();
        machine.Update(); // không còn state => không gọi gì

        Assert.IsNull(machine.CurrentState);
        Assert.AreEqual(new[] { "A.Enter", "A.Exit" }, log);
    }
}

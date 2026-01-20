using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kore;


namespace Kore.Demo
{
    public class DemoObserver : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(DemoSequence());
        }

        IEnumerator DemoSequence()
        {
            Debug.Log("You should be seeing:\n1-method1A subscribe\n2-NotifyA\n3-method2A subscribe\n4-NotifyA\n5-method1A unsubscribe\n6- notify only 2A\n7-methodB subscribe and being notified");

            Debug.Log("++++ Subscribe First method for eventA");
            ObserverService.Subscribe<TestEventA>(Method1_A);

            yield return new WaitForSeconds(0.5f);
            ObserverService.Notify(new TestEventA());

            yield return new WaitForSeconds(0.5f);
            Debug.Log("++++ Subscribe Second method for eventA");
            ObserverService.Subscribe<TestEventA>(Method2_A);

            ObserverService.Notify(new TestEventA());

            yield return new WaitForSeconds(0.5f);
            Debug.Log("---- Unsubscribe First method for eventA");
            ObserverService.Unsubscribe<TestEventA>(Method1_A);

            yield return new WaitForSeconds(0.5f);
            ObserverService.Notify(new TestEventA());

            yield return new WaitForSeconds(0.5f);
            Debug.Log("++++ Subscribe method for eventB");
            ObserverService.Subscribe<TestEventB>(MethodB);

            ObserverService.Notify(new TestEventB());

            /////////enum event

            ObserverEnum eventType = (ObserverEnum)0;
            yield return new WaitForSeconds(0.5f);
            Debug.Log("++++ Subscribe MethodEnum_A for enum event");
            ObserverService.Subscribe(eventType, MethodEnumA);
            ObserverService.Notify(eventType);

            yield return new WaitForSeconds(0.5f);
            Debug.Log("++++ Subscribe MethodEnum_BBB for enum event");
            ObserverService.Subscribe(eventType, MethodEnumB);
            ObserverService.Notify(eventType);

            yield return new WaitForSeconds(0.5f);
            Debug.Log("----- Unsubscribe MethodEnum_A for enum event");
            ObserverService.Unsubscribe(eventType, MethodEnumA);
            ObserverService.Notify(eventType);
        }

        public void Method1_A(TestEventA data)
        {
            data.value += 10;
            Debug.Log("calback from First_method11111 for event AAAAA; value: " + data.value);
        }

        public void Method2_A(TestEventA data)
        {
            data.value += 1000;
            Debug.Log("calback from SECOND_method22 for event AAaaaa; value: " + data.value);
        }

        public void MethodB(TestEventB data)
        {
            Debug.Log("callback from method for event BBBBBBBBB");
        }


        public void MethodEnumA()
        {
            Debug.Log(">callback from methodENUM_A for enumEvent");
        }
        public void MethodEnumB()
        {
            Debug.Log(">callback from methodENUM_B for enumEvent");
        }
    }

    public class TestEventA : ObserverEvent { public int value = 0; }

    public class TestEventB : ObserverEvent { }
}
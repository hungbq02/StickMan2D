using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kore.Implement
{
    public class ObserverServiceWrapper<T> where T: ObserverEvent 
    {
        //public static List<T> subscribers = new List<T>();
        public delegate void DelegateMethod(T data);

        public static event DelegateMethod delegateEvent;

        public static void Notify(T data)
        {
            delegateEvent?.Invoke(data);
        }
    }

    public class ObserverServiceEnumWrapper
    {
        public delegate void DelegateMethod();
        public event DelegateMethod delegateEvent;

        public void Notify()
        {
            delegateEvent?.Invoke();
        }

    }
}
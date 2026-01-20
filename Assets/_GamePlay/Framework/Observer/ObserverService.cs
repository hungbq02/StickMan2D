using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kore.Implement;
using System;

namespace Kore
{
    public class ObserverService : MonoBehaviour
    {
        
        public static void Subscribe<T>(ObserverServiceWrapper<T>.DelegateMethod eventHandler) where T: ObserverEvent
        {
            ObserverServiceWrapper<T>.delegateEvent += eventHandler;
        }

        public static void Unsubscribe<T>(ObserverServiceWrapper<T>.DelegateMethod eventHandler) where T : ObserverEvent
        {
            ObserverServiceWrapper<T>.delegateEvent -= eventHandler;
        }

        public static void Notify<T>(T eventData) where T : ObserverEvent
        {
            ObserverServiceWrapper<T>.Notify(eventData);
        }



        static Dictionary<ObserverEnum, ObserverServiceEnumWrapper> enumToEventDict = new Dictionary<ObserverEnum, ObserverServiceEnumWrapper>();
        public static void Subscribe(ObserverEnum eventType, ObserverServiceEnumWrapper.DelegateMethod method)
        {
            if (!enumToEventDict.ContainsKey(eventType))
                enumToEventDict.Add(eventType, new ObserverServiceEnumWrapper());
            enumToEventDict[eventType].delegateEvent += method;
        }

        public static void Unsubscribe(ObserverEnum eventType, ObserverServiceEnumWrapper.DelegateMethod method)
        {
            if (!enumToEventDict.ContainsKey(eventType)) return;
            enumToEventDict[eventType].delegateEvent -= method;
        }

        public static void Notify(ObserverEnum eventType)
        {
            if (!enumToEventDict.ContainsKey(eventType)) return;
            enumToEventDict[eventType].Notify();
        }

    }

    
}
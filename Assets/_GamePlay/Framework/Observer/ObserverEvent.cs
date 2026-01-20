using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kore
{
    public class ObserverEvent
    {

    }

    public class PurchaseResultEvent : ObserverEvent
    {
        public string iapIdKey;
        public bool isSuccessful;
        public PurchaseResultEvent(string iapIdKey, bool isSuccessful)
        {
            this.iapIdKey = iapIdKey;
            this.isSuccessful = isSuccessful;
        }
    }

    public class WatchedAdEvent : ObserverEvent
    {
        public string adType;
        public WatchedAdEvent(string adType)
        {
            this.adType = adType;
        }
    }
}

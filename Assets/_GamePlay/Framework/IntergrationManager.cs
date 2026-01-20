//analytics https://firebase.google.com/docs/analytics/unity/start  demo https://github.com/firebase/quickstart-unity/blob/master/analytics/testapp/Assets/Firebase/Sample/Analytics/UIHandler.cs
//crashlytics https://firebase.google.com/docs/crashlytics/get-started?platform=unity demo https://github.com/firebase/quickstart-unity/blob/master/crashlytics/testapp/Assets/Firebase/Sample/Crashlytics/UIHandler.cs
//cloud message https://firebase.google.com/docs/cloud-messaging/unity/client demo https://github.com/firebase/quickstart-unity/blob/master/messaging/testapp/Assets/Firebase/Sample/Messaging/UIHandler.cs
//remote config https://firebase.google.com/docs/remote-config/get-started?platform=unity demo https://github.com/firebase/quickstart-unity/blob/master/remote_config/testapp/Assets/Firebase/Sample/RemoteConfig/UIHandler.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#endif
using System.Threading.Tasks;
using Kore.Utils.Core;

namespace Kore
{
    public class IntergrationManager : MonoBehaviour
    {
        public static IntergrationManager Instance;
        public static bool isInitialized = false;
#if FIREBASE
        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
#endif

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);

#if FIREBASE
            InitializeFirebase();
            Debug.Log("Initializing Firebase");
            //ObserverService.Subscribe(ObserverEnum.WatchAds, () => { LogAnalyticsCountEvent(SaveNumber.watchAdsCount); });
            //ObserverService.Subscribe(ObserverEnum.FinishOneGame, () => { LogAnalyticsCountEvent(SaveNumber.finishGameCount); });
            //ObserverService.Subscribe(ObserverEnum.FinishCollectorGame, () => { LogAnalyticsCountEvent(SaveNumber.finishCollectorGameCount); });
#endif
        }


#if FIREBASE
        private void InitializeFirebase()
        {

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    isInitialized = true;
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    //analytics
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);//Enabling data collection. By default it is enabled.
                    AdsManager.instance.AdEventListener += LogAdEvent;

                    //Messaging
                    Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

                    //remote config
                    var defaults = new Dictionary<string, object>();
                    var constantConfig = Service.Get<Bootstrap>().constant;
                    defaults.Add(RemoteParam.AdBreakDelay.ToString(), (int)RemoteParam.AdBreakDelay);//nên lấy từ constant config, giá trị enum chỉ là tạm thời
                    defaults.Add(RemoteParam.InterAdFromLvId.ToString(), (int)RemoteParam.InterAdFromLvId);
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
                        .ContinueWithOnMainThread(task => { Debug.Log("RemoteConfig configured and ready!"); });

                    FetchRemoteDataAsync();
                }
                else
                {
                    Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }

        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
        }
        public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
        }


        public Task FetchRemoteDataAsync()
        {
            //FetchAsync only fetches new data if the current data is older than the provided timespan. Otherwise it assumes the data is "recent enough", and does nothing.
            //By default the timespan is 12 hours
            Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.FromDays(1));
            return fetchTask.ContinueWithOnMainThread(OnFetchRemoteDataComplete);
        }
        void OnFetchRemoteDataComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled) Debug.Log("Fetch canceled.");
            else if (fetchTask.IsFaulted) Debug.Log("Fetch encountered an error.");
            else if (fetchTask.IsCompleted) Debug.Log("Fetch completed successfully!");

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
                    });

                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.LogError("Fetch remote failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.LogError("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    break;
            }
        }
#endif

        public int GetRemoteValueInt(RemoteParam remoteParam)
        {
#if FIREBASE
            return (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetState(remoteParam.ToString()).LongValue;
#endif
            //var constantConfig = Service.Get<Bootstrap>().constant;
            return (int)remoteParam;////nên lấy từ constant config, giá trị enum chỉ là tạm thời
            return -1;
        }
        void GetRemoteData()
        {
            var bootstrap = Service.Get<Bootstrap>();
            // bootstrap.thresholdBeforeInviCollector[0] = GetRemoteValueInt(RemoteParam.invite_to_collector_1);
            // bootstrap.thresholdBeforeInviCollector[1] = GetRemoteValueInt(RemoteParam.invite_to_collector_2);
            // bootstrap.collectorThreshold = GetRemoteValueInt(RemoteParam.collector_break);
            // bootstrap.startBannerThreshold = GetRemoteValueInt(RemoteParam.start_banner);
            // bootstrap.startInterstitialThreshold = GetRemoteValueInt(RemoteParam.start_inter);
            // bootstrap.rateCollectorThreshold = new HashSet<int>();
            // bootstrap.rateCollectorThreshold.Add(GetRemoteValueInt(RemoteParam.rate_time_1));
            // bootstrap.rateCollectorThreshold.Add(GetRemoteValueInt(RemoteParam.rate_time_2));

        }


        public void LogEvent(string eventStr, string parameter, int value)
        {
#if FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventStr, parameter, value);
#endif
        }

        public void LogEvent(string eventStr)
        {
#if FIREBASE
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventStr);
#endif
        }

        public void LogSpendCurrency(int amount, string item_name)
        {
#if FIREBASE
            Firebase.Analytics.Parameter[] p = {
                new Parameter(FirebaseAnalytics.ParameterValue, amount),
                new Parameter(FirebaseAnalytics.ParameterItemName, item_name)
            };
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSpendVirtualCurrency, p);
#endif
        }


        public void LogEvent(string eventStr, params AnalyticsParam[] analyticsParams)
        {
#if FIREBASE
            Firebase.Analytics.Parameter[] p = new Firebase.Analytics.Parameter[analyticsParams.Length];
            for (int i = 0; i < analyticsParams.Length; i++)
                if (string.IsNullOrEmpty(analyticsParams[i].contentStr))
                    p[i] = new Firebase.Analytics.Parameter(analyticsParams[i].paramName, analyticsParams[i].quantity);
                else
                    p[i] = new Firebase.Analytics.Parameter(analyticsParams[i].paramName, analyticsParams[i].contentStr);
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventStr, p);
#endif
        }

//        public void LogAdEvent(AdsManager.AdStatusData adStatusData)
//        {
//#if FIREBASE
//            string eventName = "";
//            Firebase.Analytics.Parameter[] p = null;
//            switch (adStatusData.adStatus)
//            {
//                case AdsManager.AdStatusEvent.RequestAd:
//                    eventName = "Ad_Request";
//                    p = new Parameter[] { new Parameter("adunitid", adStatusData.adunitid) };
//                    break;
//                case AdsManager.AdStatusEvent.ShowAd:
//                    eventName = "Ad_Shown";
//                    p = new Parameter[] { new Parameter("adunitid", adStatusData.adunitid) };
//                    break;
//                case AdsManager.AdStatusEvent.AdPaid:
//                    eventName = "Ad_Impression_Revenue";
//                    p = new Parameter[] { new Parameter("adunitid", adStatusData.adunitid),
//                                        new Parameter("value", adStatusData.value),
//                                        new Parameter("currency", adStatusData.currency)};
//                    break;
//            }
//            FirebaseAnalytics.LogEvent(eventName, p);

           
//#endif

//        }

    }

    public enum RemoteParam
    {
        AdBreakDelay = 90,
        InterAdFromLvId = 1,
    }

    public class AnalyticsParam
    {
        public string paramName;
        public int quantity;
        public string contentStr;
        public AnalyticsParam(string paramName, int quantity)
        {
            this.paramName = paramName;
            this.quantity = quantity;
        }
        public AnalyticsParam(string paramName, string contentStr)
        {
            this.paramName = paramName;
            this.contentStr = contentStr;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kore.Utils.Core;
using UnityEngine.Serialization;
using System;


public class Bootstrap : MonoBehaviour
{
    [Space(20)]
    [Header("Prefab")]
    [Tooltip("Đăng ký vào Service: Service.Get<PrefabRegistrySO>()")]
    [SerializeField] private PrefabRegistrySO prefabRegistry;

    [Tooltip("FPS mục tiêu (Android mặc định chỉ 30). Tắt vSync để giá trị này có hiệu lực cả trên PC / Editor")]
    [SerializeField] private int targetFrameRate = 60;

/*    [Space(20)]
    [Header("Popup")]
    public PopupWinGame popupWinGame;
    public PopupRevive popupRevive;
    public PopupSetting popupSetting;
    public PopupTutorial popupTutorial;
    public ToastMessage toastMessage; */
    
    [Space(20)]
    [Header("Animation Curve")]
    public AnimationCurve bounceCurve;

    public AnimationCurve inBack;
    public AnimationCurve consumerScaleCurve;
    public AnimationCurve outBack;
    // public AnimationCurve 
    
    
    
    void Awake()
    {
        if (Service.IsSet<Bootstrap>())
        {
            Destroy(this.gameObject);
            return;
        }

        Service.Set<Bootstrap>(this);
        Service.Set<PrefabRegistrySO>(prefabRegistry);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
        if (Screen.currentResolution.width > 720)
            Screen.SetResolution(720, Mathf.RoundToInt(720f * Screen.currentResolution.height / Screen.currentResolution.width), true);
        
    }

    
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kore.Utils.Core;
using UnityEngine.Serialization;
using System;
using UnityEngine.WSA;


public class Bootstrap : MonoBehaviour
{
    [Space(20)]
    [Header("Prefab")]
    public DamagePopup textDamagePrefab;
    public GameObject ghostSpritePrefab;
    public Bullet bulletPrefab;

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
    
    
    [HideInInspector] public Language curLanguage;
    
    void Awake()
    {
        if (Service.IsSet<Bootstrap>())
        {
            Destroy(this.gameObject);
            return;
        }

        Service.Set<Bootstrap>(this);
        if (Screen.currentResolution.width > 720)
            Screen.SetResolution(720, Mathf.RoundToInt(720f * Screen.currentResolution.height / Screen.currentResolution.width), true);
        
    }

    
    public static int levelColorOffset = 0;
    public Dictionary<int, int> levelColorMap = new Dictionary<int, int>();
}
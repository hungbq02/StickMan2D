using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Kore.Utils.Core;

public class PopupBase : MonoBehaviour
{
    public Transform content;

    private void OnEnable()
    {
        if (content != null)
        {
            content.transform.localScale = Vector3.one;
            content.transform.DOScale(1.1f, 0.2f).SetEase(Service.Get<Bootstrap>().bounceCurve);
        }
    }

    public virtual void OnCompleteOpenPopup()
    {

    }

    public virtual void OnClickCloseBtn()
    {
        if (content != null)
        {
             this.Recycle();
        }
    }
}

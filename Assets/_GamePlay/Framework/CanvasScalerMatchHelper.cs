//ProjectSetting>ScriptExecutionOrder: thêm script này vào và set số âm, để đảm bảo hàm awake được chạy trước các awake khác
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerMatchHelper : MonoBehaviour
{
    public CanvasScaler canvasScaler;

    void Awake()
    {
        if (canvasScaler == null) canvasScaler = GetComponent<CanvasScaler>();
        float ratio = Screen.height * 1f / Screen.width;
        if (ratio < 19.5f / 9f) canvasScaler.matchWidthOrHeight = 1; //match height
        else canvasScaler.matchWidthOrHeight = 0; //match width
    }
}
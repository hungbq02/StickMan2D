using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Kore.Utils.Core;

[ExecuteInEditMode]
public class LocalizeUGUI : MonoBehaviour
{
    public TextMeshProUGUI localizedText;
    public string localizeKEY;

    // Start is called before the first frame update
    void Start()
    {
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (localizedText == null)
            localizedText = GetComponent<TextMeshProUGUI>();
    }
#endif

}

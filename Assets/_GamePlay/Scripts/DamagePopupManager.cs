using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    #region Singleton

    public static DamagePopupManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    [SerializeField]
    private GameObject damagePopupPrefab;


    public void DisplayDamagePopup(int amount, Transform popupParent)
    {
        GameObject damagePopup = PoolManager.Instance.textDamagePool.GetObject();
        damagePopup.transform.position = popupParent.position;
        damagePopup.transform.SetParent(popupParent);
        damagePopup.transform.rotation = Quaternion.identity;
        damagePopup.SetActive(true);

        damagePopup.GetComponent<DamagePopup>().SetUp(amount);
    }
}
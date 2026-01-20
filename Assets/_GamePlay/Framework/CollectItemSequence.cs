using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kore;
using Kore.Utils.Core;
using DG.Tweening;

public class CollectItemSequence : MonoBehaviour
{
    public SpriteRenderer itemPrototye;

    // Start is called before the first frame update
    void Start()
    {
        if (Service.IsSet<CollectItemSequence>())
        {
            Destroy(this.gameObject);
            return;
        }
        Service.Set<CollectItemSequence>(this);
        DontDestroyOnLoad(this);
        ObjectPool.CreatePool(itemPrototye, 10);
        //SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    //private void ChangedActiveScene(Scene current, Scene next)
    //{
    //    ObjectPool.DestroyPooled(itemPrototye);
    //}

    public void CollectItems(Sprite sprite, int numOfItems, float minSpawnRadius, float maxSpawnRadius,
        float delayCollect, float collectDur,
        Vector3 from, Vector3 to, System.Action onCompleteCollecting = null)
    {
        StartCoroutine(CollectItemsRoutine(sprite, numOfItems, minSpawnRadius, maxSpawnRadius, delayCollect, collectDur, from, to, onCompleteCollecting));
    }

    IEnumerator CollectItemsRoutine(Sprite sprite, int numOfItems, float minSpawnRadius, float maxSpawnRadius,
        float delayCollect, float collectDur, 
        Vector3 from, Vector3 to, System.Action onCompleteCollecting = null)
    {
        List<SpriteRenderer> items = new List<SpriteRenderer>();
        for (int i = 0; i < numOfItems; i++)
        {
            items.Add(ObjectPool.Spawn(itemPrototye, this.transform, from));
            yield return null;
            items[i].sprite = sprite;
            items[i].transform.localScale = new Vector3(0.5f, 0.5f, 1);
            items[i].gameObject.SetActive(true);
            items[i].transform.DOMove(from 
                + Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector3.up * Random.Range(minSpawnRadius, maxSpawnRadius), 0.25f);
        }

        yield return new WaitForSeconds(0.25f + delayCollect);

        for(int i = 0; i< numOfItems; i++)
        {
            var item = items[i];
            item.transform.DOJump(to, to.y < from.y ? 10 : 2, 1, collectDur).OnComplete(() => { item.Recycle(); });
            yield return new WaitForSeconds(0.08f);
        }

        if (onCompleteCollecting != null) onCompleteCollecting();
    }
}

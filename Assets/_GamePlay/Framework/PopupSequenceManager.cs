using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kore.Utils.Core;
using System;
using UnityEngine.SceneManagement;

namespace Kore
{
    //Cần setup Project Settings > Script Executeion order, set 1 giá trị âm trước default time,
    ///để đảm bảo các script khác gọi đến thì đã init Awake trước
    [DefaultExecutionOrder(-5)]
    public class PopupSequenceManager : MonoBehaviour
    {
        Queue<PopupSequenceEntryData> sequenceQueue = new Queue<PopupSequenceEntryData>();
        
        [HideInInspector]
        public GameObject observedGO;
        [HideInInspector]
        public float entryCountdown;

        [HideInInspector]
        public PopupSequenceEntryData playingEntry;


        void Awake()
        {
            if (Service.IsSet<PopupSequenceManager>())
            {
                Destroy(this.gameObject);
                return;
            }
            Service.Set<PopupSequenceManager>(this);
            SceneManager.activeSceneChanged += ChangedActiveScene;//cẩn trọng khi add sequence từ awake, vì ChangedActiveScene được gọi sẽ clear sequence
        }


        void Update()
        {
            if (observedGO != null && !observedGO.activeSelf) MoveNext();
            else if (entryCountdown > 0)
            {
                entryCountdown -= Time.deltaTime;
                if (entryCountdown <= 0) MoveNext();
            }
        }

        public void MoveNext()
        {
            try
            {
                if (playingEntry != null)
                {
                    if (playingEntry.OnEndAction != null) playingEntry.OnEndAction();
                }

                //Debug.Break();
                observedGO = null;
                entryCountdown = 0;
                if (sequenceQueue.Count > 0)
                {
                    playingEntry = sequenceQueue.Dequeue();
                    if (playingEntry.OnStartAction != null) playingEntry.OnStartAction();
                    if (playingEntry.entryDuration > 0) entryCountdown = playingEntry.entryDuration;
                }
                else
                {
                    playingEntry = null;
                }
            } catch(System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void AddSequenceEntry(float entryDuration, bool autoPlay = true)
        {
            AddSequenceEntry(new PopupSequenceEntryData(entryDuration), autoPlay);
        }
        public void AddSequenceEntry(Action onStartAction, Action onEndAction = null, float entryDuration = -1, bool autoPlay = true)
        {
            AddSequenceEntry(new PopupSequenceEntryData(onStartAction, onEndAction, entryDuration), autoPlay);
        }
        public void AddSequenceEntry(PopupSequenceEntryData data, bool autoPlay = true)
        {
            sequenceQueue.Enqueue(data);
            if (playingEntry == null && autoPlay) MoveNext();
        }

        public void PlaySequence()
        {
            if (playingEntry == null) MoveNext();
        }

        public void Clear()
        {
            if(playingEntry!=null) if (playingEntry.OnEndAction != null) playingEntry.OnEndAction();
            playingEntry = null;
            sequenceQueue.Clear();
        }

        private void ChangedActiveScene(Scene current, Scene next)
        {
            Clear();
        }
    }

    public class PopupSequenceEntryData
    {
        public Action OnStartAction, OnEndAction;
        public float entryDuration;


        public PopupSequenceEntryData(float entryDuration)
        {
            this.entryDuration = entryDuration;
        }

        public PopupSequenceEntryData(Action onStartAction, Action onEndAction = null, float entryDuration=-1)
        {
            OnStartAction = onStartAction;
            OnEndAction = onEndAction;
            this.entryDuration = entryDuration;
        }

    }
}
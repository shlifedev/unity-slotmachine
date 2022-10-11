using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

#endif
[System.Serializable]
public class SlotMachineTikTokAni
{
    public float overMultiple = 1.2f;
    public float tik_time = 2.6f;
    public Ease tik_ease = Ease.OutQuad;
    public Ease tok_ease = Ease.OutExpo;
    public float tok_time = 0.35f;
    public float tok_delay = 0.0f;
    [HideInInspector]
    public Tween inTween;
    [HideInInspector]
    public Tween outTween;

}

public class SlotMachine : MonoBehaviour
{
    [FormerlySerializedAs("resultEntertainerText")] public Text resultText;
    public SlotMachineTikTokAni tiktokAni;
    public System.Action<Status> onStatusChange;

    public enum Status
    {
        Ready,
        Running,
        Skip,
        Finish
    }

    private Status slotStatus;
    private bool isSkipped = false;
    public Status SlotStatus
    {
        get { return slotStatus; }
        set
        {
            if (value != slotStatus)
            {
                onStatusChange?.Invoke(value);
            }

            this.slotStatus = value;
        }
    }

    public GameObject ItemPrefab;

    public Image skipEffect;
    public Image background;

    public List<SlotMachineTime> smTimes = new List<SlotMachineTime>();

    [SerializeField] private List<SlotMachineItem> slotItemList = new List<SlotMachineItem>();

    public Tween skipTween;


    public List<SlotMachineItem> SlotItemList
    {
        get => this.slotItemList;
        set => this.slotItemList = value;
    }

    private List<int> items;

    public float itemHeightUnit;

    private Coroutine routine = null;
    private Coroutine thunderBoltRoutine = null;
    private Coroutine fadeRoutine = null;

    public enum SMAnimationType
    {
        TikTok = 0
    }

    public SMAnimationType actionType;

    void Awake()
    {
        this.onStatusChange += OnSkip;
        this.onStatusChange += (Status x) => {  };

        this.onStatusChange += (Status x) =>
        {
            if (x == Status.Ready)
            {
                this.resultText.text = null;
            }

            if (x == Status.Running)
            {
                this.resultText.text = null;
            }

            if (x == Status.Finish)
            {
                this.resultText.text = GameTable.SlotMachine.Item.Get(this.items[0]).Name;
                StartCoroutine(SkipFade());
            }
        };
    }



    public bool IsRunning() => SlotStatus == Status.Running;
    public bool IsReady() => SlotStatus == Status.Ready;
    public bool IsFinish() => SlotStatus == Status.Finish;

    private void Clear()
    {
        for (int i = 0; i < slotItemList.Count; i++)
        {
            Destroy(slotItemList[i].gameObject);
        }

        slotItemList.Clear();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
    }

    public IEnumerator SkipFade()
    {
        this.skipEffect.color = new Color(1,1,1,1);
        float a = 1;
        while (this.skipEffect.color.a > 0)
        {
            a -= Time.deltaTime;
            this.skipEffect.color = new Color(1,1,1,a);
            yield return null;
        }

        fadeRoutine = null;
    }

    public void Intialize()
    {
        Clear();
        isSkipped = false;

        this.SlotStatus = Status.Ready;
        this.transform.localPosition = new Vector3(0, 0, 0);
        if(fadeRoutine != null) StopCoroutine(fadeRoutine);
        this.skipEffect.color = new Color(1, 1, 1, 0);
        this.background.color = Color.white;
    }

    public void SkipSlotMachine()
    {

        if (tiktokAni.inTween != null)
        {
            tiktokAni.inTween.Kill(false);
        }
        if (tiktokAni.outTween != null)
        {
            tiktokAni.outTween.Kill(false);
        }
        SlotStatus = Status.Skip;


    }

    void OnSkip(Status status)
    {
        if (status == Status.Skip)
        {
            isSkipped = true;
            if (thunderBoltRoutine != null)
            {
                StopCoroutine(thunderBoltRoutine);
            }

            if (routine != null)
                StopCoroutine(routine);
            var p = this.transform as RectTransform;
            p.anchoredPosition = Vector3.zero;
            p.GetChild(0).transform.localPosition = Vector3.zero;
            for (int i = 1; i < this.transform.childCount; i++)
            {
                var cRect = p.GetChild(i).transform as RectTransform;
                cRect.anchoredPosition = Vector3.positiveInfinity;
            }

            StartCoroutine(SkipFade());
            this.SlotStatus = Status.Finish;
        }
    }

    /// <summary>
    /// slot machine start
    /// result item must be set array position [0]
    /// must be equal slot machine items count % rotation couunt == 0
    /// </summary>
    /// <param name="items"></param>
    /// <param name="swapArray"></param>
    public void StartSlotMachine(List<int> items)
    {
        actionType = SMAnimationType.TikTok;
        Debug.Log("receive slotmachine items count : " + items.Count);
        if (this.SlotStatus == Status.Running)
        {
            SkipSlotMachine();
            return;
        }
        thunderBoltRoutine = null;
        routine = null;
        Intialize();
        this.background.color = Color.black;
        AddItems(items);
        this.items = items;
        routine = StartCoroutine(SlotMachineStart());

    }

    /// <summary>
    /// add slot items
    /// </summary>
    private void AddItems(List<int> itemList)
    {
        for (int i = 0; i < itemList.Count; i++)
        {

            var data = Instantiate(ItemPrefab);
            data.name = $"Item_{i}";
            data.transform.SetParent(this.transform, false);

            var rt = data.transform as RectTransform;
            rt.anchoredPosition = new Vector3(0, (i * itemHeightUnit), 0);

            var smItem = data.GetComponent<SlotMachineItem>();
            SlotItemList.Add(smItem);
            smItem.ItemOriginalArrayIndex = i;
            smItem.ItemIndex = itemList[i];
        }
    }

    public bool IsLastFaze(int faze)
    {
        if (smTimes.Count - 1 == faze)
            return true;
        else
            return false;
    }

    /// <summary>
    /// start slotmachine
    /// </summary>
    /// <returns></returns>
    IEnumerator SlotMachineStart()
    {
        RectTransform slotTransform = this.transform as RectTransform;
        slotTransform.anchoredPosition = Vector2.zero;
        SlotStatus = Status.Running;
        float stack_y = 0;
        float progressValue = 0;
        int rotatedCount = 0;
        float movedYDistance = 0;
        for (int i = 0; i < smTimes.Count; i++)
        {
            progressValue = 0;
            rotatedCount = 0;
            var targetCnt = smTimes[i].count;
            var target_pos = (itemHeightUnit * targetCnt);
            bool tiktokProc = false;

            while (rotatedCount != targetCnt && SlotStatus != Status.Skip)
            {

                void RotateSlotMachine()
                {
                    progressValue += Time.deltaTime / smTimes[i].time;
                    // rot progress
                    if (progressValue >= 1.0)
                    {
                        progressValue = 1;
                    }

                    // target pos
                    float value = (-(target_pos) * progressValue) - movedYDistance;
                    var current = slotTransform.anchoredPosition.y;

                    // pos calc
                    var newPosition = new Vector2(0, value);
                    slotTransform.anchoredPosition = newPosition;
                    var after = slotTransform.anchoredPosition.y;

                    //  y stack add
                    stack_y += Mathf.Abs(after - current);
                    if ((stack_y + 0.3f) >= itemHeightUnit)
                    {
                        stack_y -= itemHeightUnit;
                        Swap();
                        rotatedCount++;
                    }
                    if (rotatedCount == targetCnt)
                    {
                        movedYDistance += target_pos;
                    }
                }



                if (IsLastFaze(i) && actionType == SMAnimationType.TikTok)
                {

                    if (tiktokProc == false)
                    {
                        var targetPos = this.transform.localPosition.y - itemHeightUnit;
                        tiktokAni.outTween= this.transform.DOLocalMoveY(
                            this.transform.localPosition.y - itemHeightUnit * tiktokAni.overMultiple,
                            tiktokAni.tik_time);
                        tiktokAni.outTween.SetEase(Ease.OutQuad);
                        tiktokAni.outTween.SetDelay(0);
                        tiktokAni.outTween.onComplete = () =>
                        {
                            Debug.Log("Debug_OutTweenComplete");
                            tiktokAni.inTween = this.transform.DOLocalMoveY(targetPos, tiktokAni.tok_time);
                            tiktokAni.inTween.SetDelay(tiktokAni.tok_delay);
                            tiktokAni.inTween.SetEase(tiktokAni.tik_ease);
                            tiktokAni.inTween.onComplete += () =>
                            {
                                if (isSkipped == false)
                                {
                                    SlotStatus = Status.Finish;
                                }
                            };
                        };

                        if (tiktokAni.outTween.IsInitialized() == false)
                        {
                            RotateSlotMachine();
                        }

                        tiktokProc = true;
                    }
                    else
                    {
                        // wat tiktokProc End.
                    }
                }
                else
                {
                    RotateSlotMachine();
                }

                yield return null;
            }
        }
    }

    private void Swap()
    {

        var latestItemPos = (SlotItemList[SlotItemList.Count - 1].transform as RectTransform).anchoredPosition;

        var currentItemRect = (SlotItemList[0].transform as RectTransform);
        currentItemRect.anchoredPosition = latestItemPos + new Vector2(0, itemHeightUnit);

        var prevItem = slotItemList[0];
        for (int i = 1; i < SlotItemList.Count; i++)
            SlotItemList[i - 1] = SlotItemList[i];
        SlotItemList[SlotItemList.Count - 1] = prevItem;
    }
}

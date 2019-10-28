using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class SlotMachineTime
{
    public float time;
    public float count; 
}
public class SlotMachine : MonoBehaviour
{ 
    public System.Action<Status> onStatusChange;
    public enum Status { Ready, Running, Skip,  Finish }
    private Status slotStatus;
    public Status SlotStatus
    {
        get
        {
            return slotStatus;
        }
        set
        {
            if(value != slotStatus)
            {
                onStatusChange?.Invoke(value);
            }
            this.slotStatus = value;
        }
    }
    public List<SlotMachineTime> smTimes = new List<SlotMachineTime>();
    public GameObject ItemPrefab; 
    private List<SlotMachineItem> slotItemList = new List<SlotMachineItem>();
    public List<SlotMachineItem> SlotItemList { get => this.slotItemList; set => this.slotItemList = value; }
    private List<int> originalItemArray = null;
    public float itemHeightUnit;
    private Coroutine routine = null;
    void Awake()
    {
        this.onStatusChange += OnSkip;
        this.onStatusChange += (Status x) => { Debug.Log(x); }; 
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
        for(int i = 0; i <this.transform.childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
    }
    private void Initstallize()
    {
        Clear();
        this.SlotStatus = Status.Ready; 
        this.transform.localPosition = new Vector3(0, 0, 0);
    } 

    public void SkipSlotMachine()
    {
        SlotStatus = Status.Skip;
    }
    void OnSkip(Status status)
    {
        if(status == Status.Skip)
        {
            StopCoroutine(routine);
        }
    }
    /// <summary>
    /// Example StartSlotMachine(리스트, (스왑번호));
    /// </summary>
    /// <param name="items"></param>
    /// <param name="swapArray"></param>
    public void StartSlotMachine(List<int> items, int swapArrayNum = 0)
    {
        if (this.SlotStatus == Status.Running)
            return;

        Initstallize();

        if (swapArrayNum != 0)
        {
            //swap
            var swpaedIDX = items[swapArrayNum];
            items[swapArrayNum] = items[0];
            items[0] = swpaedIDX;
        }

        originalItemArray = items; 
        AddItems(items);  
        routine = StartCoroutine(SlotMachineStart());
    }
    private void Rotate(float time, bool isLast)
    {
        var targetPos = (this.transform.localPosition.y - itemHeightUnit);
        var t = this.transform.DOLocalMoveY(targetPos, time);
        t.SetEase(Ease.Linear);

        t.onComplete += () => {
            Swap();
            if (isLast)
            { 
                this.SlotStatus = Status.Finish;
            }
        }; 

        t.onUpdate += () =>
        {
            if (routine == null || SlotStatus == Status.Skip)
            {
                t.Kill();
                TweenSkip();
                 
            }
        };
     
    }
     


    private void TweenSkip()
    { 
        this.transform.localPosition = new Vector3(0, 0, 0);
        Clear();
        AddItems(originalItemArray);
        this.SlotStatus = Status.Finish; 
    }
    IEnumerator SlotMachineStart()
    {
        SlotStatus = Status.Running;
        for (int i = 0; i < smTimes.Count; i++)
        {
            for (int h = 0; h < smTimes[i].count; h++) // 3
            {
                float t = smTimes[i].time; 
                if(i == smTimes.Count-1)
                {
                    if(h == smTimes[i].count-1)
                    {
                        Rotate(t, true);
                        yield return new WaitForSeconds(t);
                        continue;
                    }
                }
             

                Rotate(t, false);
                yield return new WaitForSeconds(t);
                continue;
            }
        }
        SlotStatus = Status.Finish; 
        if (routine != null)
        {

            StopCoroutine(routine);
            routine = null;
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

    [ContextMenu("Time Calc & Array Check")]
    private void Log()
    {
        string strLog = null;
        float t = 0;
        float c = 0;
        for (int i = 0; i < smTimes.Count; i++)
        {
            t += (smTimes[i].time * smTimes[i].count);
            c += smTimes[i].count;
        }
        strLog += "Time :: " + t +"\n";
        var picked = (c % 3 == 0);
        if(picked)
        {
            strLog += "올바른 데이터" + "\n"; ;
        }
        else
        {
            strLog += "올바르지 않은 데이터. 모든 카운트 총합은 3의 배수여야합니다. 현재 카운트 : " + c + "\n"; ;
        }

        Debug.Log(strLog);
    }
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
            smItem.ItemIndex = (uint)itemList[i];
        }
    } 

}

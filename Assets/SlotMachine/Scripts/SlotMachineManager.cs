using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GameTable.SlotMachine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public class SlotMachineManager : MonoBehaviour
{
    public Text confirmText;
    public int oneGameDiamondPrice = 10;
     private SlotMachine[] slotMachines;
    public void SkipAllSlotMachine()
    {

        if (this.remainNoUseSlot)
        {
            foreach (var sm in slotMachines)
            {
                if (sm.SlotStatus == SlotMachine.Status.Ready)
                {
                    StartSlotMachine(sm);
                    sm.SkipSlotMachine();
                }
                else if (sm.SlotStatus == SlotMachine.Status.Running)
                {
                    sm.SkipSlotMachine();
                }
            }
        }
        else
        {
            foreach (var data in slotMachines)
            {
                data.Intialize();
            }
        }
    }

    private bool remainNoUseSlot = false;
    private void Update()
    {
        if (confirmText != null)
        {
            foreach (var sm in slotMachines)
            {
                if (sm.SlotStatus == SlotMachine.Status.Finish || sm.SlotStatus == SlotMachine.Status.Skip)
                {
                    remainNoUseSlot = false;
                }
                else if(sm.SlotStatus == SlotMachine.Status.Ready || sm.SlotStatus == SlotMachine.Status.Running)
                {
                    remainNoUseSlot = true;
                }
            }
            if (remainNoUseSlot)
            {
                this.confirmText.text = "Open All";
            }
            else
            {
                this.confirmText.text = "Retry";
            }
        }
    }


    private void Awake()
    {
        GameTable.SlotMachine.Item.Load();
        GameTable.SlotMachine.GachaGroup.Load();


        slotMachines = FindObjectsOfType<SlotMachine>();
    }

    public void StartSlotMachine(SlotMachine machine)
    {
        if (machine.SlotStatus == SlotMachine.Status.Running)
        {
            machine.SkipSlotMachine();
            return;
        }

        if (machine.SlotStatus == SlotMachine.Status.Finish || machine.SlotStatus == SlotMachine.Status.Skip)
        {
            return;
        }

        var list = GetTestRandomElement();
        machine.StartSlotMachine(list);


    }

    int PickGrade()
    {
        int pickedGrade = -1;
        float p = Random.Range((float)0, (float)1);
        if (p >= GachaGroup.list[0].Percentage)
        {
            pickedGrade = 1;
        }
        else
        {
            for (int i = 1; i < GachaGroup.list.Count; i++)
            {
                var gp = GachaGroup.list[i].Percentage;
                gp = p - gp;
                if (gp >= 0)
                {
                    pickedGrade =  GachaGroup.list[i].Index;
                    break;
                }
                else
                {
                     continue;
                }
            }
        }
        return pickedGrade;
    }


    int PickItem()
    {
        var pickGrade = PickGrade();
        var list = GameTable.SlotMachine.Item.list.FindAll(x => x.Grade == pickGrade);
        return list[Random.Range(0, list.Count)].Index;
    }

    List<int> GetTestRandomElement()
    {
        List<int> list = new List<int>();
        var rewardItem = PickItem();
        list.Add((rewardItem));
        for (int i = 0; i < 4; i++)
        {
            list.Add(GameTable.SlotMachine.Item.list[Random.Range(0, GameTable.SlotMachine.Item.list.Count)].Index);
        }
        return list;
    }
}

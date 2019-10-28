using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{ 
    public SlotMachine slotMachine;  
    // Start is called before the first frame update
    void Start()
    {
        var list = new List<int>() { 1, 2, 3 };
        if (slotMachine != null)
        {
            //결과 : 슬롯머신이 끝나면 리스트[2]번이 됨.
            slotMachine.StartSlotMachine(list, 2);
        }


        //슬롯머신 스킵누르면 해당함수 호출됨.
        if (false)
        {
            slotMachine.SkipSlotMachine();
        }

        if(false)
        {
            slotMachine.onStatusChange += (SlotMachine.Status e) => {
                if (e == SlotMachine.Status.Finish)
                {
                    //슬롯머신에 반짝이는 이펙트등을 추가할때 사용
                }
                if (e == SlotMachine.Status.Running)
                {
                    //슬롯머신이 시작되는 순간
                }
                if (e == SlotMachine.Status.Skip)
                {
                    //슬롯머신이 스킵된경우
                    //스킵을 해도 Finish로 넘어가짐.
                    //Skip -> (1frame wait) -> Finish  
                } 
            };
        }
       
    }

    // Update is called once per frame
    void Update()
    {

    }
}

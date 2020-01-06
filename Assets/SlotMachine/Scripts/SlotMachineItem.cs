using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineItem : MonoBehaviour
{
     public int ItemOriginalArrayIndex = 0;
     public int ItemIndex;
     public Image ItemImage;
     public Image ItemGradeBgImage;

     //random sprite. it is temp code.
     private void Start()
     {
         InitImage();
     }


     void InitImage()
     {
         var sp = AtlasManager.Instance.GetEntertainerSprite(ItemIndex);
         ItemImage.sprite =AtlasManager.Instance.GetEntertainerSprite(ItemIndex);
         ItemGradeBgImage.sprite = AtlasManager.Instance.GetGradeSprite(GameTable.SlotMachine.Item.Get(ItemIndex).Grade);
     }

 }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineItem : MonoBehaviour
{
   
    public int ItemOriginalArrayIndex = 0;
    
    /* put your game item index*/
    public uint ItemIndex { get; set; }

    /* this is temp for example. remove this */
    public List<Sprite> randomSprites = new List<Sprite>();
    private void Awake()
    {
        /* this is temp for example. remove this */
        this.GetComponent<Image>().sprite = randomSprites[Random.Range(0, randomSprites.Count - 1)];
    }

}

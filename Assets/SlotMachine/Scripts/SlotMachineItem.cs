using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineItem : MonoBehaviour
{
    public int ItemOriginalArrayIndex = 0;
    public uint ItemIndex { get; set; }

    /* Zone4M에 임포트 한 후엔 지울것 */
    public List<Sprite> randomSprites = new List<Sprite>();
    private void Awake()
    {
        /* Zone4M에 임포트 한 후엔 지울것 */
        this.GetComponent<Image>().sprite = randomSprites[Random.Range(0, randomSprites.Count - 1)];
    }

}

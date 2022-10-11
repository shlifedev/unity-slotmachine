using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasManager : MonoBehaviour
{
    static AtlasManager instance;

    public static AtlasManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AtlasManager>();
            }

            return instance;
        }
    }

    public SpriteAtlas uiAtlas;

    public Sprite GetGradeSprite(int grade)
    {
        return uiAtlas.GetSprite("Grade" + grade.ToString());
    }

    public Sprite GetEntertainerSprite(int index)
    {
        return uiAtlas.GetSprite(GameTable.SlotMachine.Item.Get(index).ResourcePath);
    }
}
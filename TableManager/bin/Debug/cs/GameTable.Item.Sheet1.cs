
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace GameTable.Item
{
    public class Sheet1
    {
static bool isLoaded = false;
	public static List<Sheet1>       list       = new List<Sheet1>();
	public static Dictionary<int, Sheet1> dict       = new Dictionary<int, Sheet1>();
	public int Index;
	public float Value;
 

#if !SERVER
        public static void Load()
        {
if(isLoaded) return;
isLoaded = true;
          var textAsset = Resources.Load("TableDatas/GameTable.Item.Sheet1") as TextAsset;
          var str = textAsset.text; 
          var loadedList = JsonConvert.DeserializeObject<List<Sheet1>>(str);
          for(int i = 0; i < loadedList.Count; i++)
          {
            
              var data = loadedList[i];
              if(loadedList != null)
              {
                    list.Add(loadedList[i]);
                    dict.Add(loadedList[i].Index, loadedList[i]);
              }
          }
    
        }
#else
        public static void Load()
        { 
if(isLoaded) return;
isLoaded = true;
          var str = File.ReadAllText("TableDatas/GameTable.Item.Sheet1" + ".txt");
          var loadedList = JsonConvert.DeserializeObject<List<Sheet1>>(str);
          for(int i = 0; i < loadedList.Count; i++)
          {
            
              var data = loadedList[i];
              if(loadedList != null)
              {
                    list.Add(loadedList[i]);
                    dict.Add(loadedList[i].Index, loadedList[i]);
              }
          }
    
        }

#endif
 

        public static Sheet1 Get(int index)
        {
           if(list.Count == 0) Load();
           bool exist = dict.ContainsKey(index);
           if(exist)
              return dict[index];
           else
              return null;
        }

    }
}
            
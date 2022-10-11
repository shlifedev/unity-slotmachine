
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace GameTable.Item
{
    public class Info
    {
static bool isLoaded = false;
	public static List<Info>       list       = new List<Info>();
	public static Dictionary<int, Info> dict       = new Dictionary<int, Info>();
	public int Index;
	public float Value;
 

#if !SERVER
        public static void Load()
        {
if(isLoaded) return;
isLoaded = true;
          var textAsset = Resources.Load("TableDatas/GameTable.Item.Info") as TextAsset;
          var str = textAsset.text; 
          var loadedList = JsonConvert.DeserializeObject<List<Info>>(str);
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
          var str = File.ReadAllText("TableDatas/GameTable.Item.Info" + ".txt");
          var loadedList = JsonConvert.DeserializeObject<List<Info>>(str);
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
 

        public static Info Get(int index)
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
            
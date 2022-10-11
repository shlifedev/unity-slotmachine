
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace GameTable.SlotMachine
{
    public class Item
    {
static bool isLoaded = false;
	public static List<Item>       list       = new List<Item>();
	public static Dictionary<int, Item> dict       = new Dictionary<int, Item>();
	public int Index;
	public string Name;
	public string ResourcePath;
	public int Grade;
 

#if !SERVER
        public static void Load()
        {
if(isLoaded) return;
isLoaded = true;
          var textAsset = Resources.Load("TableDatas/GameTable.SlotMachine.Item") as TextAsset;
          var str = textAsset.text; 
          var loadedList = JsonConvert.DeserializeObject<List<Item>>(str);
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
          var str = File.ReadAllText("TableDatas/GameTable.SlotMachine.Item" + ".txt");
          var loadedList = JsonConvert.DeserializeObject<List<Item>>(str);
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
 

        public static Item Get(int index)
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
            
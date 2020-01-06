using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace TB
{
    class Program
    {
        public class ReadTableDatas
        {
            public string ResourcesPath;
            public string SheetName;
            public string NameSpace;
            private int columsCount = 0;
            public List<string> DataNames= new List<string>();
            public List<string> DataTypes= new List<string>();
            public List<string> Datas = new List<string>();
            private List<int> IgnoreColIndex = new List<int>();

            public int ColumsCount 
            { 
                get => this.columsCount; set => this.columsCount = value;
            }

            public bool IsIgnoreType(string type)
            {
                if (type == "Desc")
                {
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 정상적인 데이터인지 확인.
            /// </summary> 
            public bool IsValid()
            {
                return Datas.Count % ColumsCount == 0;
            }

            public void Writer(JsonWriter writer, string type, string value)
            {
                if (type == "List<int>")
                {
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        writer.WriteValue(int.Parse(value));
                    }
                }
                if (type == "List<float>")
                {
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        writer.WriteValue(float.Parse(value));
                    }
                } 
            } 

            public string ToJson()
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);
                    JsonWriter writer = new JsonTextWriter(sw);  
                    writer.WriteStartArray();

                    for(int i = 0 ; i < ColumsCount; i++)
                    { 
                        if(DataTypes[i] == "desc" || DataTypes[i] == "Desc")
                        {
                            IgnoreColIndex.Add(i);
                        }
                    }
                    for (int i = 0; i < Datas.Count; i++)
                    {
                        var idx = i % ColumsCount;  
                        if (IgnoreColIndex.Contains(idx))
                        {
                            continue;
                        }
                        if (idx == 0)         //Json Object Start.
                            writer.WriteStartObject();

                        if (IsShellArray(Datas[i]) == false)
                        {
                            writer.WritePropertyName(DataNames[idx]);
                            writer.WriteValue(Datas[i]);
                        }
                        else //배열쓰기
                        {
                            //배열로변경
                            var shellArray = GetShellArray(Datas[i]);
                            //배열타입명
                            writer.WritePropertyName(DataNames[idx]);
                            //어레이시작
                            writer.WriteStartArray();
                            foreach (var shd in shellArray)
                            { 
                                Writer(writer, DataTypes[idx], shd );
                            }
                            //어레 끝
                            writer.WriteEndArray();
                        }
                        if (idx == ColumsCount - 1)         //Json Object End.
                            writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    return sb.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }


            public string ToCSharp()
            {
                ToJson();
                ReadTableDatas rtd = this;
                var cs = $@"
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace {rtd.NameSpace}
{{
    public class {rtd.SheetName}
    {{
static bool isLoaded = false;
{{ReplaceList}}
{{ReplaceDictionary}}
{{ReplaceValues}} 
{{ReplaceLoadFunction}} 
{{ReplaceGetFunction}}
    }}
}}
            ";
                void ReplaceValues()
                {
                    string values = null;
                    for (int i = 0; i < rtd.ColumsCount; i++)
                    {
                        if (IsIgnoreType(rtd.DataTypes[i])) continue;
                        values += $"\tpublic {rtd.DataTypes[i]} {rtd.DataNames[i]};\n";
                    }
                    cs = cs.Replace("{ReplaceValues}", values);
                }

                void ReplaceListAndDictionary()
                {
                    string list = "\tpublic static List<__?__>       list       = new List<__?__>();";
                    string dict = "\tpublic static Dictionary<int, __?__> dict       = new Dictionary<int, __?__>();";
                    list = list.Replace("__?__", rtd.SheetName);
                    dict = dict.Replace("__?__", rtd.SheetName);
                    cs = cs.Replace("{ReplaceList}", list);
                    cs = cs.Replace("{ReplaceDictionary}", dict);


                }

                void FunctionGeneration()
                {


                    string loadFunction = $@"
#if !SERVER
        public static void Load()
        {{
if(isLoaded) return;
isLoaded = true;
          var textAsset = Resources.Load(<__!__>) as TextAsset;
          var str = textAsset.text; 
          var loadedList = JsonConvert.DeserializeObject<List<{this.SheetName}>>(str);
          for(int i = 0; i < loadedList.Count; i++)
          {{
            
              var data = loadedList[i];
              if(loadedList != null)
              {{
                    list.Add(loadedList[i]);
                    dict.Add(loadedList[i].Index, loadedList[i]);
              }}
          }}
    
        }}
#else
        public static void Load()
        {{ 
if(isLoaded) return;
isLoaded = true;
          var str = File.ReadAllText(<__!__> + "".txt"");
          var loadedList = JsonConvert.DeserializeObject<List<{this.SheetName}>>(str);
          for(int i = 0; i < loadedList.Count; i++)
          {{
            
              var data = loadedList[i];
              if(loadedList != null)
              {{
                    list.Add(loadedList[i]);
                    dict.Add(loadedList[i].Index, loadedList[i]);
              }}
          }}
    
        }}

#endif
";


                    string getFunction = $@"
        public static {this.SheetName} Get(int index)
        {{
           if(list.Count == 0) Load();
           bool exist = dict.ContainsKey(index);
           if(exist)
              return dict[index];
           else
              return null;
        }}
";

                    loadFunction = loadFunction.Replace("<__!__>", $"\"{ResourcesPath}{this.NameSpace}.{this.SheetName}\"");
                    cs = cs.Replace("{ReplaceLoadFunction}", loadFunction);
                    cs = cs.Replace("{ReplaceGetFunction}", getFunction);

                }


                ReplaceValues();
                ReplaceListAndDictionary();
                FunctionGeneration();

                //출력
                //Console.WriteLine(cs);

                return cs;
            }
        }

        static bool IsShellArray(string arrayData)
        {
            if (arrayData.Length == 0 || arrayData.Length == 1) return false;
            if ((arrayData[0] == '(' || arrayData[0] == '[') && (arrayData[arrayData.Length - 1] == ']' || arrayData[arrayData.Length - 1] == ')'))
            {
                return true;
            }

            return false;
        }
        static List<string> GetShellArray(string arrayData)
        {
            List<string> shellList = new List<string>();
            string       writeBuffer = null;
            for (int i = 0; i < arrayData.Length; i++)
            {
                // (, [를 만나면  write 상태에 들어간다.
                if (IsShellArray(arrayData))
                {
                    arrayData = arrayData.Remove(0, 1);
                    arrayData = arrayData.Remove(arrayData.Length-1, 1);
                    var d = arrayData.Split(',');
                    foreach(var data in d)
                    {
                        shellList.Add(data);
                    } 
                }
            }
            return shellList;
        } 
         
   
        static void Convert(string filePath, string csPath, string jsonPath)
        {
            var cs_output_path = csPath;
            var json_output_path = jsonPath;

            FileInfo fi = new FileInfo(filePath);
            var copyPath = filePath.Replace(".xlsx", "_Copy.xlsx");
            System.IO.File.Copy(filePath, filePath.Replace(".xlsx", "_Copy.xlsx"));
            FileInfo fi_copy = new FileInfo(copyPath);
            using (var stream = File.Open(copyPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    /* Read */
                    do
                    {
                        while (reader.Read())
                        {

                        }
                    }
                    while (reader.NextResult());
                    /* Result Execute */
                    var result = reader.AsDataSet(); 
                    for (int h = 0; h < result.Tables.Count; h++)
                    {
                        string tbName = result.Tables[h].TableName;
                        if (tbName.Contains("-x"))
                        {
                            tbName = tbName.Replace("-x", null);
                            continue;
                        }
                        ReadTableDatas rtd = new ReadTableDatas();
                        // Set SheetName
                        rtd.SheetName = tbName;
                        rtd.ColumsCount = result.Tables[h].Columns.Count;
                        rtd.NameSpace = "GameTable." + fi.Name.Replace(".xlsx", null);
                        rtd.ResourcesPath = "TableDatas/";
                        var t= result.Tables[h];
                        for (int i = 0; i < t.Rows.Count; i++)
                        {
                            for (int j = 0; j < t.Columns.Count; j++)
                            {
                                var data = t.Rows[i][j].ToString();
                                if (!string.IsNullOrEmpty(data))
                                {
                                    if (i == 0)
                                    {
                                        rtd.DataNames.Add(data);
                                    }
                                    else if (i == 1)
                                    {
                                        rtd.DataTypes.Add(data);
                                    }
                                    else
                                    {
                                        rtd.Datas.Add(data);
                                    }
                                }
                            }
                        }
                        if (rtd.IsValid())
                        {
                            var cs = rtd.ToCSharp();
                            var js = rtd.ToJson();
                            File.WriteAllText(cs_output_path + "/" + rtd.NameSpace + "." + rtd.SheetName + ".cs", cs);
                            File.WriteAllText(json_output_path + "/" + rtd.NameSpace + "." + rtd.SheetName + ".txt", js);
                        }

                    }
                }

            }
            System.IO.File.Delete(copyPath);
        }
        static void Main(string[] args)
        {

            if (args.Length > 2)
            {
                var input = args[0];
                var cs_output = args[1];
                var json_output = args[2]; 
                System.IO.DirectoryInfo input_di = new DirectoryInfo(input);
                System.IO.DirectoryInfo cs_di = new DirectoryInfo(cs_output);
                System.IO.DirectoryInfo js_di = new DirectoryInfo(json_output);
                foreach (var data in input_di.GetFiles())
                {

                    if (data.Extension.Contains("xlsx") == true)
                    {
                        if (data.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Generate  =>" + data.Name);
                        var path = data.FullName;
                        Convert(path, cs_di.FullName, js_di.FullName);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Complete  =>" + data.Name);
                    }
                }
            }
            else
            {
                Console.WriteLine("인수를 입력해주세요.");
                Console.WriteLine("TB.exe [xlsx경로] [cs출력경로] [json출력경로]");
                foreach (var data in args)
                {
                    Console.WriteLine(data);
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
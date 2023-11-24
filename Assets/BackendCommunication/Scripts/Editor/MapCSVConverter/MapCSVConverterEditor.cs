using System.IO;
using UnityEngine;
using UnityEditor;
using SimpleJSON;
using Backend;
public enum LaneTypes
{
    Grass = 1,
    Dirt = 2,
    Train = 3,
    River = 4,
    RiverLog = 5,
    RiverGator = 6
}

public class MapCSVConverterEditor : EditorWindow
{
    private string fileLocation = "Pick tsv";
    private string fileContent = "";
    private string fileName = "";
    private LaneTypes laneType;

    [MenuItem("Backend/Map TSV Converter")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MapCSVConverterEditor window = (MapCSVConverterEditor)EditorWindow.GetWindow(typeof(MapCSVConverterEditor));
        window.Show();
    }

    void OnGUI()
    {
        Rect r = EditorGUILayout.BeginHorizontal("Source");
        EditorGUILayout.TextField("File:", fileLocation);
        if (GUILayout.Button("Browse"))
        {
            OpenFileBrowser();
        }
        EditorGUILayout.EndHorizontal();

        if (fileContent.Length != 0)
        {
            laneType = (LaneTypes)EditorGUILayout.EnumPopup("Target lane type:", laneType);
            if (GUILayout.Button("Convert"))
            {
                ConvertAndSave();
            }
        }
    }

    void OpenFileBrowser()
    {
        string path = EditorUtility.OpenFilePanel("Choose tsv file to convert to json", "", "tsv");
        fileLocation = path;
        fileName = Path.GetFileNameWithoutExtension(path);
        if (path.Length != 0)
        {
            fileLocation = path;
            fileContent = File.ReadAllText(path);
        }
    }

    async void ConvertAndSave()
    {
        string[] lines = fileContent.Split('\n');
        JSONObject fileJson = new JSONObject();
        for(int i = 1; i < lines.Length; i++){
            string[] chunks = lines[i].Split('\t');

            JSONObject json = new JSONObject();
            string patternId = chunks[0];
            json["pattern_id"] = patternId;
            int laneCount = int.Parse(chunks[1]);
            json["lane_count"] = laneCount;

            int difficulty = int.Parse(chunks[2]);
            json["difficulty"] = difficulty;

            JSONNode entryData = JSON.Parse(chunks[3]);
            json["entry"] = entryData;

            JSONNode exitData = JSON.Parse(chunks[4]);
            json["exit"] = exitData;

            JSONNode layoutData = JSON.Parse(chunks[5]);
            JSONArray layouts = new JSONArray();
            switch(laneType){
                case LaneTypes.Grass:
                JSONArray treeLocs = layoutData["tree"].AsArray;
                JSONArray types = layoutData["type"].AsArray;
                for(int j = (laneCount - 1); j >= 0; j--){
                    JSONObject laneInfo = new JSONObject();
                    laneInfo["type"] = types[j].AsInt;
                    laneInfo["TreePosition"] = treeLocs[j];

                    layouts.Add(laneInfo);
                }
                break;
                case LaneTypes.Dirt:
                JSONArray direction = layoutData["direction"].AsArray;
                JSONArray animLocs = layoutData["animal"].AsArray;
                JSONArray trainSize = layoutData["trainSize"].AsArray;
                JSONArray trainDelay = layoutData["trainDelay"].AsArray;
                types = layoutData["type"].AsArray;

                for(int j = (laneCount - 1); j >= 0; j--){
                    JSONObject laneInfo = new JSONObject();
                    laneInfo["type"] = types[j].AsInt;
                    laneInfo["MoveDirection"] = direction[j];

                    if(laneInfo["type"].AsInt == (int)LaneTypes.Dirt){
                        laneInfo["AnimalPosition"] = animLocs[j];
                    } else if (laneInfo["type"].AsInt == (int)LaneTypes.Train){
                        laneInfo["TrainDelay"] = trainDelay[j];
                        laneInfo["TrainSize"] = trainSize[j];
                    }

                    layouts.Add(laneInfo);
                }
                break;
                case LaneTypes.River:
                direction = layoutData["direction"].AsArray;
                JSONArray lotus = layoutData["lotus"].AsArray;
                JSONArray logs = layoutData["log"].AsArray;
                JSONArray crocs = layoutData["croc"].AsArray;
                JSONArray crocsDirect = layoutData["fixCrocDirection"].AsArray;
                types = layoutData["type"].AsArray;
                for(int j = (laneCount - 1); j >= 0; j--){
                    JSONObject laneInfo = new JSONObject();
                    laneInfo["type"] = types[j].AsInt;
                    laneInfo["MoveDirection"] = direction[j];

                    if(laneInfo["type"].AsInt == (int)LaneTypes.River){
                        laneInfo["LotusPosition"] = lotus[j];
                    } else if (laneInfo["type"].AsInt == (int)LaneTypes.RiverLog){
                        laneInfo["LogPosition"] = logs[j];
                    } else if (laneInfo["type"].AsInt == (int)LaneTypes.RiverGator){
                        laneInfo["CrocPosition"] = crocs[j];
                        laneInfo["FixCrocDirection"] = crocsDirect[j];
                    }

                    layouts.Add(laneInfo);
                }
                break;
            }
            
            json["lane"] = layouts;

            fileJson[patternId] = json;
        }

        var path = EditorUtility.SaveFilePanel(
            "Save lane template as JSON",
            "",
            fileName+".json",
            "json");

        if (path.Length != 0)
        {
            File.WriteAllText(path, fileJson.ToString());
            Debug.Log($"Output : {fileJson.ToString()}");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#region Data Class
[System.Serializable]
public class UserData
{
    public int characterCount;
    public List<CharacterData> characterList;
}

[System.Serializable]
public class CharacterData
{
    public string _id;
    public string name;
    public string rarity;
    public string species;
    public int maxDistance;
    public int maxCoin;
    public int stress;
    public int health;
    public int maxHealth;
    public int stamina;
    public int maxStamina;
    public string imageBg;
}

[System.Serializable]
public class LoginData
{
    public string username;
    public string password;

    public LoginData(string mUsername, string mPassword)
    {
        username = mUsername;
        password = mPassword;
    }
}

public class API
{
    protected static API _instance;
    protected static bool _canInit = false;
    protected static API getInstance()
    {
        if (_instance == null)
        {
            _canInit = true;
            _instance = new API();
            _canInit = false;
        }
        return _instance;
    }

    protected string _token;
    public static string token
    {
        set { getInstance()._token = value; }
        get { return getInstance()._token; }
    }

    protected bool _isAlphaTest;
    public static bool isAlphaTest
    {
        set { getInstance()._isAlphaTest = value; }
        get { return getInstance()._isAlphaTest; }
    }
}

#endregion
public class WebglUtility : MonoBehaviour
{
    #region Singleton
    public static WebglUtility instance;
    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
    }
    #endregion

    #region GetCookie

    [DllImport("__Internal")]
    private static extern string GetAllCookies();

    [DllImport("__Internal")]
    private static extern void SetAllCookies(string str);

    [DllImport("__Internal")]
    private static extern void BackToHomePage();

    public string GetTokenFormCookie()
    {
        string cookieString = GetAllCookies();
        string[] cokkies = cookieString.Split(';');
        for (int i = 0; i < cokkies.Length; i++)
        {
            string cookieTemp = cokkies[i].Replace(" ", "");
            if (cookieTemp.StartsWith("jungle-cookie"))
            {
                string[] tokens = cokkies[i].Split('=');
                API.token = tokens[1];
            }
        }
        return API.token;
    }

    public static void BackToHome()
    {
        BackToHomePage();
    }
    #endregion

    #region Rest API

    private const string URL = "https://stagingxyz-api.jungleroad.io";
    private const string getuserdatapath = "/characters"; //header auth token
    private const string postMobileLogin = "/auth/login-username"; // raw json body

    public UserData userData;

    public void GetPlayerData(Action<UserData> callback,bool isAlphaTest)
    {
        if (isAlphaTest)
            GetPlayerDataAlpha(callback);
        else
            StartCoroutine(_GetPlayerData(callback));
    }

    public void GetPlayerDataAlpha(Action<UserData> callback)
    {
        UserData temp = new UserData();
        temp.characterCount = 5;
        temp.characterList = new List<CharacterData>();
        for (int i = 0; i < 5; i++)
        {
            CharacterData tempdata = new CharacterData();
            tempdata._id = "#00"+ (i+1);
            tempdata.rarity = "Common";
            tempdata.species = CharacterManager.instance.species[i];
            tempdata.maxDistance = 0;
            tempdata.maxCoin = 0;
            tempdata.stress = 0;
            tempdata.health = 100;
            tempdata.maxHealth = 100;
            tempdata.stamina = 100;
            tempdata.maxStamina = 100;
            tempdata.imageBg = "";
            tempdata.name = tempdata.species + " " + tempdata.rarity + " " + tempdata._id.Replace("#", "");
            temp.characterList.Add(tempdata);
        }
        callback?.Invoke(temp);
    }

    private IEnumerator _GetPlayerData(Action<UserData> callback)
    {
        string url = URL + getuserdatapath;
        UnityWebRequest www = UnityWebRequest.Get(url);
        //Debug.Log("use token for Auth : " + token);
        www.SetRequestHeader("Authorization", "Bearer " + API.token);
        using (www)
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(null);
            }
            else
            {
                //tokenText.text = "Get Player Data Done!";
                string t = www.downloadHandler.text;
                // wait for implement
                JObject o = JObject.Parse(t);
                userData = new UserData();
                userData.characterList = new List<CharacterData>();
                userData.characterCount = (int)o["total"];
                JArray cList = o["list"] as JArray;
                for (int i = 0; i < cList.Count; i++)
                {
                    CharacterData temp = new CharacterData();
                    temp._id = (string)cList[i]["_id"];
                    if (cList[i]["maxDistance"] != null)
                        temp.maxDistance = (int)cList[i]["maxDistance"];
                    if (cList[i]["maxCoin"] != null)
                        temp.maxCoin = (int)cList[i]["maxCoin"];
                    temp.stress = (int)cList[i]["stress"];
                    temp.health = (int)cList[i]["health"];
                    temp.maxHealth = (int)cList[i]["characterType"]["maxHealth"];
                    temp.stamina = (int)cList[i]["stamina"];
                    temp.maxStamina = (int)cList[i]["characterType"]["maxStamina"];
                    temp.name = (string)cList[i]["characterType"]["name"];
                    temp.rarity = (string)cList[i]["characterType"]["rarity"];
                    temp.species = (string)cList[i]["characterType"]["species"];

                    userData.characterList.Add(temp);
                }
                callback?.Invoke(userData);
            }
        }
    }

    #region Login Mobile
    public void LogInWithUserName(string username, string password, Action<string> callback)
    {
        StartCoroutine(_LogInWithUserName(username, password, callback));
    }

    private IEnumerator _LogInWithUserName(string username, string password, Action<string> callback)
    {
        string url = URL + postMobileLogin;
        LoginData data = new LoginData(username, password);
        string jsonString = JsonUtility.ToJson(data);
        //Debug.Log(jsonString);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        UnityWebRequest www = new UnityWebRequest(url);
        www.method = "POST";
        if (bytes.Length > 0)
        {
            www.uploadHandler = new UploadHandlerRaw(bytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.uploadHandler.contentType = "application/json";
            www.SetRequestHeader("Content-Type", "application/json");
        }

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                jsonString = www.downloadHandler.text;
                JObject o = JObject.Parse(jsonString);
                callback?.Invoke((string)o["message"]);
            }
            else
            {
                jsonString = www.downloadHandler.text;
                JObject o = JObject.Parse(jsonString);
                API.token = (string)o["accessToken"];
                callback?.Invoke(API.token);
            }
        }
    }
    #endregion

    #endregion

   #region For Test
//    [TextArea(3, 5)]
//    public string testTokenCokkie;

//    public TMP_Text tokenText;
//    public TMP_InputField user, pass;

//    void Start()
//    {
//#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
//        //SetAllCookies("_ga=GA1.1.1723527343.1649145820");
//        //SetAllCookies("_ga_P2KTS5EMLP=GS1.1.1649242147.3.1.1649242156.0");
//        //SetAllCookies("jungle-cookie=" + testTokenCokkie);
//        //Debug.Log("jungle-cookie=" + testTokenCokkie);
//#endif
//    }

//    public void TestLoginWithUsername()// test login with username and password in input field
//    {
//        LogInWithUserName(user.text, pass.text, null);
//    }

//    public void TestGetTokenBtnClick() 
//    {
//        tokenText.text = GetTokenFormCookie();
//    }
    #endregion
}

//"jungle-cookie=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2MjFlMWJhMjMyYWE3ZWMyYWY2YzE4MDYiLCIkX18iOnsiYWN0aXZlUGF0aHMiOnsicGF0aHMiOnsiYWRkcmVzcyI6ImluaXQiLCJsYXN0V2l0aGRyYXciOiJpbml0IiwiX2lkIjoiaW5pdCIsImNyZWF0ZWRBdCI6ImluaXQiLCJ1cGRhdGVkQXQiOiJpbml0IiwiX192IjoiaW5pdCIsInVzZXJuYW1lIjoiaW5pdCIsImxhc3RSZXN0IjoiaW5pdCIsImJhY2t1cExhc3RXaXRoZHJhdyI6ImluaXQifSwic3RhdGVzIjp7Imlnbm9yZSI6e30sImRlZmF1bHQiOnt9LCJpbml0Ijp7Il9pZCI6dHJ1ZSwiYWRkcmVzcyI6dHJ1ZSwiY3JlYXRlZEF0Ijp0cnVlLCJ1cGRhdGVkQXQiOnRydWUsIl9fdiI6dHJ1ZSwibGFzdFdpdGhkcmF3Ijp0cnVlLCJ1c2VybmFtZSI6dHJ1ZSwibGFzdFJlc3QiOnRydWUsImJhY2t1cExhc3RXaXRoZHJhdyI6dHJ1ZX0sIm1vZGlmeSI6e30sInJlcXVpcmUiOnt9fSwic3RhdGVOYW1lcyI6WyJyZXF1aXJlIiwibW9kaWZ5IiwiaW5pdCIsImRlZmF1bHQiLCJpZ25vcmUiXX0sImVtaXR0ZXIiOnsiX2V2ZW50cyI6e30sIl9ldmVudHNDb3VudCI6MCwiX21heExpc3RlbmVycyI6MH0sInN0cmljdE1vZGUiOnRydWUsInNlbGVjdGVkIjp7InBhc3N3b3JkIjowfSwiX2lkIjoiNjIxZTFiYTIzMmFhN2VjMmFmNmMxODA2In0sIiRpc05ldyI6ZmFsc2UsIl9kb2MiOnsiX2lkIjoiNjIxZTFiYTIzMmFhN2VjMmFmNmMxODA2IiwiYWRkcmVzcyI6IjB4MDFkYmFkZDFkYjI5YmEzYTU4MmNjZmEzOTRlZDk0YmJiODJkY2Y5YiIsImNyZWF0ZWRBdCI6IjIwMjItMDMtMDFUMTM6MTI6MDIuODYzWiIsInVwZGF0ZWRBdCI6IjIwMjItMDQtMjNUMDk6NDc6MjIuODQ3WiIsIl9fdiI6MCwibGFzdFdpdGhkcmF3IjoiMjAyMi0wNC0yM1QwOTo0NzoyMi44NDZaIiwidXNlcm5hbWUiOiJidWNrYnVjayIsImxhc3RSZXN0QXQiOiIyMDIyLTAzLTI0VDExOjI3OjE5LjIwMloiLCJsYXN0UmVzdCI6IjIwMjItMDQtMjBUMTA6MjI6MjMuNDIzWiIsImJhY2t1cExhc3RXaXRoZHJhdyI6IjIwMjItMDQtMjBUMTY6MDU6MzkuOTk2WiJ9LCIkaW5pdCI6dHJ1ZSwiaXNNb2JpbGUiOnRydWUsImlhdCI6MTY1MDcyMjQ3OCwiZXhwIjoxNjUwNzI2MDc4fQ.0zP-RBibWjQ8BpEuy5BMugSucW4EOo7KCQR1ZsqAN8I"
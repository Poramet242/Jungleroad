using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    [Header("Alpha Test")]
    public bool isAlphaTest = false;
    [Space(5)]
    public GameObject characterPrefab;
    public Texture[] texture;
    public RawImage rawImage;
    WebglUtility webglUtility;
    [SerializeField] RectTransform content;
    [SerializeField]
    AudioSource audioSource;
    // Login
    public TMP_InputField user, pass;

    public string[] species;
    public string[] rarity;
    int s;
    int r;
    public Text health_t, stamina_t, stress_t;
    public List<GameObject> characterList;
    private string selectedCharacterID;
    public Slider h_s, sta_s, str_s;
    public Button startGameBtn;

    [HideInInspector] public MainMenuUiManager mmum;

    private string ErrorRoomRoomStr;
    private void Start()
    {
        API.isAlphaTest = isAlphaTest;
        webglUtility = WebglUtility.instance;
        mmum = GetComponent<MainMenuUiManager>();
        if(isAlphaTest)
        {
            WebGlLogin();
            return;
        }
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        WebGlLogin();
#endif

    }
    public void Login()
    {
        if(isAlphaTest)
        {
            mmum.ShowCharacterListPanal();
            webglUtility.GetPlayerData((UserData) =>
            {
                GetCharacter(UserData);
            }, isAlphaTest);
            return;
        }
       webglUtility.LogInWithUserName(user.text, pass.text, (result) => 
       {
           if(result == "User not found" || result == "Invalid password")
           {
               mmum.ShowErrorPopUp(result);
               return;
           }
           else
           {
               mmum.ShowCharacterListPanal();
               webglUtility.GetPlayerData((UserData) => 
               {
                   GetCharacter(UserData); 
               }, isAlphaTest);
           }
       });
    }
    void GetCharacter(UserData userData)
    {
        for (int i = 0; i < userData.characterList.Count; i++)
        {
            GameObject characterPrefabSpawn = Instantiate(characterPrefab);
            characterPrefabSpawn.transform.SetParent(content);
            characterPrefabSpawn.transform.localScale = new Vector3(1, 1, 1);

            CharacterPrefabScript charScript = characterPrefabSpawn.GetComponent<CharacterPrefabScript>();

            charScript._id = userData.characterList[i]._id;
            charScript._name = userData.characterList[i].name;
            charScript.rarity = userData.characterList[i].rarity;
            charScript.species = userData.characterList[i].species;
            charScript.maxDistance = userData.characterList[i].maxDistance;
            charScript.maxCoin = userData.characterList[i].maxCoin;
            charScript.stress = userData.characterList[i].stress;
            charScript.health = userData.characterList[i].health;
            charScript.maxHealth = userData.characterList[i].maxHealth;
            charScript.stamina = userData.characterList[i].stamina;
            charScript.maxStamina = userData.characterList[i].maxStamina;

            for (int j = 0; j < species.Length; j++)
            {
                if (charScript.species == species[j])
                    s = j;
            }
            for (int k = 0; k < rarity.Length; k++)
            {
                if (charScript.rarity == rarity[k])
                    r = k;
            }
            charScript.number = s + (5 * r);
            charScript.imageBg.texture = texture[charScript.number]; // Get Image
            charScript.selected_btn.onClick.AddListener(() => SelectingCharacter(charScript._id, charScript.number, charScript.health, charScript.stamina, charScript.stress, charScript.maxHealth, charScript.maxStamina));
            // [android/ios]
            //content.sizeDelta += new Vector2(0,300);
            characterList.Add(characterPrefabSpawn);
        }

        content.sizeDelta = new Vector2(0, Mathf.Ceil((float)characterList.Count / 2) * content.GetComponent<GridLayoutGroup>().cellSize.y);
    }
    void SelectingCharacter(string charID, int charNumber, int h,int sta, int str, int mH, int mSta)
    {
        SoundManager.Instance.PlaySound(SoundManager.Sound.button, audioSource);
        print(charNumber);
        mmum.ShowCharacterDetailPanal();
        DataTransferScene.Instance.selectNumber = charNumber;
        rawImage.texture = texture[charNumber];
        health_t.text = h.ToString();
        stamina_t.text = sta.ToString();
        stress_t.text = str.ToString();
        selectedCharacterID = charID;
        SliderSetup(mH, mSta, h, sta, str);
        startGameBtn.interactable = sta != 0 && h != 0;
    }
    public void RemoveCharacterList()
    {
        foreach (var item in characterList)
        {
            Destroy(item);
            // [android/ios]
            //content.sizeDelta -= new Vector2(0,300);
        }
        content.sizeDelta = Vector2.zero;
        characterList.Clear();
    }
    public void ChangeToPlayScene()
    {
        ConnectToServer.getInstance().StartConnectProcess(
            API.token, 
            selectedCharacterID, 
            (success, errMsg) => 
            {
                if (!success) 
                {
                    //TODO: Ryu/Gun - inform user the error message (errMsg)
                    //Debug.Log(errMsg);
                    mmum.ShowErrorPopUp(errMsg);
                    return;
                }
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            });
    }
    public void ErrorFullGameRoom(int minutes)
    {
        ErrorRoomRoomStr = "There is no available room now, Please wait for next " + "\n" + minutes + " minutes";
        mmum.ShowErrorPopUp(ErrorRoomRoomStr);
    }

    public void WebGlLogin()
    {
        if (string.IsNullOrEmpty(API.token) && !isAlphaTest)
        {
            if (string.IsNullOrEmpty(webglUtility.GetTokenFormCookie()))
            {
                //Debug.LogError("TokenCookie Invalid");
                mmum.ShowErrorPopUp("TokenCookie Invalid");
                return;
            }
        }
        mmum.ShowCharacterListPanal();
        webglUtility.GetPlayerData((UserData) => { GetCharacter(UserData); }, isAlphaTest);
    }

    void SliderSetup(int mH, int mSta, int h, int sta, int str)
    {
        h_s.maxValue = mH;
        sta_s.maxValue = mSta;
        str_s.maxValue = 100;
        h_s.value = h;
        sta_s.value = sta;
        str_s.value = str;
    }
}

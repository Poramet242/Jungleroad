using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MainMenuUiManager : MonoBehaviour
{
    public int CurrentpageIndex = 0;// 0 = login 1 = characterList 2 = characterDetail 3 = error
    public GameObject loginPanal;
    public GameObject characterListPanal;
    public GameObject characterDetailPanal;
    public GameObject erroePanal;
    public TMP_Text ErrorMessageText;

    private CharacterManager cm;
    [SerializeField]
    AudioSource audioSource;

    public void ShowLoginPanal()
    {
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        // to do call javalib to refresh WebPage
        WebglUtility.BackToHome();
#else
        if (cm == null)
            cm = GetComponent<CharacterManager>();

        if (CurrentpageIndex == 0 || cm.isAlphaTest)
            return;
        loginPanal.SetActive(true);
        characterListPanal.SetActive(false);
        characterDetailPanal.SetActive(false);
        erroePanal.SetActive(false);
        cm.RemoveCharacterList();
        CurrentpageIndex = 0;
#endif
    }

    public void ShowCharacterListPanal()
    {
        if (CurrentpageIndex == 1)
            return;
        loginPanal.SetActive(false);
        characterListPanal.SetActive(true);
        characterDetailPanal.SetActive(false);
        erroePanal.SetActive(false);
        CurrentpageIndex = 1;
    }

    public void ShowCharacterDetailPanal()
    {
        if (CurrentpageIndex == 2)
            return;
        loginPanal.SetActive(false);
        characterListPanal.SetActive(false);
        characterDetailPanal.SetActive(true);
        erroePanal.SetActive(false);
        CurrentpageIndex = 2;
    }

    public void ShowErrorPopUp(string message)
    {
        if (CurrentpageIndex == 3)
            return;
        ErrorMessageText.text = message;
        erroePanal.SetActive(true);
        CurrentpageIndex = 3;
    }
    public void onClickButtonEff()
    {
        SoundManager.Instance.PlaySound(SoundManager.Sound.button, audioSource);
    }
    public void ErrorBackButtonClick()
    {
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        // to do call javalib to refresh WebPage
        WebglUtility.BackToHome();
#else
        if (!API.isAlphaTest)
        {
            ShowLoginPanal();
        }
        else if (API.isAlphaTest)
        {
            ShowCharacterListPanal();
        }
#endif
    }
}

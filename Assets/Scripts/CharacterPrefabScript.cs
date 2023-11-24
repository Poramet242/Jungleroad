using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPrefabScript : MonoBehaviour
{
    // SetEnum int
    public string _id;
    public string _name;
    public string rarity;
    public string species;
    public int maxDistance;
    public int maxCoin;
    public int stress;
    public int health;
    public int maxHealth;
    public int stamina;
    public int maxStamina;
    public RawImage imageBg;
    public Button selected_btn;
    public int number;

    public Text stress_t, health_t, stamina_t;
    private void Start()
    {
        stress_t.text = stress.ToString();
        health_t.text = health.ToString();
        stamina_t.text = stamina.ToString();
    }
}

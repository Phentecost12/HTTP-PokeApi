using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HTTP_Request : MonoBehaviour
{

    private const string URL = "https://pokeapi.co/api/v2/pokemon/";
    [SerializeField] private TMP_InputField pokemon;
    [SerializeField] private Pokemon_UI Pokemon_ui;

    public void Get_Info() 
    {
        string pokemonName = pokemon.text;
        StartCoroutine(HTTP_GET(URL + pokemonName));
    }

    IEnumerator HTTP_GET(string url) 
    {
        UnityWebRequest pokemonInfoRequest = UnityWebRequest.Get(url);

        yield return pokemonInfoRequest.SendWebRequest();

        if(pokemonInfoRequest.result == UnityWebRequest.Result.ConnectionError) 
        {
            Debug.LogError(pokemonInfoRequest.error);
            yield break;
        }

        JSONNode pokeinfo = JSON.Parse(pokemonInfoRequest.downloadHandler.text);
        string name = pokeinfo["name"];
        string imgURL = pokeinfo["sprites"]["front_default"];

        UnityWebRequest pokemonSpriteRequest = UnityWebRequestTexture.GetTexture(imgURL);

        yield return pokemonSpriteRequest.SendWebRequest();

        if (pokemonSpriteRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(pokemonSpriteRequest.error);
            yield break;
        }

        Texture2D img = DownloadHandlerTexture.GetContent(pokemonSpriteRequest);

        JSONNode pokeTypes = pokeinfo["types"];
        string [] types = new string[pokeTypes.Count];

        for(int i = 0, j = pokeTypes.Count-1; i < pokeTypes.Count; i++ , j--) 
        {
            types[j] = pokeTypes[i]["type"]["name"];
        }

        JSONNode pokeStats = pokeinfo["stats"];
        Dictionary<string, int> stats = new Dictionary<string, int>();

        for(int i = 0; i < pokeStats.Count; i++) 
        {
            stats.Add(pokeStats[i]["stat"]["name"], pokeStats[i]["base_stat"]);
        }

        Pokemon_ui.Fill_Information(name, types, stats, img);
    }
}

[Serializable]
public class Pokemon_UI 
{
    public TextMeshProUGUI name, types;
    public TextMeshProUGUI[] stats;
    public TextMeshProUGUI[] stats_INT;
    public Slider[] stats_Sliders; 
    public RawImage mainImage;
    public GameObject panel;

    public void Fill_Information(string name, string[] type, Dictionary<string,int> stats, Texture2D img) 
    {
        this.name.text = name;
        types.text = "";
        foreach(string txt  in type) 
        {
            types.text += txt + ", ";
        }

        string[] statsKeys = stats.Keys.ToArray();

        for(int i = 0;i < statsKeys.Length;i++) 
        {
            this.stats[i].text = statsKeys[i];
            stats_INT[i].text = stats[statsKeys[i]].ToString();
            stats_Sliders[i].value = stats[statsKeys[i]];
        }

        mainImage.texture = img;

        if(!panel.activeInHierarchy) panel.SetActive(true);
    }
}

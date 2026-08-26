using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

[Serializable]
public class SkinData
{
    public string id;

    public string name;

    public int price;

    public string img;

    public int rarity;


    public static SkinData fromDictionary(string key, IDictionary<string, object> data)
    {
        var skin = new SkinData();
        skin.id = key;
        skin.name = data.ContainsKey("name") ? data["name"].ToString() : "";
        skin.img = data.ContainsKey("img") ? data["img"].ToString() : "";
        

        if (data.ContainsKey("price"))
        {
            int.TryParse(data["price"].ToString(), out skin.price);
        }
        if (data.ContainsKey("rarity"))
        {
            int.TryParse(data["rarity"].ToString(), out skin.rarity);
        }

        return skin;
    }

}

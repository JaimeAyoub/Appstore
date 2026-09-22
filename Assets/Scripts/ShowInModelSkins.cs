using System;
using System.Collections.Generic;
using UnityEngine;

public class ShowInModelSkins : MonoBehaviour
{
    public static ShowInModelSkins instance;
    [SerializeField] private GameObject headModel;
    [SerializeField] private GameObject torsoModel;

    private readonly List<GameObject> spawnedCosmetics = new List<GameObject>();

    private SkinData skin;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void RestoreCosmetics()
    {
        if (StoreManager.Instance == null) return;

        ClearCosmetics();


        foreach (var skin in StoreManager.Instance.GetAllSkins())
        {
            if (StoreManager.Instance.IsPurchased(skin.id))
            {
                enableCosmetic(skin);
            }
        }
    }

    private void ClearCosmetics()
    {
        foreach (var obj in spawnedCosmetics)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedCosmetics.Clear();
    }
    

    public void enableCosmetic(SkinData skin)
    {
        GameObject instance = null;

        switch (skin.type)
        {
            case "head":
                var cosmeticHead = Resources.Load<GameObject>("Cosmetics/" + skin.name);
                if (cosmeticHead != null)
                    instance = Instantiate(cosmeticHead, headModel.transform.position, headModel.transform.rotation,
                        headModel.transform);
                break;
            case "torso":
                var cosmeticTorso = Resources.Load<GameObject>("Cosmetics/" + skin.name);
                if (cosmeticTorso != null)
                    instance = Instantiate(cosmeticTorso, torsoModel.transform.position, torsoModel.transform.rotation,
                        torsoModel.transform);
                break;
            default:
                break;
        }

        if (instance != null)
        {
            spawnedCosmetics.Add(instance);
        }

        Debug.Log("Agregar cosmeticos");
    }
}


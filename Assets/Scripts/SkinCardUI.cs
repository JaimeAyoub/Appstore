using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkinCardUI : MonoBehaviour
{
    [SerializeField] private Image skinImage;
    [SerializeField] private Image rarity;
    [SerializeField] private TMP_Text skinName;
    [SerializeField] private TMP_Text price;

    public void SetUp(SkinData skinData)
    {
        skinName.text = skinData.name;
        price.text = skinData.price.ToString();
        
        var sprite = Resources.Load<Sprite>("Skins/" +  skinData.img);

        if(sprite != null)
            skinImage.sprite = sprite;
        else
            Debug.LogError("No se encontro la imagen: " +  skinData.img);
    }
}

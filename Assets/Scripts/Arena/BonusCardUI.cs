using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BonusCardUI : MonoBehaviour
{
    public Image           cardBG;
    public Image           iconImage;
    public TextMeshProUGUI rarityLabel;
    public TextMeshProUGUI bonusName;
    public TextMeshProUGUI description;

    public BonusData Data { get; private set; }

    public void Setup(BonusData data)
    {
        Data = data;
        var col = BonusData.GetColor(data.rarity);

        if (cardBG      != null) cardBG.color  = new Color(col.r * 0.12f, col.g * 0.12f, col.b * 0.12f, 0.97f);
        if (iconImage   != null) iconImage.color = new Color(col.r * 0.4f, col.g * 0.4f, col.b * 0.4f, 0.9f);
        if (rarityLabel != null) { rarityLabel.text = BonusData.GetRarityLabel(data.rarity); rarityLabel.color = col; }
        if (bonusName   != null) { bonusName.text = data.name; bonusName.color = Color.white; }
        if (description != null) { description.text = data.description; description.color = new Color(0.78f, 0.72f, 0.88f); }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardThumbView : MonoBehaviour, IPointerClickHandler
{
    public Image artImage;
    //public TMP_Text nameText;
    //public Text manaText;

    public CardDataDefinition Data { get; private set; }

    public void Init(CardDataDefinition data)
    {
        Data = data;
        //nameText.text = data.displayName;
        //manaText.text = data.manaCost.ToString();

        if (artImage != null)
            artImage.sprite = CardDatabase.GetArt(data.artName);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键单击: 添加卡牌到卡组
            DeckManager.Instance.AddCard(Data.cardID);
        }
        // 拖拽功能留给 DeckCardView
    }
}

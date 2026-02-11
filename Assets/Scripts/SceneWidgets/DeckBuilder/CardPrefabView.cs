using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPrefabView : MonoBehaviour, IPointerClickHandler
{
    public Image artImage;
    //public TMP_Text nameText;
    //public Text manaText;

    public CardBaseInfo cardBaseInfo { get; private set; }

    public void Init_CardPrefab(CardBaseInfo data)
    {
        cardBaseInfo = data;

        if (artImage != null)
            artImage.sprite = CardInstanceHelper.GetArt(data.artName);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键单击: 添加卡牌到卡组
            if (DeckManager.Instance != null)
                DeckManager.Instance.AddCard2Deck(cardBaseInfo.cardID);
        }
        // 拖拽功能留给 DeckCardView
    }
}

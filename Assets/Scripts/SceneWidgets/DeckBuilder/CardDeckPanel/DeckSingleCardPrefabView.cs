// DeckSingleCardView.cs
// 子panel，卡组里每个卡的具体实例
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DeckSingleCardPrefabView : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("卡栏显示内容")]
    public TMP_Text nameText;
    public TMP_Text countText;

    private RectTransform rectTransform;
    private Canvas canvas;
    private LayoutElement layoutElement;
    private CanvasGroup canvasGroup;
    public string CardID { get; private set; }
    private DeckPanelParent deckPanelParent;
    public bool IsDraggable { get; private set; } = true;

    public static System.Action OnItemDraggedAction;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void DeckSingleCardView_Init(string cardID, int count, DeckPanelParent panel, bool isDraggable)
    {
        CardID = cardID;
        deckPanelParent = panel;
        IsDraggable = isDraggable; // 设置状态

        CardBaseInfo data = CardInstanceHelper.GetCardBaseData(cardID);

        if (data != null)
        {
            nameText.text = data.displayName;
            countText.text = $"x{count}";

            // 可视化区分：不可拖拽的卡牌稍微变暗，或者加个锁图标
            if (nameText != null)
                nameText.color = IsDraggable ? Color.white : Color.yellow;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 允许右键移除任何卡牌（由 DeckManager 逻辑决定是否允许，这里只是UI触发）
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            CardGuidePanel.Instance.Scroll2SelectCard(CardID);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DeckManager.Instance.RemoveCard4Deck(CardID);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return; // <--- 禁止拖拽检查

        if (layoutElement != null) layoutElement.ignoreLayout = true;
        canvasGroup.blocksRaycasts = false;         // 允许射线穿透以便检测下方的 dropPlaceholder
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rectTransform.position = eventData.position;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                deckPanelParent.contentRect_DeckPanelParent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            rectTransform.localPosition = localPoint;
        }

        deckPanelParent.HandleDragMove(this.rectTransform);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;

        if (layoutElement != null) layoutElement.ignoreLayout = false;
        canvasGroup.blocksRaycasts = true;
        deckPanelParent.HandleDragEnd(this);
        OnItemDraggedAction?.Invoke();
    }
}
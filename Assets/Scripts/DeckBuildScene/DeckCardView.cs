// DeckCardView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DeckCardView : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public TMP_Text nameText;
    public TMP_Text countText;
    public Image background; // 可选：用来改变颜色显示是否可拖拽

    public string CardID { get; private set; }
    private DeckPanel deckPanel;
    public bool IsDraggable { get; private set; } = true; // <--- 新增控制字段

    private RectTransform rectTransform;
    private Canvas canvas;
    private LayoutElement layoutElement;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // 修改 Init 方法，接收 isDraggable 参数
    public void Init(string cardID, int count, DeckPanel panel, bool isDraggable)
    {
        CardID = cardID;
        deckPanel = panel;
        IsDraggable = isDraggable; // 设置状态

        CardData data = CardDatabase.GetCardData(cardID);

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
            AllCardsPanel.Instance.ScrollToCard(CardID);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DeckManager.Instance.RemoveCard(CardID);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return; // <--- 禁止拖拽检查

        if (layoutElement != null) layoutElement.ignoreLayout = true;
        canvasGroup.blocksRaycasts = false; // 允许射线穿透以便检测下方的 dropPlaceholder
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
                deckPanel.contentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            rectTransform.localPosition = localPoint;
        }

        deckPanel.HandleDragMove(this.rectTransform);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;

        if (layoutElement != null) layoutElement.ignoreLayout = false;
        canvasGroup.blocksRaycasts = true;

        deckPanel.HandleDragEnd(this);
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleCardView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    public Image artImage;
    public Image backImage;

    // [修改] 持有运行时数据对象
    public RuntimeCard RuntimeData { get; private set; }

    public bool IsFaceUp { get; private set; }
    public bool IsSelectedForMulligan { get; private set; } = false;
    private Vector3 originalLocalPos; // 记录原始位置用于复位

    private const string CARD_BACK_PATH = "Cards/Arts/000back";

    // [修改] Init 接收 RuntimeCard
    public void Init(RuntimeCard runtimeCard, bool isFaceUp)
    {
        RuntimeData = runtimeCard; // 存储实例数据
        IsFaceUp = isFaceUp;
        IsSelectedForMulligan = false; // 重置状态

        // 使用静态ID加载资源
        CardData data = CardDatabase.GetCardData(runtimeCard.CardDataID);
        if (data != null)
        {
            if (artImage != null)
                artImage.sprite = CardDatabase.GetArt(data.artName);

            // 调试用：为了方便看清是哪张卡，可以改一下 GameObject 名字
            gameObject.name = $"{data.displayName}_{runtimeCard.UniqueID}";
        }

        if (backImage != null)
        {
            Sprite backSprite = Resources.Load<Sprite>(CARD_BACK_PATH);
            if (backSprite != null) backImage.sprite = backSprite;
        }

        UpdateVisuals();
    }

    public void SetFaceUp(bool faceUp)
    {
        IsFaceUp = faceUp;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (backImage != null)
        {
            backImage.gameObject.SetActive(!IsFaceUp);
        }
        // 视觉反馈：如果被选中，稍微变暗或者加框，这里演示简单的颜色变化
        if (IsFaceUp && artImage != null)
        {
            // 简单示例：选中变灰，没选中白色。
            // 更好的做法是位置上移 (transform.localPosition += Vector3.up * 20)，
            // 但因为用了 LayoutGroup，直接改位置可能会被重置，建议改 scale 或 color
            artImage.color = IsSelectedForMulligan ? new Color(0.6f, 1f, 0.6f) : Color.white;
        }
    }

    // [核心] 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        // 只有在调度阶段，且是正面（自己的手牌）时，才允许交互
        if (BattleManager.Instance.IsMulliganPhase && IsFaceUp)
        {
            ToggleMulliganSelection();
        }
        else
        {
            // 正常游戏阶段的点击逻辑（比如放大查看详情）
            // BattleUI.Instance.ShowCardDetails(RuntimeData);
        }
    }
    // 切换选中状态
    // 切换选中状态
    public void ToggleMulliganSelection()
    {
        // 情况A：如果当前已经被选中了，那么随时可以“取消选中”
        if (IsSelectedForMulligan)
        {
            IsSelectedForMulligan = false;
            UpdateVisuals();
        }
        // 情况B：如果当前没被选中，想变成选中状态，必须检查数量限制
        else
        {
            // 获取当前已选数量
            int currentCount = 0;
            if (BattleUI.Instance != null)
            {
                currentCount = BattleUI.Instance.GetSelectedMulliganCount();
            }

            // [核心修改] 只有小于 2 张时，才允许选中新的
            if (currentCount < 2)
            {
                IsSelectedForMulligan = true;
                UpdateVisuals();
            }
            else
            {
                Debug.Log("调度限制：最多只能选择 2 张卡牌。");
                // 这里可以加一个简单的视觉反馈，比如让图片抖动一下，或者弹出提示文字
            }
        }
    }
}
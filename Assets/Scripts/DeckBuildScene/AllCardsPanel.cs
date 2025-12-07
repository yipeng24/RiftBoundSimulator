using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // 用于排序和查找
public class AllCardsPanel : MonoBehaviour
{
    public static AllCardsPanel Instance { get; private set; } // 新增单例引用
    public GameObject cardThumbPrefab;   // CardThumbItem prefab
    public ScrollRect scrollRect;
    public RectTransform content;

    private List<CardThumbView> views = new List<CardThumbView>();
    // 新增：用于快速查找卡牌视图
    private Dictionary<string, CardThumbView> cardViewMap = new Dictionary<string, CardThumbView>();


    private void Awake() // 新增 Awake 以实现单例
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        PopulateAllCards();
    }

    void PopulateAllCards()
    {
        Dictionary<string, CardDataDefinition> allCards = CardDatabase.GetAllCardData();
        if (allCards == null)
        {
            Debug.LogError("CardDatabase.Instance is null");
            return;
        }
        int cardIndex = 0;
        foreach (var pair in allCards)
        {
            CardDataDefinition cardData = pair.Value;
            string cardID = pair.Key; // <--- 直接使用 Key 作为卡牌 ID

            var go = Instantiate(cardThumbPrefab, content);
            var view = go.GetComponent<CardThumbView>();
            view.Init(cardData); // 传入 CardData 对象
            views.Add(view);



            // 记录卡牌ID和视图的映射
            if (!cardViewMap.ContainsKey(cardID))
            {
                cardViewMap.Add(cardID, view);
            }
            else
            {
                Debug.LogWarning($"[AllCardsPanel] 发现重复的 CardID: {cardID}");
            }

            cardIndex++;
        }

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f; // 拉到顶部
    }
    // ==================== 新增：卡池滚动定位功能 ====================

    public void ScrollToCard(string cardID)
    {
        if (scrollRect == null || content == null) return;

        if (cardViewMap.TryGetValue(cardID, out var targetView))
        {
            // 1. 找到目标视图在其父级中的索引
            int targetIndex = targetView.transform.GetSiblingIndex();
            int totalItems = views.Count;

            if (totalItems == 0) return;

            // 2. 计算滚动位置： 0 (底部) 到 1 (顶部)
            // 目标越靠前 (index越小)，normalizedPosition 应该越接近 1

            // 假设所有卡牌视图大小相同，且布局是从上到下（垂直列表）
            // 计算目标卡牌在整个 Content 中的相对位置

            // 调整索引，因为 UI 顶部是 1，底部是 0
            float targetPos = 1f - ((float)targetIndex / (totalItems - 1));

            // 钳制滚动位置
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetPos);

            Debug.Log($"Scrolled to {cardID} at index {targetIndex}");
        }
        else
        {
            Debug.LogWarning($"Cannot find CardThumbView for ID: {cardID}");
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // 用于排序和查找
public class CardGuidePanel : MonoBehaviour
{
    public static CardGuidePanel Instance { get; private set; } 
    public GameObject cardPrefab;   
    public ScrollRect scrollRect_CardGuideView;
    public RectTransform content_CardGuideView;

    private List<CardPrefabView> CardPrefabViews = new List<CardPrefabView>();
    private Dictionary<string, CardPrefabView> CardPrefabViewMap = new Dictionary<string, CardPrefabView>();

    private void Awake() 
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
        PopulateCard2CardGuidePanel();
    }
    // 把单张卡添加到图鉴
    private void PopulateCard2CardGuidePanel()
    {
        IReadOnlyDictionary<string, CardBaseInfo> cardDB = CardManager.Instance.CardInstanceHelper.Database;
        if (cardDB == null || cardDB.Count == 0)
        {
            Debug.LogError("未找到原始卡数据库");
            return;
        }
        int cardIndex = 0;
        foreach (var pair in cardDB)
        {
            CardBaseInfo cardData = pair.Value;
            string cardID = pair.Key; // 使用 Key 作为卡牌 ID

            var go = Instantiate(cardPrefab, content_CardGuideView);
            var view = go.GetComponent<CardPrefabView>();
            view.Init_CardPrefab(cardData); 
            CardPrefabViews.Add(view);
            // 记录卡牌ID和视图的映射
            if (!CardPrefabViewMap.ContainsKey(cardID))
            {
                CardPrefabViewMap.Add(cardID, view);
            }
            else
            {
                Debug.LogWarning($"[AllCardsPanel] 发现重复的 CardID: {cardID}");
            }

            cardIndex++;
        }

        if (scrollRect_CardGuideView != null)
            scrollRect_CardGuideView.verticalNormalizedPosition = 1f; // 拉到顶部
    }

    public void Scroll2SelectCard(string cardID)
    {
        if (scrollRect_CardGuideView == null || content_CardGuideView == null) return;

        if (CardPrefabViewMap.TryGetValue(cardID, out var targetView))
        {
            // 1. 找到目标视图在其父级中的索引
            int targetIndex = targetView.transform.GetSiblingIndex();
            int totalItems = CardPrefabViews.Count;

            if (totalItems == 0) return;

            // 2. 计算滚动位置： 0 (底部) 到 1 (顶部)
            // 目标越靠前 (index越小)，normalizedPosition 应该越接近 1

            // 调整索引，因为 UI 顶部是 1，底部是 0
            float targetPos = 1f - ((float)targetIndex / (totalItems - 1));

            // 钳制滚动位置
            scrollRect_CardGuideView.verticalNormalizedPosition = Mathf.Clamp01(targetPos);

            Debug.Log($"Scrolled to {cardID} at index {targetIndex}");
        }
        else
        {
            Debug.LogWarning($"Cannot find CardThumbView for ID: {cardID}");
        }
    }
}

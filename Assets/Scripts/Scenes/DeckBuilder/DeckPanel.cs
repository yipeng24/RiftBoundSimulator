// DeckPanel.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckPanel : MonoBehaviour
{
    public RectTransform contentRect;
    public GameObject deckCardViewPrefab;
    private List<DeckCardView> deckViews = new List<DeckCardView>();
    [SerializeField] private GameObject dropPlaceholder;

    // 定义固定区域的索引边界
    private int lockedItemCount = 0;

    private void Start()
    {
        if (DeckManager.Instance != null)
            DeckManager.Instance.OnDeckUpdated += UpdateDeckUI;

        UpdateDeckUI();

        if (dropPlaceholder == null)
        {
            dropPlaceholder = new GameObject("DropPlaceholder");
            dropPlaceholder.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 30f);
            dropPlaceholder.transform.SetParent(contentRect);
        }
        dropPlaceholder.SetActive(false);
    }

    private void OnDestroy()
    {
        if (DeckManager.Instance != null)
            DeckManager.Instance.OnDeckUpdated -= UpdateDeckUI;
    }

    // DeckPanel.cs

    private void UpdateDeckUI()
    {
        
        foreach (var view in deckViews) Destroy(view.gameObject);
        deckViews.Clear();
        lockedItemCount = 0;

        List<string> rawList = DeckManager.Instance.GetDeckCardIDs().ToList();
        if (rawList.Count == 0) return;

        // --- A. 确定传奇数据 ---
        CardData currentLegend = null;
        string legendID = rawList.FirstOrDefault(id => CardDatabase.GetCardData(id)?.type == CardType.Legend);
        if (legendID != null) currentLegend = CardDatabase.GetCardData(legendID);

        // --- B. 确定展示英雄 ID (核心逻辑复用) ---
        string targetDisplayHeroID = null;
        // 必须在整个列表中找（注意：此时列表已经排序过，展示英雄肯定在前面，但逻辑上我们还是要找符合条件的）
        // 因为 SortDeckFixedOrder 已经把展示英雄排在符文后面了，我们只要找第一个符合Tag的英雄即可
        foreach (var id in rawList)
        {
            var d = CardDatabase.GetCardData(id);
            if (d != null && d.type == CardType.HeroUnit)
            {
                if (currentLegend != null && currentLegend.tags != null && currentLegend.tags.Contains(d.displayName))
                {
                    targetDisplayHeroID = id;
                    break; // 锁死这个ID
                }
            }
        }

        // --- C. 生成 UI ---
        for (int i = 0; i < rawList.Count; i++)
        {
            string currentID = rawList[i];
            int count = 1;
            while (i + 1 < rawList.Count && rawList[i + 1] == currentID)
            {
                count++;
                i++;
            }

            // 传入 targetDisplayHeroID 进行比对
            CreateCardView(currentID, count, targetDisplayHeroID);
        }

        if (dropPlaceholder != null)
            dropPlaceholder.transform.SetAsLastSibling();
    }

    // 修改方法签名，接收 targetDisplayHeroID
    private void CreateCardView(string cardID, int count, string targetDisplayHeroID)
    {
        var go = Instantiate(deckCardViewPrefab, contentRect);
        var view = go.GetComponent<DeckCardView>();
        CardData data = CardDatabase.GetCardData(cardID);

        bool isDraggable = true;

        // 1. 绝对锁定类型
        if (data.type == CardType.Legend ||
            data.type == CardType.Rune ||
            data.type == CardType.Battlefield)
        {
            isDraggable = false;
        }
        // 2. 英雄单位判定
        else if (data.type == CardType.HeroUnit)
        {
            // 只有 ID 完全等于我们认定的那个“展示英雄ID”时，才不可拖拽
            if (!string.IsNullOrEmpty(targetDisplayHeroID) && cardID == targetDisplayHeroID)
            {
                isDraggable = false; // 这是“正宫”
            }
            else
            {
                isDraggable = true; // 其他的（哪怕Tag符合，但ID不同）都是普通卡
            }
        }

        if (!isDraggable) lockedItemCount++;

        view.Init(cardID, count, this, isDraggable);
        deckViews.Add(view);
    }
    // ==================== 拖拽排序逻辑优化 ====================

    public void HandleDragMove(RectTransform draggingObject)
    {
        dropPlaceholder.SetActive(true);

        int closestIndex = lockedItemCount; // 默认最小插入位置是锁定区之后
        float closestDistance = float.MaxValue;

        // 我们只在 "可拖拽区域" 内寻找插入点
        // 从 lockedItemCount 开始遍历
        for (int i = 0; i < deckViews.Count; i++)
        {
            // 如果这个 View 是锁定的，跳过它的位置计算（不能插到它前面）
            if (i < lockedItemCount) continue;

            if (deckViews[i].transform == draggingObject.transform) continue;

            float distance = Mathf.Abs(deckViews[i].transform.position.y - draggingObject.position.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                if (draggingObject.position.y > deckViews[i].transform.position.y)
                {
                    closestIndex = deckViews[i].transform.GetSiblingIndex();
                }
                else
                {
                    closestIndex = deckViews[i].transform.GetSiblingIndex() + 1;
                }
            }
        }

        // 强制约束：索引不能小于锁定数量
        if (closestIndex < lockedItemCount) closestIndex = lockedItemCount;
        // 强制约束：索引不能超过子物体总数
        if (closestIndex > contentRect.childCount) closestIndex = contentRect.childCount;

        dropPlaceholder.transform.SetSiblingIndex(closestIndex);
    }

    public void HandleDragEnd(DeckCardView draggingView)
    {
        int newIndex = dropPlaceholder.transform.GetSiblingIndex();

        // 双重保险：防止由于UI刷新延迟导致的越界
        if (newIndex < lockedItemCount) newIndex = lockedItemCount;

        draggingView.transform.SetSiblingIndex(newIndex);
        dropPlaceholder.SetActive(false);

        // 重建 ID 列表
        List<string> newDeckOrderIDs = new List<string>();

        // 获取当前顺序的 View
        var orderedViews = contentRect.GetComponentsInChildren<DeckCardView>(false)
            .OrderBy(v => v.transform.GetSiblingIndex())
            .ToList();

        foreach (var view in orderedViews)
        {
            if (DeckManager.Instance.GetCardCounts().TryGetValue(view.CardID, out int count))
            {
                for (int i = 0; i < count; i++)
                {
                    newDeckOrderIDs.Add(view.CardID);
                }
            }
        }

        DeckManager.Instance.ReorderDeck(newDeckOrderIDs);
    }
}
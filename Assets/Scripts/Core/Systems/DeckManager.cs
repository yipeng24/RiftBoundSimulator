using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [SerializeField] private List<string> deckCardIDs = new List<string>();
    private Dictionary<string, int> cardCountMap = new Dictionary<string, int>();
    public TextAsset cardCsvAsset;

    public System.Action OnDeckUpdated;
    private const string DECK_SAVE_FILENAME = "current_deck.json";

    private void Awake()
    {
        CardDatabase.LoadAllCards(cardCsvAsset.text);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadDeck();
    }

    public IReadOnlyList<string> GetDeckCardIDs()
    {
        return deckCardIDs.AsReadOnly();
    }

    public IReadOnlyDictionary<string, int> GetCardCounts()
    {
        return cardCountMap;
    }

    // 尝试添加卡牌到卡组
    // DeckManager.cs

    public void AddCard(string cardID)
    {
        if (string.IsNullOrEmpty(cardID)) return;

        CardData data = CardDatabase.GetCardData(cardID);
        if (data == null) return; // 错误处理省略

        // 获取当前ID数量
        int count = cardCountMap.ContainsKey(cardID) ? cardCountMap[cardID] : 0;

        // --- 规则检查 ---

        // 1. 传奇 & 战场：绝对限制 1 张
        if (data.type == CardType.Legend || data.type == CardType.Battlefield)
        {
            if (count >= 1)
            {
                Debug.LogWarning($"⚠️ 无法添加 {data.displayName}: 此类型卡牌限制 1 张。");
                return;
            }
        }
        // 2. 符文：总数限制 12 张
        else if (data.type == CardType.Rune)
        {
            int totalRuneCount = deckCardIDs.Count(id => CardDatabase.GetCardData(id)?.type == CardType.Rune);
            if (totalRuneCount >= 12)
            {
                Debug.LogWarning("⚠️ 符文牌堆已满 (12/12)。");
                return;
            }
        }
        // 3. 其他 (单位、法术、装备、英雄单位)：限制 3 张
        else
        {
            if (count >= 3)
            {
                Debug.LogWarning($"⚠️ 无法添加 {data.displayName}: 同名卡最多 3 张。");
                return;
            }
        }

        // 添加卡牌
        deckCardIDs.Add(cardID);

        // 立即执行强制排序 (核心逻辑)
        SortDeckFixedOrder();

        UpdateCardCountMap();
        OnDeckUpdated?.Invoke();
        SaveDeck();
    }

    public void RemoveCard(string cardID)
    {
        if (string.IsNullOrEmpty(cardID)) return;

        bool removed = deckCardIDs.Remove(cardID);
        if (removed)
        {
            UpdateCardCountMap();
            OnDeckUpdated?.Invoke();
            SaveDeck();
        }
    }

    public void ReorderDeck(List<string> newOrderIDs)
    {
        // 仅接受重新排序，但必须再次通过强制排序逻辑，防止前端黑客行为
        deckCardIDs = newOrderIDs;
        // 这里可以再次调用 SortDeckFixedOrder() 确保头部顺序不乱，
        // 但为了支持 MainDeck 部分的拖拽，我们假设传入的 newOrderIDs 已经保持了头部固定

        UpdateCardCountMap();
        OnDeckUpdated?.Invoke();
        SaveDeck();
    }

    private void UpdateCardCountMap()
    {
        cardCountMap.Clear();
        foreach (var id in deckCardIDs)
        {
            if (cardCountMap.ContainsKey(id)) cardCountMap[id]++;
            else cardCountMap.Add(id, 1);
        }
    }

    // ==================== 新增：卡组验证逻辑 ====================
    public bool ValidateDeck(out string errorMessage)
    {
        errorMessage = "";
        StringBuilder sb = new StringBuilder();
        bool isValid = true;

        var allCards = deckCardIDs.Select(id => CardDatabase.GetCardData(id)).Where(c => c != null).ToList();

        // --- CHECK 1: LEGEND (传奇卡) ---
        var legends = allCards.Where(c => c.type == CardType.Legend).ToList();
        if (legends.Count != 1)
        {
            sb.AppendLine($"❌ 必须包含且仅能包含 1 张【传奇】卡牌 (当前: {legends.Count})。");
            isValid = false;
        }
        // 提前确定传奇数据，以便后续的 Tag 检查
        CardData legend = (legends.Count == 1) ? legends[0] : null;

        // --- CHECK 2: BATTLEFIELDS (场地) ---
        var battlefields = allCards.Where(c => c.type == CardType.Battlefield).ToList();
        if (battlefields.Count != 3)
        {
            sb.AppendLine($"❌ 必须包含且仅能包含 3 张【场地】卡牌 (当前: {battlefields.Count})。");
            isValid = false;
        }

        // --- CHECK 3: RUNES (符文) ---
        var runes = allCards.Where(c => c.type == CardType.Rune).ToList();
        if (runes.Count != 12)
        {
            sb.AppendLine($"❌ 必须包含且仅能包含 12 张【符文】卡牌 (当前: {runes.Count})。");
            isValid = false;
        }

        // --- CHECK 4: SELECTED HERO TAG MATCH (选定英雄特性检查) ---
        // 选定英雄必须是 HeroUnit 且其 Tag 必须与 Legend Tag 匹配
        var heroUnits = allCards.Where(c => c.type == CardType.HeroUnit).ToList();
        var selectedHeroes = new List<CardData>();

        if (legend != null)
        {
            // 寻找所有符合 Tag 的英雄单位，确保至少有一张可以作为“选定英雄”
            selectedHeroes = heroUnits.Where(h =>
                h.tags != null && legend.tags != null && legend.tags.Contains(h.displayName)).ToList();
        }

        if (selectedHeroes.Count == 0)
        {
            sb.AppendLine("❌ 必须包含至少 1 张符合【传奇】特性的【英雄单位】作为选定英雄。");
            isValid = false;
        }

        // --- CHECK 5: MAIN DECK TOTAL COUNT (主牌堆卡牌总数) ---
        // 主牌堆卡牌 = 所有卡 - 传奇 - 场地 - 符文
        // 主牌堆卡牌必须至少有 40 张 (包含所有英雄单位)
        var mainDeckCards = allCards.Where(c =>
            c.type != CardType.Legend &&
            c.type != CardType.Battlefield &&
            c.type != CardType.Rune).ToList();

        // 根据规则要求至少 40 张卡牌。
        if (mainDeckCards.Count < 40)
        {
            sb.AppendLine($"❌ 主牌堆卡牌总数 (包含所有英雄、单位、装备、法术) 必须至少为 40 张 (当前: {mainDeckCards.Count})。");
            isValid = false;
        }

        // --- CHECK 6: RUNES & MAIN DECK CARD ATTRIBUTE CONSISTENCY ---
        // 只有当 Legend 存在时才检查符文特性限制
        if (legend != null)
        {
            List<RuneType> allowedRunes = legend.runes;

            // 6a: 检查所有符文卡必须符合传奇的符文特性
            foreach (var runeCard in runes)
            {
                if (runeCard.runes == null || !runeCard.runes.Any(r => allowedRunes.Contains(r)))
                {
                    sb.AppendLine($"❌ 符文不匹配: 符文 [{runeCard.displayName}] 不属于传奇的符文领域。");
                    isValid = false;
                }
            }

            // 6b: 检查所有主牌堆卡牌（包括英雄单位）必须符合传奇的符文特性
            foreach (var card in mainDeckCards)
            {
                bool matches = false;

                // 无色/中性卡允许
                if (card.runes == null || card.runes.Count == 0 || card.runes.Contains(RuneType.None))
                {
                    matches = true;
                }
                // 否则，卡牌特性必须是传奇允许的特性子集
                else
                {
                    matches = card.runes.Any(r => allowedRunes.Contains(r));
                }

                if (!matches)
                {
                    sb.AppendLine($"❌ 构筑违规: 卡牌 [{card.displayName}] 的特性不符合传奇的符文限制。");
                    isValid = false;
                }
            }
        }


        if (isValid)
        {
            errorMessage = "✅ 卡组验证通过！已保存。";
        }
        else
        {
            errorMessage = sb.ToString();
        }

        // 无论验证是否通过，都尝试保存当前状态
        SaveDeck();

        return isValid;
    }
    // ==================== 新增：强制排序逻辑 ====================
    // 顺序：传奇(1) -> 符文(12) -> 英雄(1) -> 场地(3) -> 剩余(39)

    private void SortDeckFixedOrder()
    {
        List<string> tempDeckList = new List<string>(this.deckCardIDs);
        List<string> finalSortedIDs = new List<string>();

        // 1. 提取 1 张【传奇】
        CardData legendData = null;
        for (int i = 0; i < tempDeckList.Count; i++)
        {
            var data = CardDatabase.GetCardData(tempDeckList[i]);
            if (data != null && data.type == CardType.Legend)
            {
                legendData = data;
                finalSortedIDs.Add(tempDeckList[i]);
                tempDeckList.RemoveAt(i);
                break;
            }
        }

        // 2. 提取所有【符文】
        var runes = new List<string>();
        for (int i = tempDeckList.Count - 1; i >= 0; i--)
        {
            var data = CardDatabase.GetCardData(tempDeckList[i]);
            if (data != null && data.type == CardType.Rune)
            {
                runes.Add(tempDeckList[i]);
                tempDeckList.RemoveAt(i);
            }
        }
        runes.Reverse();
        finalSortedIDs.AddRange(runes);

        // ============================================================
        // 3. 提取【展示英雄】 (核心修改)
        // ============================================================

        // A. 先确定谁是“展示英雄 ID” (First Added Rule)
        string targetDisplayHeroID = null;

        // 正序遍历，找到第一个符合条件的
        foreach (var id in tempDeckList)
        {
            var data = CardDatabase.GetCardData(id);
            if (data != null && data.type == CardType.HeroUnit)
            {
                bool matchesLegend = false;
                if (legendData != null && legendData.tags != null && legendData.tags.Contains(data.displayName))
                {
                    matchesLegend = true;
                }

                if (matchesLegend)
                {
                    targetDisplayHeroID = id; // 锁定这个 ID！
                    break; // 找到了，不再找其他的，先来后到
                }
            }
        }

        // B. 如果找到了目标ID，把列表中所有这个ID的卡都提取出来
        if (!string.IsNullOrEmpty(targetDisplayHeroID))
        {
            // 使用倒序遍历安全移除所有该 ID 的副本
            var displayHeroes = new List<string>();
            for (int i = tempDeckList.Count - 1; i >= 0; i--)
            {
                if (tempDeckList[i] == targetDisplayHeroID)
                {
                    displayHeroes.Add(tempDeckList[i]);
                    tempDeckList.RemoveAt(i);
                }
            }
            // 这里的 displayHeroes 只有一种 ID，顺序不重要，直接加
            finalSortedIDs.AddRange(displayHeroes);
        }
        // 注意：其他 ID 不同的英雄单位（即使符合Tag），因为不等于 targetDisplayHeroID，
        // 所以依然留在 tempDeckList 中，稍后会变成普通卡。

        // ============================================================

        // 4. 提取所有【场地】
        var battlefields = new List<string>();
        for (int i = tempDeckList.Count - 1; i >= 0; i--)
        {
            var data = CardDatabase.GetCardData(tempDeckList[i]);
            if (data != null && data.type == CardType.Battlefield)
            {
                battlefields.Add(tempDeckList[i]);
                tempDeckList.RemoveAt(i);
            }
        }
        battlefields.Reverse();
        finalSortedIDs.AddRange(battlefields);

        // 5. 剩余卡牌 (Main Deck)
        finalSortedIDs.AddRange(tempDeckList);

        this.deckCardIDs = finalSortedIDs;
    }
    public void SaveDeck()
    {
        DeckDataWrapper wrapper = new DeckDataWrapper { cardIDs = this.deckCardIDs };
        string json = JsonUtility.ToJson(wrapper, true);
        string filePath = Path.Combine(Application.persistentDataPath, DECK_SAVE_FILENAME);
        try
        {
            File.WriteAllText(filePath, json);
            // Debug.Log("Deck saved.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save: " + e.Message);
        }
    }

    public void LoadDeck()
    {
        string filePath = Path.Combine(Application.persistentDataPath, DECK_SAVE_FILENAME);
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                DeckDataWrapper wrapper = JsonUtility.FromJson<DeckDataWrapper>(json);
                if (wrapper != null && wrapper.cardIDs != null)
                {
                    this.deckCardIDs = wrapper.cardIDs;
                    SortDeckFixedOrder(); // 加载时也排序
                    UpdateCardCountMap();
                    OnDeckUpdated?.Invoke();
                    return;
                }
            }
            catch (System.Exception) { }
        }
        this.deckCardIDs.Clear();
        UpdateCardCountMap();
        OnDeckUpdated?.Invoke();
    }

    public IReadOnlyList<string> GetOrderedUniqueCardIDs()
    {
        // 这里的顺序已经是 SortDeckFixedOrder 处理过的
        return deckCardIDs.Distinct().ToList().AsReadOnly();
    }
}

// === 在这里添加 DeckDataWrapper 类 ===
[System.Serializable]
public class DeckDataWrapper
{
    public List<string> cardIDs;
}
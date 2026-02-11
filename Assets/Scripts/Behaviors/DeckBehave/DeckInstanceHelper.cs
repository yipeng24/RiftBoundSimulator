//////////////////////////////////////////////////
/// 卡组的单例类，负责加载和提供卡牌基础数据。
/// 用于对外提供卡牌基础数据的访问接口。
/// 提供对卡牌数据的增删改查功能。
/// 提供的功能包括：
/// 1. 将卡牌添加到卡组中
/// 2. 按照特定顺序对卡组进行排序
/// 3. 保存卡组数据到本地文件。
/// 4. 检查卡组合法性
//////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class DeckDataWrapper
{
    public List<string> cardIDs;
}

public class DeckInstanceHelper : IDisposable
{
    public string _DeckName { get; }
    internal static DeckInstanceHelper Create(string DeckName, CardInstanceHelper cardHelper) => new DeckInstanceHelper(DeckName, cardHelper);
    private readonly CardInstanceHelper _cardInstanceHelper;
    // 卡牌类型计数器
    private Dictionary<CardType, int> cardTypeCountMap = new();
    // 卡牌ID计数器
    private Dictionary<string, int> cardIDCountMap = new();

    private List<RunePropertyType> deckRuneProperties = new();
    private CardBaseInfo LengendCard = null;
    private static bool existLegend = false;

    [SerializeField] private List<string> currentDeckCardIDs = new();
    // 暴露一个只读属性给外部
    public IReadOnlyList<string> CardIDs => currentDeckCardIDs;


    public DeckInstanceHelper(string DeckName, CardInstanceHelper cardHelper)
    {
        this._cardInstanceHelper = cardHelper;
        try
        {
            string fileName = DeckName.EndsWith(".json") ? DeckName : $"{DeckName}.json";
            string json = File.ReadAllText(Path.Combine(SysConfig.DEFAULT_DECK_FULL_PATH, fileName));
            //SysInfo.sysLogInfo(SysLogHead.Info, "DeckInstanceHelper init");
            DeckDataWrapper wrapper = JsonUtility.FromJson<DeckDataWrapper>(json);
            if (wrapper != null && wrapper.cardIDs != null)
            {
                this.currentDeckCardIDs = wrapper.cardIDs;
                this._DeckName = DeckName;
                UpdateDeckData();
            }
        }
        catch (System.Exception) { }
    }
    public void AddCardToDeck(string cardID)
    {
        // 获取卡牌基本信息
        CardBaseInfo cardBaseInfo = CardInstanceHelper.GetCardBaseData(cardID);

        if (cardBaseInfo == null) return;
        // 首先检查传奇，如果添加的不是传奇，且传奇不存在，提示先添加传奇，
        // 如果是传奇，但是传奇已经存在，提示传奇已经存在，不能在添加
        if (cardBaseInfo.cardType != CardType.Legend && !existLegend)
        {
            SysInfo.sysLogInfo(SysLogHead.Warning, "请先添加传奇");
            return;
        }
        else if(cardBaseInfo.cardType == CardType.Legend && existLegend)
        {
            SysInfo.sysLogInfo(SysLogHead.Warning, "已有传奇，不支持重复添加");
            return;
        }
        // 然后根据类型检查
        int count = cardIDCountMap.ContainsKey(cardID) ? cardIDCountMap[cardID] : 0;
        // 规则：同一张卡牌最多允许放入三张
        // 按照规定顺序排序：传奇x1 > 英雄 > 单位 > 法术 > 装备 > 符文 > 战场
        // 1. 数量限制
        if ( cardBaseInfo.cardType == CardType.Legend || cardBaseInfo.cardType == CardType.Battlefield )
        {
            if (count >= 1) 
            { 
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 此类型卡牌限制 1 张。"); 
                return; 
            }
            if (cardBaseInfo.cardType == CardType.Legend)
            {
                existLegend = true;
                this.deckRuneProperties = cardBaseInfo.runeProperties;
            }
        }
        else if(cardBaseInfo.cardType == CardType.ExSpell)
        {
            if (LengendCard.tags[0].Equals(cardBaseInfo.tags[0]))
            {
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 非传奇专属法术");
            }
            if (count >= 3)
            {
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 此类型卡牌限制 3 张。");
                return;
            }
        }
        else if (cardBaseInfo.cardType == CardType.Rune)
        {
            int totalRuneCount = cardTypeCountMap.ContainsKey(CardType.Rune) ? cardTypeCountMap[CardType.Rune] : 0;//currentDeckCardIDs.Count(id => _cardInstanceHelper.GetCardBaseData(id)?.cardType == CardType.Rune);
            if (totalRuneCount >= 12)
            {
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 此类型卡牌限制 12 张。");
                return;
            }
            if (cardBaseInfo.runeProperties == null || !cardBaseInfo.runeProperties.Any(rp => deckRuneProperties.Contains(rp)))
            {
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 符文属性不匹配。");
                return;
            }
        }
        else
        {
            if (count >= 3) 
            { 
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 同一张卡牌限制 3 张。");
                return; 
            }
            if (cardBaseInfo.runeProperties != null && !cardBaseInfo.runeProperties.Any(rp => deckRuneProperties.Contains(rp)))
            {
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 符文属性不匹配。");
                return;
            }
        }

        if (cardBaseInfo.cardType == CardType.Battlefield)
        {
            int totalRuneCount = cardTypeCountMap.ContainsKey(CardType.Battlefield) ? cardTypeCountMap[CardType.Battlefield] : 0;
            if (totalRuneCount >= 3)
            {
                SysInfo.sysLogInfo(SysLogHead.Warning, $"无法添加 {cardBaseInfo.displayName}: 场地卡限制共 3 张。");
                return;
            }
        }
        currentDeckCardIDs.Add(cardID);
        SortDeckOrder();
        UpdateDeckData();
        SaveDeckWithoutValidate();
    }
    public void RemoveCardFromDeck(string cardID)
    {
        currentDeckCardIDs.Remove(cardID);
        if (CardInstanceHelper.GetCardBaseData(cardID).cardType == CardType.Legend)
            existLegend = false;
        UpdateDeckData();
        SaveDeckWithoutValidate();
    }
    private void SortDeckOrder()
    {
        List<string> tempDeckList = new List<string>(this.currentDeckCardIDs);
        List<string> finalSortedIDs = new List<string>();

        // 1. 提取 1 张【传奇】
        LengendCard=null;
        for (int i = 0; i < tempDeckList.Count; i++)
        {
            var data = CardInstanceHelper.GetCardBaseData(tempDeckList[i]);
            if (data != null && data.cardType == CardType.Legend)
            {
                LengendCard = data;
                finalSortedIDs.Add(tempDeckList[i]); //只存ID
                tempDeckList.RemoveAt(i);
                break;
            }
        }
        // --->到这里 【传奇】 | 其他乱序
        // 2. 提取【展示英雄】
        string targetDisplayHeroID = null;
        // 正序遍历，找到第一个符合条件的
        foreach (var id in tempDeckList)
        {
            var data = CardInstanceHelper.GetCardBaseData(id);
            if (data != null && data.cardType == CardType.HeroUnit)
            {
                if (LengendCard != null && LengendCard.tags != null && LengendCard.tags.Contains(data.displayName))
                {
                    targetDisplayHeroID = id; // 锁定这个 ID！
                    break;
                }
            }
        }
        // 如果找到了目标ID，把列表中所有这个ID的卡都提取出来
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

        // 3. 提取所有【符文】
        var runes = new List<string>();
        for (int i = 0; i < tempDeckList.Count; i++)
        {
            var data = CardInstanceHelper.GetCardBaseData(tempDeckList[i]);
            if (data != null && data.cardType == CardType.Rune)
            {
                runes.Add(tempDeckList[i]);
                tempDeckList.RemoveAt(i);
                i--;                            // 调整索引以避免跳过下一张卡
                if (runes.Count >= 12) break;   // 符文最多12张，提前结束循环
            }
        }
        finalSortedIDs.AddRange(runes);

        
        // 4. 提取所有【场地】
        var battlefields = new List<string>();
        for (int i = tempDeckList.Count - 1; i >= 0; i--)
        {
            var data = CardInstanceHelper.GetCardBaseData(tempDeckList[i]);
            if (data != null && data.cardType == CardType.Battlefield)
            {
                battlefields.Add(tempDeckList[i]);
                tempDeckList.RemoveAt(i);
            }
        }
        battlefields.Reverse();
        finalSortedIDs.AddRange(battlefields);

        // 5. 剩余卡牌 (Main Deck)
        tempDeckList.Sort();
        finalSortedIDs.AddRange(tempDeckList);

        this.currentDeckCardIDs = finalSortedIDs;
    }
    // 更新卡组数据。更新传奇和卡色情况。
    private void UpdateDeckData()
    {
        cardIDCountMap.Clear();
        cardTypeCountMap.Clear();
        existLegend = false;
        LengendCard = null;
        deckRuneProperties = null;
        foreach (var id in currentDeckCardIDs)
        {
            if (cardIDCountMap.ContainsKey(id)) cardIDCountMap[id]++;
            else cardIDCountMap.Add(id, 1);

            CardBaseInfo card = CardInstanceHelper.GetCardBaseData(id);
            if (cardTypeCountMap.ContainsKey(card.cardType)) cardTypeCountMap[card.cardType]++;
            else cardTypeCountMap.Add(card.cardType, 1);

            if (card.cardType == CardType.Legend)
            {
                existLegend = true;
                LengendCard = card;
                deckRuneProperties = card.runeProperties;
            }
        }
    }
    public void UpdateDeckSort(List<string> newOrderIDs)
    {
        currentDeckCardIDs = newOrderIDs;
        UpdateDeckData();
        SaveDeckWithoutValidate();
    }
    public bool ValidateDeck(out StringBuilder errorMessage)
    {
        // 先排序
        errorMessage = new();
        bool isValid = true;
        if (currentDeckCardIDs.Count == 0)
        {
            errorMessage.Append("Empty Deck");
            return false; 
        }
        SortDeckOrder();

        var allCards = currentDeckCardIDs.Select(id => CardInstanceHelper.GetCardBaseData(id)).Where(c => c != null).ToList();
        if (allCards.Count != 56)
        {
            errorMessage.AppendLine( $"卡组必须包含 56 张卡牌，目前包含 {allCards.Count} 张。");
            isValid = false;
        }
        var typeCount = allCards.GroupBy(c => c.cardType)
                        .ToDictionary(g => g.Key, g => g.Count());

        if (!typeCount.ContainsKey(CardType.Legend))
        {
            errorMessage.AppendLine("未检测到传奇");
            isValid = false;
        }
        else if(typeCount[CardType.Legend] != 1)
        {
            errorMessage.AppendLine("卡组中的传奇卡牌只能有 1 张。");
            isValid = false;
        }
        if (currentDeckCardIDs.Count < 2)
        {
            errorMessage.Append("卡组中不存在合规英雄单位");
            isValid = false;
        }
        else if (allCards[1].cardType != CardType.HeroUnit || !LengendCard.tags.Contains(allCards[1].displayName))
        {
            errorMessage.AppendLine("卡组中不存在合规英雄单位");
            isValid = false;
        }

        if (!typeCount.ContainsKey(CardType.Rune))
        {
            errorMessage.AppendLine("未检测到符文");
            isValid = false;
        }
        else if (typeCount[CardType.Rune] != 12)
        {
            errorMessage.AppendLine($"卡组中的符文卡牌必须有 12 张，目前有 {typeCount[CardType.Rune]} 张。");
            isValid = false;
        }

        if (!typeCount.ContainsKey(CardType.Battlefield))
        {
            errorMessage.AppendLine("未检测到战场");
            isValid = false;
        }
        else if (typeCount[CardType.Battlefield] != 3)
        {
            errorMessage.AppendLine($"卡组中的战场卡牌必须有 3 张，目前有 {typeCount[CardType.Battlefield]} 张。");
            isValid = false;
        }
        SaveDeckWithoutValidate();//保存当前状态
        return isValid;
    }
    public void SaveDeckWithoutValidate()
    {
        DeckDataWrapper wrapper = new DeckDataWrapper { cardIDs = this.currentDeckCardIDs };
        string json = JsonUtility.ToJson(wrapper, true);
        string filePath = Path.Combine(SysConfig.DEFAULT_DECK_FULL_PATH, $"{_DeckName}.json");
        try
        {
            File.WriteAllText(filePath, json);
            SysInfo.sysLogInfo(SysLogHead.Info, $"saved {this._DeckName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save: " + e.Message);
        }
    }
    public void Dispose()
    {
        SaveDeckWithoutValidate();
    }
}


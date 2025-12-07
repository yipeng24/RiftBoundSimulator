using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // 用于 Debug.Log 和 TextAsset

public static class CardDatabase
{
    private static Dictionary<string, CardData> database = new Dictionary<string, CardData>();
    private static Dictionary<string, Sprite> artCache = new Dictionary<string, Sprite>();
    public static CardData GetCardData(string id)
    {
        if (database.TryGetValue(id, out CardData card))
        {
            return card;
        }
        Debug.LogError($"[CardDatabase] 找不到 CardID: {id}");
        return null;
    }

    /// <summary>
    /// 从 CSV 内容中加载所有卡牌数据。
    /// 假设 CSV 字段名：cardID, runeDominian, displayName, cardType, tags, manacost, runeCostNum, power, health, ruleText, flavorText, artName
    /// </summary>
    /// <param name="csvContent">RiftboundCardList.csv 的文本内容</param>
    public static void LoadAllCards(string csvContent)
    {
        database.Clear();

        // --- CSV 解析逻辑开始 ---
        if (string.IsNullOrEmpty(csvContent))
        {
            Debug.LogError("[CardDatabase] CSV 内容为空，加载失败。");
            return;
        }

        string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogError("[CardDatabase] CSV 行数不足（可能没有数据或只有表头）。");
            return;
        }

        // 假设第一行是表头
        string[] headers = lines[0].Split(',');

        int loadedCount = 0;
        // 从第二行开始遍历数据
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length != headers.Length)
            {
                Debug.LogWarning($"[CardDatabase Warning] 行 {i + 1} 字段数量不匹配，跳过。");
                continue;
            }

            // 将当前行转换为方便查找的字典
            Dictionary<string, string> record = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                // 移除可能的空格和引号
                string header = headers[j].Trim();
                string value = values[j].Trim().Replace("\"", "");
                record[header] = value;
            }

            // --- 核心数据映射和加载逻辑 ---
            string cardID = record.ContainsKey("cardID") ? record["cardID"] : null;
            if (string.IsNullOrEmpty(cardID))
            {
                Debug.LogWarning($"[CardDatabase Warning] 行 {i + 1} 缺少 cardID，跳过。");
                continue;
            }

            try
            {
                CardData card = new CardData
                {
                    cardID = cardID,
                    displayName = record.ContainsKey("displayName") ? record["displayName"] : string.Empty,
                    artName = record.ContainsKey("artName") ? record["artName"] : string.Empty,

                    // 费用与战力（使用 TryParse 进行安全转换）
                    manaCost = int.TryParse(record.ContainsKey("manacost") ? record["manacost"] : "0", out int mCost) ? mCost : 0,
                    basePower = int.TryParse(record.ContainsKey("power") ? record["power"] : "0", out int p) ? p : 0,
                    baseHealth = int.TryParse(record.ContainsKey("health") ? record["health"] : "0", out int h) ? h : 0,

                    // 核心转换逻辑
                    type = ParseCardType(record.ContainsKey("cardType") ? record["cardType"] : "单位"),
                    tags = ParseDelimitedString(record.ContainsKey("tags") ? record["tags"] : string.Empty, '+'),

                    // 符文特性
                    runes = ParseRuneTypes(record.ContainsKey("runeDomian") ? record["runeDomian"] : string.Empty),

                    // 符能费用：假设符能类型与卡牌特性相同，数量由 runeCostNum 决定
                    runeCost = ParseRuneCosts(
                        record.ContainsKey("runeCostType") ? record["runeCostType"] : string.Empty,
                        record.ContainsKey("runeCostNum") ? record["runeCostNum"] : "0"
                    ),

                    ruleText = record.ContainsKey("ruleText") ? record["ruleText"] : string.Empty,
                    flavorText = record.ContainsKey("flavorText") ? record["flavorText"] : string.Empty
                };

                // 提取关键词
                card.keywords = ExtractKeywords(card.ruleText);

                database.Add(card.cardID, card);
                loadedCount++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardDatabase Error] 加载 CardID: {cardID} 失败. 错误: {ex.Message}");
            }
        }

        Debug.Log($"[System] 所有卡牌数据加载完成。共加载 {loadedCount} 张卡牌。");
    }

    // --- 辅助解析函数：确保与 CardData.cs 中的枚举定义匹配 ---

    private static CardType ParseCardType(string typeString)
    {
        switch (typeString)
        {
            case "传奇": return CardType.Legend;
            case "单位": return CardType.Unit;
            case "法术": return CardType.Spell;
            case "装备": return CardType.Equipment;
            case "符文": return CardType.Rune;
            case "战场": return CardType.Battlefield;
            case "传说": return CardType.Legend;
            case "英雄单位": return CardType.HeroUnit;
            // 更多类型...
            default: return CardType.Unit;
        }
    }

    private static RuneType ParseRuneType(string runeStr)
    {
        // 确保输入字符串干净
        runeStr = runeStr.Trim();

        // 实现中文到枚举的映射
        switch (runeStr)
        {
            case "炽烈": return RuneType.Fervor;
            case "翠意": return RuneType.Verdant;
            case "灵光": return RuneType.Brilliant;
            case "摧破": return RuneType.Shatter;
            case "混沌": return RuneType.Chaos;
            case "序理": return RuneType.Order;
            default: return RuneType.None;
        }
    }

    private static List<RuneType> ParseRuneTypes(string runeDominian)
    {
        // 处理您提到的 '+' 分隔符
        List<RuneType> runes = new List<RuneType>();
        if (string.IsNullOrEmpty(runeDominian)) return runes;

        foreach (var runeStr in runeDominian.Split('+').Select(s => s.Trim()))
        {
            RuneType rune = ParseRuneType(runeStr);
            if (rune != RuneType.None)
            {
                runes.Add(rune);
            }
        }
        return runes;
    }

    // 假设符文费用类型与卡牌特性相同，数量由 runeCostNum 决定
    private static List<RuneType> ParseRuneCosts(string runeCostType, string costNumString)
    {
        List<RuneType> costs = new List<RuneType>();
        int num = int.TryParse(costNumString, out int n) ? n : 0;

        // 解析符能类型，这里假设 runeCostType 是单一特性（例如：“摧破”），不是“摧破+混沌”
        RuneType costType = ParseRuneType(runeCostType);

        if (costType != RuneType.None)
        {
            for (int i = 0; i < num; i++)
            {
                costs.Add(costType);
            }
        }
        return costs;
    }

    private static List<string> ParseDelimitedString(string data, char delimiter)
    {
        if (string.IsNullOrEmpty(data)) return new List<string>();
        return data.Split(delimiter).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    private static List<KeywordType> ExtractKeywords(string ruleText)
    {
        // 关键：实现中文关键词识别 (匹配规则文本中包含的关键词)
        List<KeywordType> keywords = new List<KeywordType>();
        if (string.IsNullOrEmpty(ruleText)) return keywords;

        // 根据规则手册和常见卡牌关键词进行映射
        if (ruleText.Contains("迅捷")) keywords.Add(KeywordType.Quick); //
        if (ruleText.Contains("反应")) keywords.Add(KeywordType.Reactive); //
        if (ruleText.Contains("强攻")) keywords.Add(KeywordType.Aggressive); //
        if (ruleText.Contains("坚守")) keywords.Add(KeywordType.Defensive); //
        if (ruleText.Contains("法盾")) keywords.Add(KeywordType.SpellShield); //
        if (ruleText.Contains("游走")) keywords.Add(KeywordType.Mobile); //
        if (ruleText.Contains("绝念")) keywords.Add(KeywordType.Afterthought); //
        if (ruleText.Contains("预知")) keywords.Add(KeywordType.Foresight); //
        if (ruleText.Contains("瞬息")) keywords.Add(KeywordType.Momentary); //

        // 可以在这里添加更多的关键词判断

        return keywords.Distinct().ToList(); // 确保关键词不重复
    }

    public static Dictionary<string, CardData> GetAllCardData()
    {
        // 返回内部的字典（确保在 LoadAllCards 成功后调用）
        return database;
    }

    // 根据 artName 加载图片
    public static Sprite GetArt(string artName)
    {
        if (string.IsNullOrEmpty(artName)) return null;

        if (artCache.TryGetValue(artName, out var sp))
            return sp;

        // 假设图片资源路径是 Resources/CardArts/
        // 注意：实际项目中可能需要使用 AssetBundle 或 Addressables
        Sprite loadedSprite = Resources.Load<Sprite>($"Cards/Arts/{artName}");

        if (loadedSprite != null)
        {
            artCache.Add(artName, loadedSprite);
        }
        else
        {
            Debug.LogWarning($"[CardDatabase] 找不到卡图资源: Cards/Arts/{artName}");
        }

        return loadedSprite;
    }
}
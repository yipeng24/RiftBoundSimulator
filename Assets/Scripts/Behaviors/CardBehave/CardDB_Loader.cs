///////////////////////////////////////
/// cardDB_Loader.cs
/// Author: Francle
/// Function: 加载卡牌数据库
///////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class CardDB_Loader
{
    private static Dictionary<string, CardBaseInfo> database = new Dictionary<string, CardBaseInfo>();

    public static Dictionary<string, CardBaseInfo> LoadAllCards(string csvContent)
    {
        database.Clear();
        // 1. 空文件检查
        if (string.IsNullOrEmpty(csvContent))
        {
            Debug.LogError("[CardDatabase] CSV 内容为空，加载失败。");
            return database;
        }
        // 2. 文件有效性（行数检查）
        string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogError("[CardDatabase] CSV 行数不足（可能没有数据或只有表头）。");
            return database;
        }
        // 3. 解析表头
        string[] headers = lines[0].Split(',');
        int loadedCount = 0;
        // 4. 逐行解析数据
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length != headers.Length) // 字段数量检查
            {
                Debug.LogWarning($"[CardDatabase Warning] 行 {i + 1} 字段数量不匹配，跳过。");
                continue;
            }
            Dictionary<string, string> record = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length; j++)
            {
                // 移除可能的空格和引号
                string header = headers[j].Trim();
                string value = values[j].Trim().Replace("\"", "");
                record[header] = value;
            }
            string cardID = record.ContainsKey("cardID") ? record["cardID"] : null;
            if (string.IsNullOrEmpty(cardID))
            {
                Debug.LogWarning($"[CardDatabase Warning] 行 {i + 1} 缺少 cardID，跳过。");
                continue;
            }
            // 5. 创建 CardBaseInfo 对象
            try
            {
                CardBaseInfo card = new CardBaseInfo
                {
                    cardID = cardID,
                    displayName = record.ContainsKey("displayName") ? record["displayName"] : string.Empty,
                    cardType = ParseCardType(record.ContainsKey("cardType") ? record["cardType"] : string.Empty),
                    runeProperties = ParseRunePropertiesType(record.ContainsKey("runeDomian") ? record["runeDomian"] : string.Empty),
                    tags = ParseTags(record.ContainsKey("tags") ? record["tags"] : string.Empty),
                    artName = record.ContainsKey("artName") ? record["artName"] : string.Empty,

                    manaCost = int.TryParse(record.ContainsKey("manacost") ? record["manacost"] : "0", out int mCost) ? mCost : 0,
                    runeCost = ParseRuneCost(
                        record.ContainsKey("runeCostType") ? record["runeCostType"] : string.Empty,
                        record.ContainsKey("runeCostNum") ? record["runeCostNum"] : "0"
                    ),
                    basePower = int.TryParse(record.ContainsKey("power") ? record["power"] : "0", out int power) ? power : 0,

                    ruleText = record.ContainsKey("ruleText") ? record["ruleText"] : string.Empty,
                    flavorText = record.ContainsKey("flavorText") ? record["flavorText"] : string.Empty
                };

                database.Add(card.cardID,card);
                loadedCount++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardDatabase Error] 加载 CardID: {cardID} 失败. 错误: {ex.Message}");
            }

            Debug.Log($"[System] 所有卡牌数据加载完成。共加载 {loadedCount} 张卡牌。");
        }
        return database;
    }

    private static CardType ParseCardType(string typeString)
    {
        switch (typeString)
        {
            case "传奇": return CardType.Legend;
            case "英雄单位": return CardType.HeroUnit;
            case "单位": return CardType.Unit;
            case "法术": return CardType.Spell;
            case "装备": return CardType.Equipment;
            case "符文": return CardType.Rune;
            case "战场": return CardType.Battlefield;
        }
        return CardType.Unit;
    }

    private static RunePropertyType ParseRunePropertyType(string propertyString)
    {
        switch (propertyString)
        {
            case "炽烈": return RunePropertyType.Red;
            case "摧破": return RunePropertyType.Orange;
            case "序理": return RunePropertyType.Yellow;
            case "翠意": return RunePropertyType.Green;
            case "灵光": return RunePropertyType.Blue;
            case "混沌": return RunePropertyType.Purple;
            default: return RunePropertyType.None;
        }
    }

    private static List<RunePropertyType> ParseRunePropertiesType(string runePropertyString)
    {
        List<RunePropertyType> runes = new List<RunePropertyType>();
        if (string.IsNullOrEmpty(runePropertyString)) return runes;
        // 假设符文属性在 CSV 中以 "+" 分隔，例如 "炽烈 + 翠意"
        foreach (var runeStr in runePropertyString.Split('+').Select(s => s.Trim()))
        {
            RunePropertyType rune = ParseRunePropertyType(runeStr);
            if (rune != RunePropertyType.None)
            {
                runes.Add(rune);
            }
        }
        return runes;
    }

    private static List<string> ParseTags(string tagString)
    {
        if (string.IsNullOrEmpty(tagString)) return new List<string>();
        return tagString.Split("+").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    private static CardRuneCost ParseRuneCost(string costTypeStr, string costNumStr)
    {
        CardRuneCost requirements = new CardRuneCost();
        int num = int.TryParse(costNumStr, out int n) ? n : 0;

        if (num <= 0) return requirements;

        string[] types = costTypeStr.Split('+');
        foreach (var t in types)
        {
            RunePropertyType rt = ParseRunePropertyType(t);
            if (rt != RunePropertyType.None) requirements.acceptableTypes.Add(rt);
        }
        requirements.amount = num;

        return requirements;
    }
}

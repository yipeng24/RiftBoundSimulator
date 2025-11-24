// EffectDataLoaderw.cs
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public static class EffectDatabase
{
    private static Dictionary<string, CardEffectDefinition> effectMap = new Dictionary<string, CardEffectDefinition>();

    // ----------------------------------------------------
    // 核心 API
    // ----------------------------------------------------

    public static CardEffectDefinition GetEffectDefinition(string cardID)
    {
        if (effectMap.TryGetValue(cardID, out var def))
        {
            return def;
        }
        return null;
    }

    // ----------------------------------------------------
    // 加载逻辑
    // ----------------------------------------------------

    /// <summary>
    /// 从 JSON 内容中加载所有卡牌效果定义。
    /// </summary>
    /// <param name="jsonContent">CardEffects.json 的文本内容</param>
    public static void LoadAllEffects(string jsonContent)
    {
        effectMap.Clear();

        if (string.IsNullOrEmpty(jsonContent))
        {
            Debug.LogError("[EffectDatabase] JSON 内容为空，加载失败。");
            return;
        }

        try
        {
            // 使用自定义设置，确保 EffectConverter 被用于反序列化 BaseEffect
            List<CardEffectDefinition> definitions = JsonConvert.DeserializeObject<List<CardEffectDefinition>>(jsonContent, new JsonSerializerSettings
            {
                Converters = { new EffectConverter() }
            });

            if (definitions != null)
            {
                foreach (var def in definitions)
                {
                    if (!string.IsNullOrEmpty(def.cardID) && !effectMap.ContainsKey(def.cardID))
                    {
                        effectMap.Add(def.cardID, def);
                    }
                }
            }
            Debug.Log($"[System] 卡牌效果定义加载完成。共加载 {effectMap.Count} 张卡牌的效果。");
        }
        catch (System.Exception ex)
        {
            // 捕获和记录任何 JSON 解析或转换错误
            Debug.LogError($"[EffectDatabase Error] JSON 解析失败. 错误: {ex.Message}");
        }
    }
}
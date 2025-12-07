using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; // 依然建议用 Newtonsoft，因为它比 Unity自带的更稳健，但用 JsonUtility 也可以了

// -------------------------------------------------------------------------
// 功能：效果数据库系统
// 职责：读取 JSON 配置文件，提供根据 CardID 查询效果定义的接口。
// -------------------------------------------------------------------------

public static class EffectDatabase
{
    // 缓存字典：Key = CardID
    private static Dictionary<string, CardEffectDefinition> effectMap = new Dictionary<string, CardEffectDefinition>();

    /// <summary>
    /// 获取某张卡牌的所有效果定义
    /// </summary>
    public static CardEffectDefinition GetEffect(string cardID)
    {
        if (effectMap.TryGetValue(cardID, out var def))
        {
            return def;
        }
        return null; // 该卡牌没有配置特殊效果（比如白板单位）
    }

    /// <summary>
    /// 加载所有效果配置 (在游戏启动或 Loading 时调用)
    /// </summary>
    /// <param name="jsonContent">CardEffects.json 的内容</param>
    public static void LoadEffects(string jsonContent)
    {
        effectMap.Clear();

        if (string.IsNullOrEmpty(jsonContent))
        {
            Debug.LogError("[EffectDatabase] JSON 内容为空");
            return;
        }

        try
        {
            // 直接反序列化为列表，无需转换器
            var list = JsonConvert.DeserializeObject<List<CardEffectDefinition>>(jsonContent);

            if (list != null)
            {
                foreach (var def in list)
                {
                    if (!string.IsNullOrEmpty(def.cardID))
                    {
                        if (!effectMap.ContainsKey(def.cardID))
                            effectMap.Add(def.cardID, def);
                        else
                            Debug.LogWarning($"[EffectDatabase] 重复的 CardID 配置: {def.cardID}");
                    }
                }
            }
            Debug.Log($"[EffectDatabase] 加载完成，共 {effectMap.Count} 条数据。");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EffectDatabase] JSON 解析错误: {ex.Message}");
        }
    }
}
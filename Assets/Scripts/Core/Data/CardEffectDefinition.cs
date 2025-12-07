using System;
using System.Collections.Generic;
using RiftBound.Core;

// -------------------------------------------------------------------------
// 功能：定义卡牌效果的数据结构
// 变更：移除了继承结构，改为统一的 "EffectData" 类，方便 JSON 序列化和配置。
// -------------------------------------------------------------------------

[Serializable]
public class CardEffectDefinition
{
    // 对应 CardData 中的 cardID
    public string cardID;

    // 效果触发列表 (一张卡可能有多个时机的效果，比如打出时造成伤害，遗愿时抽牌)
    public List<EffectTriggerGroup> triggerGroups = new List<EffectTriggerGroup>();
}

[Serializable]
public class EffectTriggerGroup
{
    // 触发时机 (OnPlay, OnDeath, etc.)
    public EffectTrigger trigger;

    // 效果链 (顺序执行的一组效果)
    public List<EffectData> effects = new List<EffectData>();
}

[Serializable]
public class EffectData
{
    // --- 核心行为 ---
    public EffectActionType actionType; // 必填：是伤害？抽牌？还是Buff？

    // --- 数值参数 (通用的 Value 字段) ---
    public int value1;       // 主要数值 (伤害量、抽牌数、Buff值)
    public int value2;       // 次要数值 (如需要)
    public string strValue;  // 字符串参数 (要召唤的单位ID、Buff的ID、音频名等)

    // --- 目标参数 ---
    public TargetType targetType;       // 目标类型 (Unit, Player, Self...)
    public TargetLocation targetLoc;    // 目标区域 (Battlefield, Hand...)
    public int targetCount = 1;         // 目标数量
    public bool isFriendly = false;     // 是找友方(true)还是敌方(false)

    // --- 条件参数 (可选) ---
    // 如果没有条件，留空即可
    public string conditionType;        // 例如 "TargetIsStunned"
    public int conditionValue;

    // --- UI/表现参数 ---
    public string vfxName;              // 播放的特效名称
}
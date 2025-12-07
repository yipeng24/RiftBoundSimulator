// CardEffectDefinition.cs
using System;
using System.Collections.Generic;
using Newtonsoft.Json; // 依赖 Newtonsoft.Json 库
// ----------------------------------------------------
// 核心数据模型
// ----------------------------------------------------
[Serializable]
public class CardEffectDefinition
{
    public string cardID;

    // 该卡牌效果的触发时机（例如：OnPlay, Passive）
    public EffectTrigger trigger;

    // 一个触发器可以引发出一个或多个串联/并行的效果链
    public List<BaseEffect> effectsChain;
}


// ----------------------------------------------------
// 组件：目标定义 (TargetDefinition)
// ----------------------------------------------------

[Serializable]
public class TargetDefinition
{
    public TargetType type;
    public TargetLocation location;

    // 目标数量，-1 表示所有目标，1 表示单个目标
    public int count = 1;

    // 用于指定目标是友方(true)还是敌方(false)
    public bool isFriendly = true;

    // 当 type 为 Targeted 时，引用效果链中其他效果的ID
    public string refEffectID;

    // 扩展字段，用于处理“任意位置”或“所有战场”
    public bool allowAnyLocation = false;
}


// ----------------------------------------------------
// 组件：条件 (Condition) - 复杂效果的基石
// ----------------------------------------------------

[Serializable]
public class Condition
{
    // 例如：TargetDestroyed (目标被摧毁), OtherCardPlayedThisTurn (本回合打出过其他卡牌)
    public string type;
    public string refEffectID; // 引用 BaseEffect 的 ID
    public int value;          // 数值条件
}


// ----------------------------------------------------
// 核心效果基类 (BaseEffect)
// ----------------------------------------------------

[Serializable]
// 重点：使用 JsonConverter 来反序列化 BaseEffect 的具体子类
[JsonConverter(typeof(EffectConverter))]
public abstract class BaseEffect
{
    // 用于效果链引用和条件判断
    public string id;

    public Condition condition;

    public TargetDefinition target;

    // 所有效果的执行入口
    public abstract void Execute(object invoker, List<object> targets);
}


// ----------------------------------------------------
// 扩展点：具体效果的实现 (Action Classes)
// ----------------------------------------------------

// 1. OGN_005, OGN_024 使用: 造成伤害
[Serializable]
public class DamageEffect : BaseEffect
{
    public int damageValue; // 伤害数值

    public override void Execute(object invoker, List<object> targets)
    {
        // 游戏逻辑占位符
    }
}

// 2. OGN_005, OGN_024 使用: 抽卡
[Serializable]
public class DrawCardEffect : BaseEffect
{
    public int cardCount; // 抽卡数量

    public override void Execute(object invoker, List<object> targets)
    {
        // 游戏逻辑占位符
    }
}

// 3. OGN_016 使用: 单位增益
[Serializable]
public class BuffUnitEffect : BaseEffect
{
    public string buffType; // "Defense", "Power"
    public int value;
    public string duration; // "CurrentTurn", "Permanent"

    public override void Execute(object invoker, List<object> targets)
    {
        // 游戏逻辑占位符
    }
}

// 4. OGN_031 使用: 玩家增益/状态修改
[Serializable]
public class BuffPlayerEffect : BaseEffect
{
    public string buffType; // "NextSpellCostReduction"
    public int value;
    public string duration; // "CurrentTurn"

    public override void Execute(object invoker, List<object> targets)
    {
        // 游戏逻辑占位符
    }
}

// 5. OGN_040 使用: 获得符能
[Serializable]
public class GainRuneEffect : BaseEffect
{
    public RuneType runeType;
    public int count;
    public string restriction; // "ForRuneCostOnly"

    public override void Execute(object invoker, List<object> targets)
    {
        // 游戏逻辑占位符
    }
}

// 6. 之前示例中提到的召唤效果
[Serializable]
public class SummonEffect : BaseEffect
{
    public string unitCardID; // 要召唤的卡牌ID
    public bool isDormant;    // 是否休眠

    public override void Execute(object invoker, List<object> targets)
    {
        // 游戏逻辑占位符
    }
}
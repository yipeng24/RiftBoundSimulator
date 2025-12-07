// EffectEnums.cs
public enum EffectTrigger
{
    OnPlay,             // 打出时
    OnAttack,           // 攻击时
    OnBlock,            // 防御时
    OnTurnStart,        // 己方回合开始
    OnTurnEnd,          // 己方回合结束
    OnTargetDestroyed,  // 目标被摧毁 (用于条件检查)
    OnMove,             // 移动时 (如亚索)
    Passive,            // 被动/常驻效果
    // ... 未来可扩展
}

public enum TargetType
{
    Unit,               // 任意单位
    HeroUnit,           // 英雄单位
    Spell,              // 法术
    Player,             // 玩家
    Self,               // 卡牌自身 (打出者)
    Targeted,           // 引用上一个效果的目标
    // ... 未来可扩展
}

public enum TargetLocation
{
    Battlefield,        // 战场
    Base,               // 基地
    Hand,               // 手牌
    Deck,               // 牌库
    DiscardPile,        // 废牌堆
    // ... 未来可扩展
}

public enum EffectActionType
{
    Damage,             // 造成伤害
    DrawCard,           // 抽牌
    Buff,               // 增益 (Power/Health)
    Move,               // 移动单位
    Stun,               // 眩晕
    Destroy,            // 摧毁单位
    Summon,             // 召唤单位
    GainRune,           // 获得符能
    // ... 未来可扩展
}
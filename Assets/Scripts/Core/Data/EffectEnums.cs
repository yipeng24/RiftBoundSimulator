namespace RiftBound.Core
{
    // 单位状态 (对应规则 592 休眠 / 593 活跃)
    public enum UnitState
    {
        Active,     // 活跃 (竖置，可行动)
        Resting,    // 休眠 (横置，已行动)
        Stunned     // 眩晕 (无法行动，跳过下一次唤醒)
    }

    // 效果触发时机
    public enum EffectTrigger
    {
        None = 0,
        OnPlay,             // 打出时
        OnAttack,           // 攻击时
        OnDefend,           // 防御时
        OnTurnStart,        // 回合开始
        OnTurnEnd,          // 回合结束
        OnDeath,            // 阵亡/遗愿 (绝念)
        OnDraw,             // 抽到时
        Passive             // 被动光环
    }

    // 效果行为类型
    public enum EffectActionType
    {
        None = 0,
        Damage,             // 造成伤害
        Heal,               // 治疗
        DrawCard,           // 抽牌
        BuffStats,          // 增加攻血
        AddKeyword,         // 增加关键词 (如强攻)
        SummonUnit,         // 召唤单位
        GainMana,           // 获得法力/符能
        Stun,               // 眩晕
        Recall              // 召回
    }

    // 目标类型
    public enum TargetType
    {
        None,
        Unit,
        Hero,
        Player,
        AllEnemies,
        AllAllies
    }

    // 目标位置
    public enum TargetLocation
    {
        Battlefield,
        Hand,
        Deck,
        Graveyard
    }
}
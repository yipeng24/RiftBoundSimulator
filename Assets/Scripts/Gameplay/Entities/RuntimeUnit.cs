using System;
using UnityEngine;
using RiftBound.Core; // 引用 UnitState

[Serializable]
public class RuntimeUnit
{
    public RuntimeCard SourceCard; // 对应的数据卡牌
    public UnitState State;        // 当前状态 (活跃/休眠)

    // 运行时属性
    public int CurrentHealth;
    public int CurrentPower;

    public RuntimeUnit(RuntimeCard card)
    {
        SourceCard = card;
        State = UnitState.Resting; // 进场通常是休眠 (除非有急速)

        // 这里应该从 CardData 初始化血量
        // CurrentHealth = CardDatabase.GetCardData(card.CardDataID).baseHealth;
    }

    public void SetState(UnitState newState)
    {
        State = newState;
    }

    public void ClearDamage()
    {
        // 规则 517.2.a: 回合结束清除伤害 (除非另有规则)
        // CurrentHealth = MaxHealth... 
        Debug.Log($"单位 {SourceCard.UniqueID} 清除伤害");
    }
}
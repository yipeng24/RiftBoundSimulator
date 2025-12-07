using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// 阶段基类
public abstract class BasePhase
{
    public abstract BattlePhaseType PhaseType { get; }
    public abstract IEnumerator Execute();
}

// 515.1 唤醒阶段
public class WakePhase : BasePhase
{
    public override BattlePhaseType PhaseType => BattlePhaseType.Wake;

    public override IEnumerator Execute()
    {
        var player = TurnManager.Instance.ActivePlayer;
        // 规则：将所有单位和神器置为“活跃”(Active)
        foreach (var unit in player.BoardUnits)
        {
            unit.SetState(UnitState.Active); // 重置休眠状态
            // 可以在这里播放单位“站起来”的动画
        }

        // 重置法力/符文池上限（如果有此规则）或刷新当回合资源
        yield break;
    }
}

// 515.2 开始阶段
public class StartPhase : BasePhase
{
    public override BattlePhaseType PhaseType => BattlePhaseType.Start;

    public override IEnumerator Execute()
    {
        // 1. 触发 "回合开始" (OnTurnStart) 效果
        // 例如：蒙多医生回复生命
        yield return EffectSystem.Instance.TriggerEvent(EffectTrigger.OnTurnStart, TurnManager.Instance.ActivePlayer);

        // 2. 据守得分计算 (515.2.b)
        ScoreManager.Instance.CalculateHoldScore(TurnManager.Instance.ActivePlayer);
    }
}

// 515.3 召出阶段
public class SummonPhase : BasePhase
{
    public override BattlePhaseType PhaseType => BattlePhaseType.Summon;

    public override IEnumerator Execute()
    {
        var player = TurnManager.Instance.ActivePlayer;

        // 规则：召出2张符文
        // 这里只是逻辑调用，具体要调用 DeckManager 从符文堆顶拿牌
        yield return player.SummonRunes(2);
    }
}

// 515.4 抽牌阶段
public class DrawPhase : BasePhase
{
    public override BattlePhaseType PhaseType => BattlePhaseType.Draw;

    public override IEnumerator Execute()
    {
        var player = TurnManager.Instance.ActivePlayer;

        // 规则：抽1张牌
        yield return player.DrawCard(1);

        // 规则 515.4.d: 抽牌阶段结束时，清空双方符文池 (Mana Burn)
        // 注意：如果你设计的是每回合重置法力，在这里清空
        player.CurrentRunes = 0;
        player.CurrentMana = 0; // 假设法力也重置，或者根据具体卡牌增加
    }
}

// 516 行动阶段 (最复杂)
public class ActionPhase : BasePhase
{
    public override BattlePhaseType PhaseType => BattlePhaseType.Action;

    // 标志位：双方是否连续让过
    private bool _consecutivePasses = false;

    public override IEnumerator Execute()
    {
        Debug.Log("进入行动阶段 - Open Loop");

        _consecutivePasses = false;

        // 只要没有连续让过，就一直循环
        while (!_consecutivePasses)
        {
            // 1. 等待拥有优先权的玩家操作
            // 这里会挂起协程，直到收到 "PlayerAction" 或 "Pass" 信号
            yield return WaitForPriorityPlayerAction();
        }

        Debug.Log("行动阶段结束");
    }

    private IEnumerator WaitForPriorityPlayerAction()
    {
        var priorityPlayer = TurnManager.Instance.PriorityPlayer;

        // 告诉 UI：现在是谁的回合，等待输入
        // 我们使用 TaskCompletionSource 或简单的 bool 标志来等待
        bool actionReceived = false;
        bool isPass = false;

        // 订阅 UI 事件
        void OnAction() { actionReceived = true; isPass = false; }
        void OnPass() { actionReceived = true; isPass = true; }

        BattleInteractionManager.Instance.OnPlayerAction += OnAction;
        BattleInteractionManager.Instance.OnPlayerPass += OnPass;

        // 挂起，等待信号
        while (!actionReceived)
        {
            yield return null;
        }

        // 取消订阅
        BattleInteractionManager.Instance.OnPlayerAction -= OnAction;
        BattleInteractionManager.Instance.OnPlayerPass -= OnPass;

        // 处理结果
        if (isPass)
        {
            Debug.Log($"{priorityPlayer.PlayerName} 让过");

            // 结算链是否为空？
            if (ResolutionChain.Instance.IsEmpty)
            {
                // 如果栈为空且是回合玩家让过 -> 还没结束，看对手
                // 如果对手也让过 -> 阶段结束
                if (TurnManager.Instance.PriorityPlayer != TurnManager.Instance.ActivePlayer)
                {
                    // 对手让过，且栈为空 -> 回到 ActivePlayer
                    TurnManager.Instance.SetPriority(TurnManager.Instance.ActivePlayer);
                }
                else
                {
                    // ActivePlayer 让过，且此前对手也让过了（或是回合第一动）
                    // 简化逻辑：如果栈空，ActivePlayer让过，通常意味着回合结束请求
                    // 但正式规则可能是双方都 Pass 才能进下一阶段
                    // 这里我们假设：对手Pass交还优先权，回合玩家再Pass则结束阶段
                    _consecutivePasses = true;
                }
            }
            else
            {
                // 栈不为空，让过意味着“不响应”，执行栈顶效果
                yield return ResolutionChain.Instance.ResolveNext();
            }
        }
        else
        {
            // 玩家采取了行动（打牌/移动），重置连续让过标记
            _consecutivePasses = false;

            // 行动本身（Command）已经执行了入栈操作
            // 如果栈内有东西，优先权通常会转移给对方进行响应（进入闭环）
            TurnManager.Instance.PassPriority();
        }
    }
}

// 517 结束阶段
public class EndPhase : BasePhase
{
    public override BattlePhaseType PhaseType => BattlePhaseType.End;

    public override IEnumerator Execute()
    {
        // 517.2 失效步骤：移除 "本回合" (This Turn) 的临时效果
        EffectSystem.Instance.ClearTemporaryEffects();

        // 517.2.a 清除所有单位的伤害 (规则书提到伤害会清除)
        foreach (var unit in UnitManager.Instance.AllUnits)
        {
            unit.ClearDamage();
        }

        // 触发 "回合结束" 效果
        yield return EffectSystem.Instance.TriggerEvent(EffectTrigger.OnTurnEnd, TurnManager.Instance.ActivePlayer);
    }
}
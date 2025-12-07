using UnityEngine;
using System.Collections;
using System;

// 游戏阶段枚举，对应规则书 515-517
public enum BattlePhaseType
{
    None,
    Wake,       // 唤醒 (515.1)
    Start,      // 开始 (515.2)
    Summon,     // 召出 (515.3)
    Draw,       // 抽牌 (515.4)
    Action,     // 行动 (516) - 最核心
    End         // 结束 (517)
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public BattlePhaseType CurrentPhaseType { get; private set; }
    private BasePhase _currentPhaseState;

    // 事件：通知UI更新
    public event Action<BattlePhaseType> OnPhaseChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 假设手牌调度已结束，由这里开始第一回合
        StartCoroutine(StartNewTurnRoutine());
    }

    // 开启新回合流程
    public IEnumerator StartNewTurnRoutine()
    {
        // 1. 切换回合拥有者
        TurnManager.Instance.StartNewTurn();
        Debug.Log($"--- 新回合开始: {TurnManager.Instance.ActivePlayer.PlayerName} ---");

        // 2. 按顺序执行阶段
        yield return RunPhase(new WakePhase());
        yield return RunPhase(new StartPhase());
        yield return RunPhase(new SummonPhase());
        yield return RunPhase(new DrawPhase());

        // 行动阶段通常持续很久，直到玩家让过
        yield return RunPhase(new ActionPhase());

        yield return RunPhase(new EndPhase());

        // 3. 递归开始下一回合
        StartCoroutine(StartNewTurnRoutine());
    }

    private IEnumerator RunPhase(BasePhase phase)
    {
        _currentPhaseState = phase;
        CurrentPhaseType = phase.PhaseType;
        OnPhaseChanged?.Invoke(CurrentPhaseType);

        Debug.Log($"进入阶段: {CurrentPhaseType}");

        // 执行阶段逻辑
        yield return phase.Execute();

        // 稍微等待，给UI动画留出缓冲
        yield return new WaitForSeconds(0.5f);

        _currentPhaseState = null;
    }
}
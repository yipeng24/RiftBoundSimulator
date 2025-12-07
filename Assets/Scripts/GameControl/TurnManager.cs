using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public RuntimePlayer Player1;
    public RuntimePlayer Player2;

    public RuntimePlayer ActivePlayer { get; private set; }   // 当前回合玩家
    public RuntimePlayer PriorityPlayer { get; private set; } // 当前拥有优先权的玩家

    public int RoundCount { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
        // 初始化玩家数据（测试用）
        Player1 = new RuntimePlayer("Player 1", true);
        Player2 = new RuntimePlayer("Player 2", false);
    }

    public void StartNewTurn()
    {
        RoundCount++;

        // 简单的轮换逻辑
        if (ActivePlayer == null)
        {
            ActivePlayer = Player1; // 默认P1先手
        }
        else
        {
            ActivePlayer = (ActivePlayer == Player1) ? Player2 : Player1;
        }

        // 回合开始时，回合玩家获得优先权
        SetPriority(ActivePlayer);
    }

    public void SetPriority(RuntimePlayer player)
    {
        PriorityPlayer = player;
        Debug.Log($"优先权移交至: {player.PlayerName}");

        // 通知UI：如果是本地玩家获得优先权，解锁操作
        BattleInteractionManager.Instance.OnPriorityChanged(player.IsLocalPlayer);
    }

    public void PassPriority()
    {
        // 将优先权交给对方
        SetPriority(GetOpponent(PriorityPlayer));
    }

    public RuntimePlayer GetOpponent(RuntimePlayer player)
    {
        return player == Player1 ? Player2 : Player1;
    }
}
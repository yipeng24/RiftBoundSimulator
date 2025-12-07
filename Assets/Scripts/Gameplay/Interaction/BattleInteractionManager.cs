using System;
using UnityEngine;

public class BattleInteractionManager : MonoBehaviour
{
    public static BattleInteractionManager Instance { get; private set; }

    // 事件：通知 ActionPhase 玩家做了决定
    public event Action OnPlayerAction; // 玩家打牌或移动了
    public event Action OnPlayerPass;   // 玩家点击了让过

    private bool _isMyTurnPriority;

    private void Awake()
    {
        Instance = this;
    }

    // 由 TurnManager 调用，更新当前能否操作
    public void OnPriorityChanged(bool isLocalPlayerPriority)
    {
        _isMyTurnPriority = isLocalPlayerPriority;
        // 这里可以更新 UI 按钮状态 (比如亮起让过按钮)
        Debug.Log($"[Interaction] 操作权限更新: {isLocalPlayerPriority}");
    }

    // UI 按钮绑定：点击“让过”
    public void OnPassButtonClicked()
    {
        if (!_isMyTurnPriority) return;

        Debug.Log("[Interaction] 玩家点击让过");
        OnPlayerPass?.Invoke();
    }

    // 模拟玩家执行了某种操作（比如打出卡牌后调用）
    public void NotifyPlayerAction()
    {
        if (!_isMyTurnPriority) return;

        Debug.Log("[Interaction] 玩家执行了行动");
        OnPlayerAction?.Invoke();
    }
}
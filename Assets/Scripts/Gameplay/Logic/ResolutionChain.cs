using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiftBound.Core;
public class ResolutionChain : MonoBehaviour
{
    public static ResolutionChain Instance { get; private set; }

    // 栈结构存储待结算的指令
    private Stack<GameCommand> _stack = new Stack<GameCommand>();

    public bool IsEmpty => _stack.Count == 0;

    private void Awake()
    {
        Instance = this;
    }

    // 将指令推入结算链
    public void PushCommand(GameCommand cmd)
    {
        _stack.Push(cmd);
        Debug.Log($"指令入栈: {cmd.GetType().Name}. 当前栈深度: {_stack.Count}");

        // 通知 UI：进入闭环状态，显示堆叠
        //BattleUI.Instance.UpdateChainDisplay(_stack);
    }

    // 结算栈顶指令
    public IEnumerator ResolveNext()
    {
        if (_stack.Count > 0)
        {
            var cmd = _stack.Pop();
            Debug.Log($"结算指令: {cmd.GetType().Name}");

            // 执行指令的具体逻辑 (造成伤害、召唤单位等)
            cmd.ExecuteResolution();

            // 播放动画并等待
            yield return new WaitForSeconds(1.0f); // 模拟动画时间

            //BattleUI.Instance.UpdateChainDisplay(_stack);
        }
    }
}
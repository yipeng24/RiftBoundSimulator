using UnityEngine;

// 这是一个抽象类，具体的逻辑（如打出卡牌、造成伤害）需要继承它
public abstract class GameCommand
{
    // 执行指令的核心逻辑 (修改数据)
    public abstract void ExecuteResolution();
}

// 示例：一个空的占位指令，防止报错
public class DebugCommand : GameCommand
{
    private string _message;
    public DebugCommand(string msg) { _message = msg; }
    public override void ExecuteResolution()
    {
        Debug.Log($"[Command Resolved] {_message}");
    }
}
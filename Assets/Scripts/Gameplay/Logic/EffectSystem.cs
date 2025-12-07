using System.Collections;
using UnityEngine;
using RiftBound.Core; // 引用枚举

public class EffectSystem : MonoBehaviour
{
    public static EffectSystem Instance { get; private set; }

    private void Awake() { Instance = this; }

    // 触发全局事件
    public IEnumerator TriggerEvent(EffectTrigger trigger, RuntimePlayer player)
    {
        Debug.Log($"[EffectSystem] 触发事件: {trigger} (Player: {player.PlayerName})");

        // TODO: 遍历场上所有卡牌，检查是否有匹配 trigger 的效果
        // 如果有，创建 Command 并压入 ResolutionChain

        yield return null;
    }

    // 清除本回合临时效果 (如 "本回合+2攻击")
    public void ClearTemporaryEffects()
    {
        Debug.Log("[EffectSystem] 清除临时效果...");
    }
}
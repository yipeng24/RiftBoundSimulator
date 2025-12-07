using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private void Awake() { Instance = this; }

    // 计算据守得分 (Rule 629)
    public void CalculateHoldScore(RuntimePlayer player)
    {
        // TODO: 遍历战场，检查控制权
        Debug.Log($"[ScoreManager] 计算 {player.PlayerName} 的据守得分...");
    }
}
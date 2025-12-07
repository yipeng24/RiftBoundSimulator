// RuntimeCard.cs
using System;

[Serializable]
public class RuntimeCard
{
    // 运行时唯一ID (例如: 1, 2, 3...)
    // 用于逻辑层区分每一张具体的卡
    public int UniqueID;

    // 静态数据ID (例如: "OGN_005")
    // 用于查询 CardDatabase 获取原画、费用、效果文本等
    public string CardDataID;

    // 你可以在这里扩展更多运行时状态
    // public int CurrentHealth;
    // public int CurrentPower;
    // public bool IsStunned;

    public RuntimeCard(int uniqueID, string cardDataID)
    {
        UniqueID = uniqueID;
        CardDataID = cardDataID;
    }
}
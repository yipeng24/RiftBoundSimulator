using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimePlayer
{
    public string PlayerName;
    public bool IsLocalPlayer;

    // 资源
    public int CurrentMana;
    public int CurrentRunes;

    // 区域
    public List<RuntimeCard> Hand = new List<RuntimeCard>();
    public List<RuntimeUnit> BoardUnits = new List<RuntimeUnit>();
    public List<string> RuneDeck = new List<string>(); // ID列表

    public RuntimePlayer(string name, bool isLocal)
    {
        PlayerName = name;
        IsLocalPlayer = isLocal;
    }

    public IEnumerator SummonRunes(int count)
    {
        Debug.Log($"{PlayerName} 正在召出 {count} 个符文...");
        // 实现从 RuneDeck 移动到 Field/Pool 的逻辑
        CurrentRunes += count; // 简化实现
        yield return null;
    }

    public IEnumerator DrawCard(int count)
    {
        Debug.Log($"{PlayerName} 正在抽 {count} 张牌...");
        // 实现从 Deck 移动到 Hand 的逻辑
        yield return null;
    }
}
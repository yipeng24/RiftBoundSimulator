//////////////////////////////////
/// Version: 1.0
/// function: 枚举卡牌的基本属性
/// Author: francle
/////////////////////////////////// 
using System.Collections.Generic;
using UnityEngine;

// 打出卡牌的符文消耗
public class CardRuneCost
{
    public List<RunePropertyType> acceptableTypes = new List<RunePropertyType>();
    public int amount;
    public CardRuneCost(params RunePropertyType[] types)
    {
        acceptableTypes.AddRange(types);
    }
}
public class CardBaseInfo
{
    public string cardID;       // 唯一标识符
    public string displayName;  // 显示名称 
    public CardType cardType;       // 卡牌类型 
    public List<RunePropertyType> runeProperties; // 符文特性 
    public List<string> tags;          // 标签 (用于卡牌分类和效果触发)
    public string artName;      // 美术资源名称 (用于加载对应的美术资源) 

    public int manaCost;          // 卡牌的法力消耗
    public CardRuneCost runeCost; // 卡牌的符文消耗
    public int basePower;         // 基础攻击力 (仅对单位和英雄单位有效)

    public string ruleText;       // 规则文本 (描述卡牌效果和使用条件)
    public string flavorText;     // 旁白文本 (提供卡牌背景故事或趣味信息)
}


using System;
using System.Collections.Generic;

// 枚举定义 (与规则手册对应)
public enum CardType { Unit, HeroUnit, Spell, Equipment, Rune, Battlefield, Legend }
// RuneType 对应六大符文特性
public enum RuneType { None = 0, Fervor, Verdant, Brilliant, Shatter, Chaos, Order } // 炽烈、翠意、灵光、摧破、混沌、序理
// KeywordType 对应规则手册中的关键词
public enum KeywordType { Quick, Reactive, Aggressive, Defensive, SpellShield, Mobile, Momentary, Foresight, Afterthought, Other }
// 迅捷, 反应, 强攻, 坚守, 法盾, 游走, 瞬息, 预知, 绝念, 其他/通用

[Serializable]
public class CardData
{
    // 基础信息
    public string cardID;
    public string displayName;
    public CardType type;
    public string artName;

    // 费用与战力 (原始数值)
    public int manaCost;        // 法力费用
    public List<RuneType> runeCost; // 符能费用（打出费用，并非特性）
    public int basePower;       // 基础战力 (Unit)
    public int baseHealth;      // 基础生命/耐久 (Unit/Equipment)

    // 特性和标签
    public List<RuneType> runes; // 卡牌特性 (用于卡组构筑限制)
    public List<string> tags;    // 标签 (例如：机械、城邦、约德尔人)

    // 规则文本
    // 关键优化：结构化存储，方便程序逻辑判断
    public List<KeywordType> keywords; // 关键词列表
    public string ruleText;       // 详细规则文本 (非关键词部分)
    public string flavorText;
}

// ====================================================================
// 运行时状态类（UnitCard/EquipmentCard 等）
// 运行时状态（如当前伤害、是否休眠）将存储在这些类中，而不是 CardData。
// 您可以将这些类放在单独的文件中，如 UnitCard.cs
// ====================================================================
// public class UnitCard : MonoBehaviour 
// {
//     public CardData baseData;    
//     public bool isDormant;      
//     // ... 其他运行时状态
// }
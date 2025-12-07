using System;
using System.Collections.Generic;

[Serializable]
public class DeckData
{
    public string deckName;          // 卡组名字，例如 "火焰快攻"
    public List<string> cardIDs;     // 只存 cardID 列表，比如 ["OGN_005", "OGN_012", ...]

    public DeckData(string name)
    {
        deckName = name;
        cardIDs = new List<string>();
    }
}

[Serializable]
public class DeckSaveFile
{
    public string currentDeckName;       // 当前使用的卡组名
    public List<DeckData> decks = new List<DeckData>();
}



//////////////////////////////////////////////////
/// 相当于卡牌数据库的单例类，负责加载和提供卡牌基础数据。
/// 用于对外提供卡牌基础数据的访问接口。
//////////////////////////////////////////////////
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

public class CardInstanceHelper
{
    private Dictionary<string, CardBaseInfo> _database;
    public ReadOnlyDictionary<string, CardBaseInfo> Database { get; private set; }
    // 美术资源缓存
    private static Dictionary<string, Sprite> artCache = new Dictionary<string, Sprite>();
    // Todo: 需要优化
    private const string DEFAULT_RESOURCE_PATH = "Cards/Data/RiftboundCardList";
    private const string DEFAULT_ART_PATH = "Cards/Arts/";
    public void InitializeDatabase()
    {
        if (Database != null) return;
        TextAsset csv = Resources.Load<TextAsset>(DEFAULT_RESOURCE_PATH);
        if (csv != null)
        {
            _database = CardDB_Loader.LoadAllCards(csv.text);
            Database = new ReadOnlyDictionary<string, CardBaseInfo>(_database);
        }
        else
        {
            Debug.LogError($"[CardDatabase] 致命错误：无法在路径 'Resources/{DEFAULT_RESOURCE_PATH}' 找到卡牌数据文件！请检查文件位置。");
        }
    }

    public CardBaseInfo GetCardBaseData(string id)
    {
        if (_database != null &&_database.TryGetValue(id, out CardBaseInfo card))
        {
            return card;
        }

        Debug.LogError($"[CardDatabase] 找不到 CardID: {id}");
        return null;
    }
    public static Sprite GetArt(string artName)
    {
        if (string.IsNullOrEmpty(artName)) return null;

        if (artCache.TryGetValue(artName, out var sp))
            return sp;
        
        Sprite loadedSprite = Resources.Load<Sprite>(Path.Combine(DEFAULT_ART_PATH,artName));
        if (loadedSprite != null)
        {
            artCache.Add(artName, loadedSprite);
        }
        else
        {
            Debug.LogWarning($"[CardDatabase] 找不到卡图资源: {Path.Combine(DEFAULT_ART_PATH,artName)}");
        }

        return loadedSprite;
    }
}
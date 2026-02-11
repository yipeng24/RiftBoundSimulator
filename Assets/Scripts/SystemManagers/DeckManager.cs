using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Unity;
using UnityEngine;

class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }
    public System.Action OnDeckUpdated;
    private string _currentDeckName;
    public IReadOnlyList<string> UsrDecks;
    public DeckInstanceHelper _DeckInstaceHelper { get; private set; }
    public DeckDB_Helper _DeckDB_Helper { get; private set; }
    //private static bool _deckModified = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this._DeckDB_Helper = new DeckDB_Helper();
            UsrDecks = _DeckDB_Helper.GetUsrDeckList().AsReadOnly();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void LoadDeckInstance(string Name)
    {
        if (_DeckInstaceHelper != null)
        {
            _DeckInstaceHelper.SaveDeckWithoutValidate();
            _DeckInstaceHelper.Dispose();        
        }
        _currentDeckName = Name;
        var cardDB = CardManager.Instance.CardInstanceHelper;
        _DeckInstaceHelper = new DeckInstanceHelper(Name, cardDB);

        Debug.Log($"[DeckManager] 已成功加载卡组实例: {Name}");
        OnDeckUpdated?.Invoke();
    }
    public void AddCard2Deck(string cardID)
    {
        _DeckInstaceHelper.AddCardToDeck(cardID);
        OnDeckUpdated?.Invoke();
    }
    public void RemoveCard4Deck(string cardID)
    {
        _DeckInstaceHelper.RemoveCardFromDeck(cardID);
        OnDeckUpdated?.Invoke();
    }
    public void reSortDeckCards(List<string> newOrderIDs)
    {
        _DeckInstaceHelper?.UpdateDeckSort(newOrderIDs);
        OnDeckUpdated?.Invoke();
    }
    public void CreateNewDeck(string Name)
    {
        _DeckDB_Helper?.CreateDeck(Name);
        LoadDeckInstance(Name);
        UsrDecks = _DeckDB_Helper.GetUsrDeckList().AsReadOnly();
    }
    public void DeleteDeck()
    {
        _DeckDB_Helper?.DeleteDeck(_currentDeckName);
        UsrDecks = _DeckDB_Helper.GetUsrDeckList().AsReadOnly();
    }
    public bool SaveDeck(out StringBuilder errorBuilder)
    {
        return  _DeckInstaceHelper.ValidateDeck(out errorBuilder);
    }
    // 传递当前卡组的卡 ID 列表
    public IReadOnlyList<string> GetCurrentDeckCardIDs()
    {
        // 增加空检查，防止 Helper 还没创建时 UI 就去拿数据
        if (_DeckInstaceHelper == null)
        {
            Debug.LogWarning(" 卡组尚未初始化，返回空列表");
            return new List<string>().AsReadOnly();
        }
        return _DeckInstaceHelper.CardIDs;
    }

}
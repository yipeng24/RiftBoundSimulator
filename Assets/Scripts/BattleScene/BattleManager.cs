// BattleManager.cs
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Data Resources")]
    [SerializeField] private TextAsset cardCsvAsset;

    // [新增] ID 生成器
    private int nextUniqueID = 1000;

    // [修改] 数据结构改为 RuntimeCard
    private RuntimeCard legendCard;
    private RuntimeCard heroUnit;
    private RuntimeCard selectedBattlefield;

    private List<RuntimeCard> mainDeck = new List<RuntimeCard>();
    private List<RuntimeCard> runeDeck = new List<RuntimeCard>();
    private List<RuntimeCard> hand = new List<RuntimeCard>();

    public bool IsMulliganPhase { get; private set; }
    // 流程控制
    public bool IsPlayerTurn { get; private set; }
    private bool isWaitingForMulliganInput = false; // 是否正在等待玩家点击调度按钮

    private const string DECK_SAVE_FILENAME = "current_deck.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 1. 核心防御：在开始游戏前，确保 CardDatabase 已经加载了数据
        EnsureCardDatabaseLoaded();

        // 2. 开始游戏设置
        SetupGame();
        StartCoroutine(StartGameSequence());
    }

    /// <summary>
    /// 检查静态卡牌数据库是否为空，如果为空（例如直接运行此场景或已被释放），则重新加载。
    /// </summary>
    private void EnsureCardDatabaseLoaded()
    {
        var allCards = CardDatabase.GetAllCardData();
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogWarning("[BattleManager] CardDatabase 为空 (可能是首次进入或已被释放)。正在重新加载...");

            if (cardCsvAsset != null)
            {
                CardDatabase.LoadAllCards(cardCsvAsset.text);
            }
            else
            {
                // 尝试从 Resources 加载作为后备方案 (路径参考自之前的工程代码)
                TextAsset csv = Resources.Load<TextAsset>("Cards/Data/RiftboundCardList");
                if (csv != null)
                {
                    CardDatabase.LoadAllCards(csv.text);
                }
                else
                {
                    Debug.LogError("[BattleManager] 严重错误：无法加载 CardDatabase！Inspector 中未赋值 CSV，且 Resources 路径下未找到。");
                }
            }
        }
    }

    // [新增] 辅助方法：生成带唯一ID的卡牌实例
    private RuntimeCard CreateRuntimeCard(string dataID)
    {
        if (string.IsNullOrEmpty(dataID)) return null;
        return new RuntimeCard(nextUniqueID++, dataID);
    }

    /// <summary>
    /// 直接从 JSON 文件加载卡组，并将卡牌分发到各自的起始区域。
    /// </summary>
    private void SetupGame()
    {
        List<string> loadedDeckIDs = LoadDeckFromSaveFile();
        if (loadedDeckIDs == null || loadedDeckIDs.Count < 56) return; // 简单校验

        // --- 1. 分离并实例化 Legend ---
        string legendID = loadedDeckIDs[0];
        legendCard = CreateRuntimeCard(legendID);

        // --- 2. 分离并实例化 Runes ---
        runeDeck.Clear();
        foreach (var id in loadedDeckIDs.GetRange(1, 12))
        {
            runeDeck.Add(CreateRuntimeCard(id));
        }

        // --- 3. 剩余卡牌处理 ---
        List<string> remainingIDs = loadedDeckIDs.Skip(13).ToList();

        // 提取并随机选择战场
        List<string> battlefieldIDs = remainingIDs
            .Where(id => CardDatabase.GetCardData(id)?.type == CardType.Battlefield)
            .ToList();
        string bfID = battlefieldIDs[Random.Range(0, battlefieldIDs.Count)];
        selectedBattlefield = CreateRuntimeCard(bfID);

        // --- 4. 英雄与主牌堆 ---
        CardDataDefinition legendData = CardDatabase.GetCardData(legendID);
        string displayHeroID = remainingIDs.FirstOrDefault(id => {
            CardDataDefinition d = CardDatabase.GetCardData(id);
            return d != null && d.type == CardType.HeroUnit &&
                   (legendData != null && legendData.tags.Contains(d.displayName));
        });

        // 实例化英雄区域卡牌
        heroUnit = CreateRuntimeCard(displayHeroID);

        // 实例化主牌堆
        mainDeck.Clear();
        int totalHeroCopies = remainingIDs.Count(id => id == displayHeroID);
        int copiesInMainDeck = totalHeroCopies - 1;
        int currentHeroCopiesAdded = 0;

        foreach (string id in remainingIDs)
        {
            CardDataDefinition data = CardDatabase.GetCardData(id);
            if (data == null || data.type == CardType.Battlefield) continue;

            if (id == displayHeroID)
            {
                if (currentHeroCopiesAdded < copiesInMainDeck)
                {
                    mainDeck.Add(CreateRuntimeCard(id)); // 实例化
                    currentHeroCopiesAdded++;
                }
            }
            else
            {
                mainDeck.Add(CreateRuntimeCard(id)); // 实例化
            }
        }

        // 洗牌
        ShuffleList(mainDeck);
        ShuffleList(runeDeck);

        // 渲染
        if (BattleUI.Instance != null) BattleUI.Instance.SetupBoardVisuals();
    }

    private List<string> LoadDeckFromSaveFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, DECK_SAVE_FILENAME);
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                SavedDeckData wrapper = JsonUtility.FromJson<SavedDeckData>(json);
                return wrapper?.cardIDs;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading deck file: {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.LogError($"Deck file not found at: {filePath}. Please build a deck first.");
            return null;
        }
    }
    // [修改] 抽卡逻辑使用 RuntimeCard
    public void DrawCard()
    {
        if (mainDeck.Count > 0)
        {
            RuntimeCard card = mainDeck[0];
            mainDeck.RemoveAt(0);
            hand.Add(card);

            if (BattleUI.Instance != null)
            {
                BattleUI.Instance.DrawCardToHand(card);
            }
        }
        else
        {
            Debug.LogWarning("牌堆已空，无法抽牌！(应触发燃尽逻辑)");
        }
    }
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    // ==================== 流程控制核心 ====================

    private IEnumerator StartGameSequence()
    {
        Debug.Log(">>> 游戏准备阶段开始 <<<");
        yield return new WaitForSeconds(0.5f);

        // 1. 决定先后手
        IsPlayerTurn = Random.value > 0.5f;
        Debug.Log($"[流程] 决定先手: {(IsPlayerTurn ? "玩家" : "对手")}");
        // TODO: 通知 UI 显示先后手提示

        yield return new WaitForSeconds(0.5f);

        // 2. 确认/翻出场地
        // (在 SetupGame 里已经随机选好了 selectedBattlefield)
        Debug.Log($"[流程] 随机场地确认为: {CardDatabase.GetCardData(selectedBattlefield.CardDataID)?.displayName}");
        // 这里可以加一个 UI 动画高亮一下场地

        yield return new WaitForSeconds(0.5f);

        // 3. 初始抽牌 (4张)
        Debug.Log("[流程] 开始抽 4 张起始手牌...");
        for (int i = 0; i < 4; i++)
        {
            DrawCard();
            yield return new WaitForSeconds(0.2f); // 抽卡间隔动画
        }

        yield return new WaitForSeconds(0.5f);

        // 4. 开启调度 (Mulligan)
        Debug.Log("[流程] 进入调度阶段，等待玩家选择...");
        IsMulliganPhase = true; // [核心] 开启交互
        isWaitingForMulliganInput = true;

        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.ShowMulliganPanel(true);
        }

        // 暂停协程，死循环等待，直到 OnMulliganChoiceReceived 修改状态
        while (isWaitingForMulliganInput)
        {
            yield return null;
        }
        IsMulliganPhase = false; // [核心] 关闭交互
        Debug.Log(">>> 准备阶段结束，游戏正式开始 <<<");
        // StartTurn(); // 正式进入第一回合
    }

    /// <summary>
    /// [修改] 接收具体的卡牌列表
    /// </summary>
    public void OnMulliganConfirmed(List<RuntimeCard> cardsToReplace)
    {
        if (!isWaitingForMulliganInput) return;

        int count = cardsToReplace.Count;
        if (count == 0)
        {
            Debug.Log("[调度] 玩家选择不调度 (保留全部)。");
        }
        else
        {
            Debug.Log($"[调度] 玩家选择替换 {count} 张牌。");
            ProcessMulligan(cardsToReplace);
        }

        // 结束等待
        isWaitingForMulliganInput = false;
    }
    /// <summary>
    /// 执行具体的换牌逻辑：移除手牌 -> 抽新牌 -> 旧牌洗回牌堆
    /// </summary>
    /// <summary>
    /// [修改] 执行换牌逻辑
    /// </summary>
    private void ProcessMulligan(List<RuntimeCard> cardsToReplace)
    {
        // 1. UI: 先把它们移回牌堆 (视觉上)
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.RemoveCardsFromHandToDeck(cardsToReplace);
        }

        // 2. Data: 从手牌移除，添加到待洗入列表
        foreach (var card in cardsToReplace)
        {
            // 通过 UniqueID 在手牌列表中找到对应的数据引用并移除
            // 注意：因为 cardsToReplace 是 UI 层传来的引用，和 hand 里的引用是同一个对象，但也可能是通过ID查找的
            // 最稳妥是用 UniqueID 查找
            var cardInHand = hand.FirstOrDefault(c => c.UniqueID == card.UniqueID);
            if (cardInHand != null)
            {
                hand.Remove(cardInHand);
            }
        }

        // 3. 抽相同数量的新牌
        // 注意：规则上通常是先把牌洗回去再抽，还是先抽再洗？
        // 炉石是：先把你选的牌放到一边 -> 抽新牌 -> 把旧牌洗入牌库 (这样你不会抽到刚才扔掉的牌)
        // 我们这里按炉石逻辑：
        int drawCount = cardsToReplace.Count;
        for (int i = 0; i < drawCount; i++)
        {
            DrawCard();
        }

        // 4. 将旧牌加回主牌堆
        foreach (var card in cardsToReplace)
        {
            mainDeck.Add(card);
        }

        // 5. 洗牌
        ShuffleList(mainDeck);
        Debug.Log("[调度] 完成，牌堆已重洗。");
    }
    // [修改] 访问器返回 RuntimeCard
    public RuntimeCard GetLegendCard() => legendCard;
    public RuntimeCard GetHeroUnit() => heroUnit;
    public RuntimeCard GetSelectedBattlefield() => selectedBattlefield;
    public IReadOnlyList<RuntimeCard> GetShuffledMainDeck() => mainDeck.AsReadOnly();
    public IReadOnlyList<RuntimeCard> GetShuffledRuneDeck() => runeDeck.AsReadOnly();
}

[System.Serializable]
public class SavedDeckData { public List<string> cardIDs; }
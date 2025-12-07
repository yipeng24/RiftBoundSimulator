using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance { get; private set; }

    [Header("Card Prefab")]
    public GameObject battleCardPrefab;

    [Header("Player A Containers")]
    public Transform panelLegend;
    public Transform panelHero;
    public Transform panelMainDeck;
    public Transform panelRune;
    public Transform panelHand;
    public Transform panelBase;

    [Header("Fight Area Containers")]
    public Transform panelStandingArena1;

    [Header("Mulligan UI")] // [新增] 调度相关 UI
    public GameObject panelMulligan; // 调度面板的父物体
    public Button btnConfirmMulligan; // [修改] 只保留一个确认按钮
    public TMP_Text mulliganTipText;  // [可选] 提示文字：“请选择要更换的卡牌”

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 绑定确认按钮
        if (btnConfirmMulligan)
            btnConfirmMulligan.onClick.AddListener(OnConfirmMulliganClicked);

        if (panelMulligan) panelMulligan.SetActive(false);

    }

    // [修改] 点击确认按钮，收集所有被选中的卡牌
    // [修改] 点击确认按钮，收集所有被选中的卡牌
    private void OnConfirmMulliganClicked()
    {
        List<RuntimeCard> cardsToReplace = new List<RuntimeCard>();

        // 遍历手牌区所有 BattleCardView
        foreach (Transform child in panelHand)
        {
            var view = child.GetComponent<BattleCardView>();
            if (view != null && view.IsSelectedForMulligan)
            {
                cardsToReplace.Add(view.RuntimeData);
            }
        }

        // 发送给 Manager
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnMulliganConfirmed(cardsToReplace);
        }

        // 隐藏面板
        if (panelMulligan) panelMulligan.SetActive(false);
    }

    public void ShowMulliganPanel(bool show)
    {
        if (panelMulligan) panelMulligan.SetActive(show);
        // 这里可以重置提示文字
        if (show && mulliganTipText) mulliganTipText.text = "点击卡牌进行替换，再次点击取消";
    }


    public void SetupBoardVisuals()
    {
        if (BattleManager.Instance == null) return;

        ClearContainer(panelLegend);
        ClearContainer(panelHero);
        ClearContainer(panelStandingArena1);
        ClearContainer(panelRune);
        ClearContainer(panelMainDeck);
        ClearContainer(panelHand);

        // [修改] 所有获取方法现在返回 RuntimeCard 或 List<RuntimeCard>

        CreateCard(BattleManager.Instance.GetLegendCard(), panelLegend, true);
        CreateCard(BattleManager.Instance.GetHeroUnit(), panelHero, true);
        CreateCard(BattleManager.Instance.GetSelectedBattlefield(), panelStandingArena1, true);

        foreach (var card in BattleManager.Instance.GetShuffledRuneDeck())
        {
            CreateCard(card, panelRune, false);
        }

        foreach (var card in BattleManager.Instance.GetShuffledMainDeck())
        {
            CreateCard(card, panelMainDeck, false);
        }
    }

    // [修改] 抽卡动画：根据 UniqueID 精确查找
    public void DrawCardToHand(RuntimeCard targetCard)
    {
        BattleCardView targetView = null;

        // 在牌堆里找 UniqueID 匹配的那个视图
        foreach (Transform child in panelMainDeck)
        {
            var view = child.GetComponent<BattleCardView>();
            if (view != null && view.RuntimeData.UniqueID == targetCard.UniqueID)
            {
                targetView = view;
                break;
            }
        }

        if (targetView != null)
        {
            // 1. 移动父级到手牌区
            targetView.transform.SetParent(panelHand, false);

            // [新增] 2. 强制重置变换信息，防止卡牌歪斜或缩放不对
            // 因为从牌堆（可能叠在一起）移动到 LayoutGroup（自动布局），需要归零
            targetView.transform.localPosition = Vector3.zero;
            targetView.transform.localRotation = Quaternion.identity; // 修正旋转，确保它是正的
            targetView.transform.localScale = Vector3.one;            // 修正缩放

            // 3. 翻面为正面
            targetView.SetFaceUp(true);

            Debug.Log($"[BattleUI] Drew card: {targetCard.CardDataID} (UID: {targetCard.UniqueID})");
        }
        else
        {
            Debug.LogError($"[BattleUI] Cannot find view for UID: {targetCard.UniqueID}");
        }
    }
    // [新增] 移除手牌并洗回牌堆的视觉处理
    // [修改] 视觉上移除手牌并洗回牌堆
    public void RemoveCardsFromHandToDeck(List<RuntimeCard> cards)
    {
        foreach (var cardData in cards)
        {
            // 在手牌区找视图
            BattleCardView targetView = null;
            foreach (Transform child in panelHand)
            {
                var view = child.GetComponent<BattleCardView>();
                if (view != null && view.RuntimeData.UniqueID == cardData.UniqueID)
                {
                    targetView = view;
                    break;
                }
            }

            if (targetView != null)
            {
                targetView.SetFaceUp(false); // 翻背面
                targetView.transform.SetParent(panelMainDeck, false); // 移回牌堆父物体
                targetView.transform.SetAsLastSibling(); // 放到最下面

                // [重要] 重置选中状态，防止下次用这个View时颜色不对
                // 虽然 BattleCardView.Init 会重置，但这里只是移动，不会重新 Init
                // 最好手动重置一下 visuals
                // 或者简单粗暴一点：直接 Destroy(targetView.gameObject) 并在 SetupGame 里重新生成牌堆
                // 但这里我们仅仅移动位置是最高效的。
                // 此时它是背面，所以颜色无所谓。
            }
        }
    }
    // [修改] 创建卡牌方法接收 RuntimeCard
    private BattleCardView CreateCard(RuntimeCard card, Transform parent, bool isFaceUp)
    {
        if (card == null) return null;

        GameObject go = Instantiate(battleCardPrefab, parent);
        BattleCardView view = go.GetComponent<BattleCardView>();

        // 传递 RuntimeCard
        if (view != null) view.Init(card, isFaceUp);

        return view;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container) Destroy(child.gameObject);
    }

    // [新增] 获取当前处于“调度选中状态”的手牌数量
    public int GetSelectedMulliganCount()
    {
        int count = 0;
        foreach (Transform child in panelHand)
        {
            var view = child.GetComponent<BattleCardView>();
            if (view != null && view.IsSelectedForMulligan)
            {
                count++;
            }
        }
        return count;
    }
}
// DeckPanel.cs
// 父容器，整个卡组显示
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckPanelParent : MonoBehaviour
{
    [Header("Head")]
    public TMP_Dropdown dropDown_selectDeck;
    public Button button_createDeck;
    public Button button_renameDeck;
    public Button button_deleteDeck;
    public Button button_saveDeck;
    [Header("Body")]
    public RectTransform contentRect_DeckPanelParent;
    public GameObject deckCardViewPrefab;

    private List<DeckSingleCardPrefabView> deckSingleCardViews = new List<DeckSingleCardPrefabView>();
    [SerializeField] private GameObject dropPlaceholder = null!;

    //private static bool _deckModified = false;

    private void Start()
    {
        DeckSingleCardPrefabView.OnItemDraggedAction += UpdateDeckPanelUI;
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnDeckUpdated += UpdateDeckPanelUI;

            RefreshDropDown_selectDeck();
        }
        //<--------- 初始化功能按钮 ----------->
        button_createDeck.onClick.AddListener(async () => await On_CreateDeckButton_Clicked());
        dropDown_selectDeck.onValueChanged.AddListener(On_DropDown_selectDeck_Changed);
        button_deleteDeck.onClick.AddListener(On_DeleteDeckButton_Clicked);
        button_saveDeck.onClick.AddListener(async ()=> await On_SaveDeckButton_Clicked());

        //<-------- 完成控件初始化 ------------>
        UpdateDeckPanelUI();

        // 软创建一个拖拽器
        if (dropPlaceholder == null)
        {
            dropPlaceholder = new GameObject("DropPlaceholder");
            dropPlaceholder.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 30f);
            dropPlaceholder.transform.SetParent(contentRect_DeckPanelParent);
        }
        dropPlaceholder.SetActive(false);
    }

    private void OnDestroy()
    {
        DeckSingleCardPrefabView.OnItemDraggedAction -= UpdateDeckPanelUI;
        if (DeckManager.Instance != null)
            DeckManager.Instance.OnDeckUpdated -= UpdateDeckPanelUI;
    }

    private void UpdateDeckPanelUI()
    {
        foreach (var view in deckSingleCardViews) 
            Destroy(view.gameObject);

        deckSingleCardViews.Clear();

        var rawList = DeckManager.Instance.GetCurrentDeckCardIDs();
        if (rawList.Count == 0) return;

        // --- 生成 UI ---
        // 相同卡合并显示
        for (int i = 0; i < rawList.Count; i++)
        {
            string currentID = rawList[i];
            int count = 1;
            while (i + 1 < rawList.Count && rawList[i + 1] == currentID)
            {
                count++;
                i++;
            }
            // 传入 targetDisplayHeroID 进行比对
            CreateCardView(currentID, count);
        }

        if (dropPlaceholder != null)
            dropPlaceholder.transform.SetAsLastSibling();
    }

    private void CreateCardView(string cardID, int count)
    {
        var go = Instantiate(deckCardViewPrefab, contentRect_DeckPanelParent);
        var view = go.GetComponent<DeckSingleCardPrefabView>();
        CardBaseInfo data = CardInstanceHelper.GetCardBaseData(cardID);

        bool isDraggable = true;

        // 1. 不允许拖拽类型
        if (data.cardType == CardType.Legend ||
            data.cardType == CardType.Rune ||
            data.cardType == CardType.Battlefield)
        {
            isDraggable = false;
        }

        view.DeckSingleCardView_Init(cardID, count, this, isDraggable);
        deckSingleCardViews.Add(view);
    }

    private void SaveModify()
    {
        // 重建 ID 列表
        List<string> newDeckOrderIDs = new List<string>();

        // 获取当前顺序的 View
        var orderedViews = contentRect_DeckPanelParent.GetComponentsInChildren<DeckSingleCardPrefabView>(false)
            .OrderBy(v => v.transform.GetSiblingIndex())
            .ToList();

        foreach (var view in orderedViews)
        {
            if ( int.TryParse(view.countText.text.Replace("x", ""), out int count) )
            {
                for (int i = 0; i < count; i++)
                {
                    newDeckOrderIDs.Add(view.CardID);
                }
            }
        }

        DeckManager.Instance.reSortDeckCards(newDeckOrderIDs);
        
    }

    #region Head 下拉列表+功能操作按钮行为
    private void On_DropDown_selectDeck_Changed(int index)
    {
        string selectedName = dropDown_selectDeck.options[index].text;
        if (selectedName != null)
        {
            SaveModify();
            DeckManager.Instance.LoadDeckInstance(selectedName);
            UpdateDeckPanelUI();
        }
    }
    public async Task On_CreateDeckButton_Clicked()
    {
        string? newDeckName = await InteractionHelper.Instance.ShowGetInputAsync("输入卡组名");
        if (string.IsNullOrWhiteSpace(newDeckName))
        {
            Debug.Log("未输入有效名称，取消操作");
            return;
        }
        DeckManager.Instance.CreateNewDeck(newDeckName);
        SysInfo.sysLogInfo(SysLogHead.Info, "已创建" + newDeckName);
        RefreshDropDown_selectDeck();
    }
    public void On_DeleteDeckButton_Clicked()
    {
        var options = dropDown_selectDeck.options;
        if (options == null || options.Count == 0) return;
        string? showNewDeckName =null; 
        if (options.Count > 1)
        {
            int currentIndex = dropDown_selectDeck.value;
            int nextIndex = (currentIndex == options.Count - 1) ? currentIndex - 1 : currentIndex + 1;
            showNewDeckName = options[nextIndex].text;
        }

        DeckManager.Instance.DeleteDeck();
        SysInfo.sysLogInfo(SysLogHead.Info, "已删除" + options[dropDown_selectDeck.value].text);
        RefreshDropDown_selectDeck();
    }
    private async Task On_SaveDeckButton_Clicked()
    {
        bool isSuccess = DeckManager.Instance.SaveDeck(out StringBuilder message);
        if (isSuccess)
        {
            await InteractionHelper.Instance.ShowMessageAsync("提示", "保存成功");
        }
        else
        {
            await InteractionHelper.Instance.ShowWarnAsync("提示",message.ToString());
        }
    }
    private void RefreshDropDown_selectDeck()
    {
        if (DeckManager.Instance.UsrDecks != null)
        {
            dropDown_selectDeck.ClearOptions();
            if (DeckManager.Instance.UsrDecks.ToList().Count != 0)
            {
                dropDown_selectDeck.AddOptions(DeckManager.Instance.UsrDecks.ToList());
                dropDown_selectDeck.value = 0;
                dropDown_selectDeck.RefreshShownValue();

                DeckManager.Instance.LoadDeckInstance(dropDown_selectDeck.options[dropDown_selectDeck.value].text);
            }
        }
        SysInfo.sysLogInfo(SysLogHead.Info, "已刷新卡组列表");
    }
    #endregion


    #region 卡栏目拖拽功能
    public void HandleDragMove(RectTransform draggingObject)
    {
        dropPlaceholder.SetActive(true);
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < deckSingleCardViews.Count; i++)
        {
            if (deckSingleCardViews[i].transform == draggingObject.transform) continue;

            float distance = Mathf.Abs(deckSingleCardViews[i].transform.position.y - draggingObject.position.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                if (draggingObject.position.y > deckSingleCardViews[i].transform.position.y)
                {
                    closestIndex = deckSingleCardViews[i].transform.GetSiblingIndex();
                }
                else
                {
                    closestIndex = deckSingleCardViews[i].transform.GetSiblingIndex() + 1;
                }
            }
        }

        // 强制约束：索引不能超过子物体总数
        if (closestIndex > contentRect_DeckPanelParent.childCount) closestIndex = contentRect_DeckPanelParent.childCount;

        dropPlaceholder.transform.SetSiblingIndex(closestIndex);
    }

    public void HandleDragEnd(DeckSingleCardPrefabView draggingView)
    {
        int newIndex = dropPlaceholder.transform.GetSiblingIndex();

        draggingView.transform.SetSiblingIndex(newIndex);
        dropPlaceholder.SetActive(false);
        
    }
    #endregion
}
//////////////////////////////////////
/// 卡组编辑界面主要窗口
/// 《卡组编辑窗口》具有其组件
/// 进行前端显示更新
//////////////////////////////////////
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuilderMainWindow : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button saveButton;
    // [SerializeField] private Text messageText; // 可选：用于显示验证结果

    private void Awake()
    {
        backButton.onClick.AddListener(OnBackClicked);
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveClicked);
        }
    }

    private void OnSaveClicked()
    {
        if (DeckManager.Instance != null)
        {
            //string message;
            //bool success = DeckManager.Instance.ValidateDeck(out message);

            //Debug.Log(message); // 在控制台输出验证结果

        }
    }

    private void OnBackClicked()
    {
        //if (DeckManager.Instance != null)
        //{
        //    DeckManager.Instance.SaveDeck();
        //}
        SceneManager.LoadScene("MainMenuScene");
    }
}
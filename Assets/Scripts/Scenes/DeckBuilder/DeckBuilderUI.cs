using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuilderUI : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button saveButton; // <--- 新增
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
            string message;
            bool success = DeckManager.Instance.ValidateDeck(out message);

            Debug.Log(message); // 在控制台输出验证结果

            // 如果你有UI弹窗系统，在这里调用
            // UIManager.ShowMessage(message); 
        }
    }

    private void OnBackClicked()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.SaveDeck();
        }
        SceneManager.LoadScene("MainMenuScene");
    }
}
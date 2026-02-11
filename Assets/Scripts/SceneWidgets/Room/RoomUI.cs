using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        startGameButton.onClick.AddListener(OnStartGameClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnStartGameClicked()
    {
        // 从自定义房间进入对战界面
        SceneManager.LoadScene("BattleScene");
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}

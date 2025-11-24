using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playOnlineButton;
    [SerializeField] private Button deckBuilderButton;

    private void Awake()
    {
        playOnlineButton.onClick.AddListener(OnPlayOnlineClicked);
        deckBuilderButton.onClick.AddListener(OnDeckBuilderClicked);
    }

    private void OnPlayOnlineClicked()
    {
        // 从主界面进入自定义房间界面
        SceneManager.LoadScene("RoomScene");
    }

    private void OnDeckBuilderClicked()
    {
        // 从主界面进入组卡界面
        SceneManager.LoadScene("DeckBuilderScene");
    }
}

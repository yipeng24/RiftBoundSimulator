using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnExitClicked()
    {
        // 对战结束后返回主界面（后面可以改成结果结算界面）
        SceneManager.LoadScene("MainMenuScene");
    }
}

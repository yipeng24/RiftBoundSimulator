using TMPro;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public enum MessageBoxType
{
    ShowMassage, WarnMassage, GetInputMassage
} 
public class MessageBoxPrefabView : MonoBehaviour//, IPointerClickHandler
{
    public TMP_Text titleText;
    public GameObject messageScrollview;
    public TMP_Text messageText;
    public TMP_InputField inputField;
    public UnityEngine.UI.Button confirmButton;
    public UnityEngine.UI.Button cancelButton;

    private float buttonYesOrigin_x;
    private void Awake()
    {
        buttonYesOrigin_x = confirmButton.GetComponent<RectTransform>().anchoredPosition.x;
    }
    public void Init_MessageBoxPrefab(string title, string content, MessageBoxType type, UnityAction onConfirm = null, UnityAction onCancel = null)
    {
        titleText.text = title;
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            //Destroy(gameObject); // 点击后销毁
        });
        cancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            //Destroy(gameObject); // 点击后销毁
        });
        RectTransform confirmRT = confirmButton.GetComponent<RectTransform>();

        inputField.gameObject.SetActive(false);
        messageScrollview.SetActive(false);
        //messageText.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(true);
        confirmRT.anchoredPosition = new Vector2(buttonYesOrigin_x, confirmRT.anchoredPosition.y);
        switch (type)
        {
            case MessageBoxType.ShowMassage:
                //messageText.gameObject.SetActive(true);
                messageScrollview.gameObject.SetActive(true);
                messageText.text = content;
                break;

            case MessageBoxType.WarnMassage:
                //messageText.gameObject.SetActive(true);
                messageScrollview.gameObject.SetActive(true);
                messageText.text = content;                
                cancelButton.gameObject.SetActive(false); 
                confirmRT.anchoredPosition = new Vector2(0, confirmRT.anchoredPosition.y); 
                break;

            case MessageBoxType.GetInputMassage:
                inputField.gameObject.SetActive(true);
                inputField.text = ""; 
                break;
        }
    }

    public string GetInputText()
    {
        return inputField.text;
    }
}
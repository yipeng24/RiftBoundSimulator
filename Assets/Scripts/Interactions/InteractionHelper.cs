#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

public interface IInteractionHelper
{
    Task<bool> ShowMessageAsync(string title, string message);
    Task ShowWarnAsync(string title, string message);
    Task<string?> ShowGetInputAsync(string title);
}

public class InteractionHelper : MonoBehaviour, IInteractionHelper
{
    public GameObject messageBoxPrefab = null!;
    public Transform canvasTransform = null!;
    public static InteractionHelper Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public async Task ShowWarnAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        var go = Instantiate(messageBoxPrefab, canvasTransform);
        go.GetComponent<MessageBoxPrefabView>().Init_MessageBoxPrefab(title, message, MessageBoxType.ShowMassage, () => { tcs.SetResult(true); Destroy(go); });
        await tcs.Task;
    }

    public async Task<bool> ShowMessageAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        var go = Instantiate(messageBoxPrefab, canvasTransform);
        go.GetComponent<MessageBoxPrefabView>().Init_MessageBoxPrefab(title, message, MessageBoxType.ShowMassage, 
            onConfirm: () =>
            {
                tcs.SetResult(true); 
                Destroy(go);
            }, 
            onCancel: () =>
            {
                tcs.SetResult(false);
                Destroy(go);
            }
        );

        return await tcs.Task;
    }

    public async Task<string?> ShowGetInputAsync(string title)
    {
        var tcs = new TaskCompletionSource<string?>();
        GameObject go = Instantiate(messageBoxPrefab, canvasTransform);
        var view = go.GetComponent<MessageBoxPrefabView>();
        view.Init_MessageBoxPrefab(
            title,
            "",
            MessageBoxType.GetInputMassage,
            onConfirm: () =>
            {
                string inputText = view.GetInputText();
                tcs.TrySetResult(inputText);
                Destroy(go);
            },
            onCancel: () =>
            {
                tcs.SetResult(null);
                Destroy(go);
            }
        );
        return await tcs.Task;
    }
}
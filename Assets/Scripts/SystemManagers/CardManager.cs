using System.Collections.Generic;
using Unity;
using UnityEngine;

class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    public CardInstanceHelper CardInstanceHelper { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 如果已经存在实例，销毁重复的
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);


        CardInstanceHelper = new CardInstanceHelper();
        CardInstanceHelper.InitializeDatabase();
    }


}

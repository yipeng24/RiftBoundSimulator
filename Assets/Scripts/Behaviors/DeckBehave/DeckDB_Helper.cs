using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DeckDB_Helper
{
    private static readonly string BasePath = SysConfig.DEFAULT_DECK_FULL_PATH;//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SysConfig.DEFAULT_DECK_PATH_PREFIX);
    private static readonly List<string> UsrDeckList = new List<string>();

    public void CreateDeck(string deckName)
    {
        try
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            string deckFilePath = Path.Combine(BasePath, $"{deckName}.json");
            if (File.Exists(deckFilePath))
            {
                Debug.LogWarning($"Deck '{deckName}' already exists.");
                return;
            }
            File.WriteAllText(deckFilePath, "{}", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating deck '{deckName}': {ex.Message}");
        }
    }

    public List<string> GetUsrDeckList()
    {
        UsrDeckList.Clear();
        try
        {
            if (!Directory.Exists(BasePath))
            {
                return UsrDeckList;
            }
            string[] files = Directory.GetFiles(BasePath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string filePath in files)
            {
                UsrDeckList.Add(Path.GetFileNameWithoutExtension(filePath));
                
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error retrieving user deck list: {ex.Message}");
        }
        return UsrDeckList;
    }

    public void DeleteDeck(string deckName)
    {
        try
        {
            string deckFilePath = Path.Combine(BasePath, $"{deckName}.json");
            if (File.Exists(deckFilePath))
            {
                File.Delete(deckFilePath);                
            }
            else
            {
                SysInfo.sysLogInfo(SysLogHead.Error, $"{deckName} ²»´æÔÚ");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting deck '{deckName}': {ex.Message}");
        }
    }

}

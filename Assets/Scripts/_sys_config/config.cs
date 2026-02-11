using System;
using System.IO;

public static class SysConfig
{
    public const string DEFAULT_DECK_PATH_PREFIX = "UsrData";
    public static string DEFAULT_DECK_FULL_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DEFAULT_DECK_PATH_PREFIX);
}
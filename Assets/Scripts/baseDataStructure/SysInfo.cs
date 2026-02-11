using UnityEngine;

public enum SysLogHead
{
    Info,
    Warning,
    Error
}
class SysInfo
{
    public static void sysLogInfo(SysLogHead head, string message)
    {
        switch(head)
        {
            case SysLogHead.Error:
                Debug.LogError("[" + head + "] " + message);
                return;
            case SysLogHead.Warning:
                Debug.LogWarning("[" + head + "] " + message);
                return;
            case SysLogHead.Info:
                Debug.Log("[" + head + "] " + message);
                return;
        }
    }
}

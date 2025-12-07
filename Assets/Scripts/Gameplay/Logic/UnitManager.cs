using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    // 全局所有存活单位的列表
    public List<RuntimeUnit> AllUnits = new List<RuntimeUnit>();

    private void Awake() { Instance = this; }

    public void RegisterUnit(RuntimeUnit unit)
    {
        if (!AllUnits.Contains(unit)) AllUnits.Add(unit);
    }

    public void UnregisterUnit(RuntimeUnit unit)
    {
        if (AllUnits.Contains(unit)) AllUnits.Remove(unit);
    }
}
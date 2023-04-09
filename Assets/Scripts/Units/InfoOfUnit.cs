using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoOfUnit: MonoBehaviour
{
    public InfoOfUnitStruct infoOfUnitStruct;

    public void SetTeamNumberOfUnit(int teamNumber)
    {
        infoOfUnitStruct.teamNumber = teamNumber;
    }

    public int GetTeamNumberOfUnit()
    {
        return infoOfUnitStruct.teamNumber;
    }
}

[Serializable]
public struct InfoOfUnitStruct
{
    [SerializeField] private string Name;
    [SerializeField] private GameObject UnitGameObject;
    [SerializeField] private Animator Animator;
    [SerializeField] public int teamNumber;
}

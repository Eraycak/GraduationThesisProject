using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoOfUnit: MonoBehaviour
{
    public InfoOfUnitStruct infoOfUnitStruct;
}

[Serializable]
public struct InfoOfUnitStruct
{
    [SerializeField] private string Name;
    [SerializeField] private GameObject UnitGameObject;
    [SerializeField] private Animator Animator;
}

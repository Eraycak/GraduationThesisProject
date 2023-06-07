using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InfoOfUnit : MonoBehaviour
{
    [SerializeField] private string nameOfUnit;
    [SerializeField] private GameObject unitGameObject;
    [SerializeField] private Animator animator;
    [SerializeField] private int teamNumber;
    [SerializeField] private int healthValue;
    [SerializeField] private int damageValue;
    private Vector3 unitsPosition = Vector3.zero;
    private Quaternion unitsRotation = Quaternion.identity;
    internal int costOfUnit = 5;
    [SerializeField] private int startHealthValue;

    public string Name
    {
        get { return nameOfUnit; }
        set { nameOfUnit = value; }
    }
    public GameObject UnitGameObject
    {
        get { return unitGameObject; }
        set { unitGameObject = value; }
    }
    public Animator Animator
    {
        get { return animator; }
        set { animator = value; }
    }
    public int TeamNumber
    {
        get { return teamNumber; }
        set { teamNumber = value; }
    }
    public int HealthValue
    {
        get { return healthValue; }
        set { 
            healthValue = value;
            if (healthValue <= 0)
            {
                StartCoroutine(DieAnimationCoroutine());
            }
        }
    }
    public int DamageValue
    {
        get { return damageValue; }
        set { damageValue = value; }
    }

    public Vector3 UnitsPosition
    {
        get { return unitsPosition; }
        set { unitsPosition = value; }
    }

    public Quaternion UnitsRotation
    {
        get { return unitsRotation; }
        set { unitsRotation = value; }
    }

    private IEnumerator DieAnimationCoroutine()
    {
        Animator.SetTrigger("Died");
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    public int StartHealthValue
    {
        get { return startHealthValue; }
    }
}
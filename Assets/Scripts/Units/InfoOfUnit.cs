using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class InfoOfUnit : NetworkBehaviour
{
    [SerializeField] public Animator animator;
    [SerializeField] private Animator localAnimator;
    [SerializeField] public NetworkVariable<int> TeamNumber = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> healthValue;
    [SerializeField] private int localHealthValue;
    [SerializeField] public NetworkVariable<int> damageValue;
    [SerializeField] private int localDamageValue;
    public NetworkVariable<Vector3> unitsPosition;
    public NetworkVariable<Quaternion> unitsRotation;
    [SerializeField] public int costOfUnit;
    [SerializeField] public NetworkVariable<int> startHealthValue;

    public Animator Animator
    {
        get { return animator; }
        set { animator = value; }
    }

    public NetworkVariable<int> HealthValue
    {
        get { return healthValue; }
        set {  healthValue.Value = value.Value; }
    }
    public NetworkVariable<int> DamageValue
    {
        get { return damageValue; }
        set { damageValue = value; }
    }

    public NetworkVariable<Vector3> UnitsPosition
    {
        get { return unitsPosition; }
        set { unitsPosition = value; }
    }

    public NetworkVariable<Quaternion> UnitsRotation
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

    public NetworkVariable<int> StartHealthValue
    {
        get { return startHealthValue; }
    }

    private void SetTeamNumber(int _value, ulong clientId)
    {
        TeamNumber.Value = _value;
    }

    public void SetTeamNumberOfObject(int currentPlayerTeamNumber)
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            SetTeamNumber(currentPlayerTeamNumber, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            SetOnServerRpc(currentPlayerTeamNumber);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetOnServerRpc(int _currentPlayerTeamNumber, ServerRpcParams serverRpcParams = default)
    {
        SetTeamNumber(_currentPlayerTeamNumber, serverRpcParams.Receive.SenderClientId);
    }

    private void Start()
    {
        UnitsPosition.Value = Vector3.zero;
        UnitsRotation.Value = Quaternion.identity;
        costOfUnit = 5;
        HealthValue.Value = localHealthValue;
        startHealthValue.Value = localHealthValue;
        DamageValue.Value = localDamageValue;
        Animator = localAnimator;
        HealthValue.OnValueChanged += OnHealthValueChanged;
    }

    private void OnHealthValueChanged(int oldValue, int newValue)
    {
        if (newValue <= 0)
        {
            StartCoroutine(DieAnimationCoroutine());
        }
    }
}
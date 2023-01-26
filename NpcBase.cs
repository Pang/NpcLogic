using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NpcBase : MonoBehaviour
{
    [HideInInspector] public PlayerBase HostileTarget;
    [HideInInspector] public NpcBaseState CurrentState;
    [HideInInspector] public Vector3 TargetLastKnownLocation;
    [HideInInspector] public bool TurnTowardsTarget;

    [SerializeField] public List<GameObject> NpcSkins;
    public GameObject SkinSelected { get; private set; }

    public NetworkScene NetworkScene { get; private set; }
    public NpcStateManager StateManager { get; private set; }
    public NavMeshPath NavMeshPath { get; private set; }
    public NpcHealth NpcHealth { get; private set; }
    public Rigidbody Rb { get; private set; }
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    public int NpcSceneId;
    public Vector3 HeadPos { get; private set; } = new Vector3(0, 3, 0);
    public float WalkSpeed { get; private set; } = 3f;
    public float RunSpeed { get; private set; } = 8f;
    public float TurningRunSpeed { get; private set; } = 360f;
    public float TurningWalkSpeed { get; private set; } = 200f;
    public float AttackDmg { get; private set; } = 10f;
    public float CutoffSight { get; private set; } = 45f;
    public float Radius { get; private set; } = 20f;

    public GameObject WeaponObj;
    public GameObject QuestionMarkAlert;

    public Action<PlayerBase> Alarm;

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Agent = GetComponent<NavMeshAgent>();
        NpcHealth = GetComponent<NpcHealth>();
        Animator = GetComponentInChildren<Animator>();
        NetworkScene = FindObjectOfType<NetworkScene>();
        StateManager = FindObjectOfType<NpcStateManager>();
        NavMeshPath = new NavMeshPath();
    }

    void Start()
    {
        Alarm += OnAlarm;
        NpcHealth.Death += OnDeath;
        NpcHealth.HitWhileIdle += AlarmOthers;
        StateManager.SwitchState(StateManager.IdleState, this);

        SkinSelected = NpcSkins[UnityEngine.Random.Range(0, NpcSkins.Count)];
        SkinSelected.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (TurnTowardsTarget) RotateTowards(HostileTarget.transform);
    }

    public void OnAlarm(PlayerBase player)
    {
        if (CurrentState != StateManager.DeadState)
        {
            HostileTarget = player;
            StateManager.SwitchState(StateManager.ChasingState, this);
        }
    }
    public void AlarmOthers(PlayerBase player)
    {
        foreach (NpcBase npc in FindObjectsOfType<NpcBase>())
        {
            if (npc == this) continue;

            float distance = (npc.transform.position - this.transform.position).magnitude;
            if (distance < Radius && npc.Alarm?.GetInvocationList() != null) npc.Alarm.Invoke(player);
        }
    }

    public void AlarmOthersInSight(PlayerBase player)
    {
        foreach (NpcBase npc in FindObjectsOfType<NpcBase>())
        {
            if (npc == this) continue;

            float distance = (npc.transform.position - this.transform.position).magnitude;
            if (distance < Radius)
            {
                bool isInSight = HelperMethods.CheckInRangeAndSight(this.transform.position, npc.transform, Radius, CutoffSight);
                if (isInSight && !npc.NpcHealth.IsDead) npc.Alarm.Invoke(player);
            }
        }
    }

    public IEnumerator AlarmRoutine()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (!NpcHealth.IsDead)
        {
            foreach(var player in players)
            {
                if (HelperMethods.CheckInRangeAndSight(player.transform.position, transform, Radius, CutoffSight))
                    Alarm.Invoke(player.GetComponent<PlayerBase>());
            }
            yield return wait;
        }
    }

    public IEnumerator AlertRoutine()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (!NpcHealth.IsDead)
        {
            yield return wait;
            foreach(var player in players)
            {
                if (HelperMethods.CheckInRange(player.transform.position, transform.position, Radius))
                {
                    PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                    if (!playerMovement.crouching && playerMovement.move != Vector2.zero) StateManager.SwitchState(StateManager.AlertState, this);
                }
            }
        }
    }
    
    private void OnDeath()
    {
        AlarmOthersInSight(HostileTarget);
        StateManager.SwitchState(StateManager.DeadState, this);
        Alarm -= OnAlarm;
        NpcHealth.Death -= OnDeath;
        NpcHealth.HitWhileIdle -= AlarmOthers;
    }

    public void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class NpcStateManager : MonoBehaviour
{
    public static NpcStateManager Instance { get; private set; }
    private AnnounceUI AnnounceUI;

    public static Dictionary<int, GameObject> NpcsInScene = new Dictionary<int, GameObject>();
    private int NoOfNpcsInScene = 0;
    public List<GameObject> NpcTypes = new List<GameObject>();

    public NpcIdleState IdleState = new NpcIdleState();
    public NpcAlertState AlertState = new NpcAlertState();
    public NpcInvestigateState InvestigateState = new NpcInvestigateState(); 
    public NpcChasingState ChasingState = new NpcChasingState();
    public NpcAttackingState AttackingState = new NpcAttackingState(); 
    public NpcDeadState DeadState = new NpcDeadState();

    public Action NpcKilled;

    private void Awake() => Instance = this;
    private void Start()
    {
        SpawnNpcsInScene();
        NpcKilled += OnNpcKilled;
        AnnounceUI = FindObjectOfType<AnnounceUI>();
    }

    public void SwitchState(NpcBaseState state, NpcBase npc)
    {
        if (npc.CurrentState != null) npc.CurrentState.LeaveState(this, npc);
        npc.CurrentState = state;
        state.EnterState(this, npc);
    }

    private void SpawnNpcsInScene()
    {
        foreach (var npc in NpcPlacementData.Npcs)
        {
            var npcInit = Instantiate(NpcTypes.FirstOrDefault(), npc.Position, npc.Rotation);
            npcInit.GetComponent<NpcBase>().NpcSceneId = npc.Id;
            NpcsInScene.Add(npc.Id, npcInit);
        }
        NoOfNpcsInScene = NpcsInScene.Count();
    }

    private void GetAllNpcsInScene()
    {
        var npcs = FindObjectsOfType<NpcBase>();
        var npcsInSceneProxy = new Dictionary<int, GameObject>();

        for (int i = 0; i < npcs.Count(); i++)
        {
            npcs[i].NpcSceneId = i;
            npcsInSceneProxy.Add(i, npcs[i].gameObject);
        }
    }

    public void OnNpcKilled()
    {
        NoOfNpcsInScene--;
        if (NoOfNpcsInScene == 0)
        {
            StartCoroutine(this.AnnounceUI.CreateAnnouncement($"All enemies defeated"));
            return;
        }
        StartCoroutine(this.AnnounceUI.CreateAnnouncement($"{NoOfNpcsInScene} enemies remaining!"));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcIdleState : NpcBaseState
{
    public override void EnterState(NpcStateManager state, NpcBase npc)
    {
        SetNpcStats(state, npc);
        npc.StartCoroutine(npc.AlarmRoutine());
        npc.StartCoroutine(npc.AlertRoutine());
    }

    public override void LeaveState(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Idle", false);
        npc.StopAllCoroutines();
    }

    protected override void SetNpcStats(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Idle", true);
    }

    protected override IEnumerator Routine(NpcStateManager state, NpcBase npc)
    {
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        yield return wait;
    }
}

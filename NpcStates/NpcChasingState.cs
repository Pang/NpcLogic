using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NpcChasingState : NpcBaseState
{
    public override void EnterState(NpcStateManager state, NpcBase npc)
    {
        SetNpcStats(state, npc);
        npc.StartCoroutine(Routine(state, npc));

    }

    public override void LeaveState(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Run", false);
        npc.StopAllCoroutines();
    }

    protected override void SetNpcStats(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Run", true);
        npc.Agent.speed = npc.RunSpeed;
        npc.Agent.angularSpeed = npc.TurningRunSpeed;

        npc.Agent.isStopped = false;
        npc.Agent.updateRotation = true;
    }

    protected override IEnumerator Routine(NpcStateManager state, NpcBase npc)
    {
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (npc.CurrentState == this)
        {
            yield return wait;
            if (npc.HostileTarget != null)
            {
                var target = npc.HostileTarget.transform;
                npc.Agent.SetDestination(target.position);

                float distance = Vector3.Distance(npc.transform.position, target.position);
                //Debug.Log(distance + " | " + (npc.agent.stoppingDistance + 1));
                if (distance <= (npc.Agent.stoppingDistance + 1))
                {
                    state.SwitchState(state.AttackingState, npc);
                }
                else
                {
                    npc.Agent.SetDestination(target.position);
                }
            }
        }
    }
}
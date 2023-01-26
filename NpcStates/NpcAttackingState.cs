using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class NpcAttackingState : NpcBaseState
{
    private bool InRange;
    private float ExtraDistanceToHit = 0f;
    private float DistanceToAttack = 0f;

    public override void EnterState(NpcStateManager state, NpcBase npc)
    {
        SetNpcStats(state, npc);
        npc.StartCoroutine(Routine(state, npc));
    }
    public override void LeaveState(NpcStateManager state, NpcBase npc)
    {
        npc.StopAllCoroutines();
        npc.Animator.SetBool("Idle", false);
        npc.Animator.SetBool("Attacking", false);
    }

    protected override void SetNpcStats(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Idle", true);
        DistanceToAttack = npc.Agent.stoppingDistance + ExtraDistanceToHit;
    }

    protected override IEnumerator Routine(NpcStateManager state, NpcBase npc)
    {
        while (npc.CurrentState == this)
        {
            npc.Animator.SetTrigger("Attack");
            npc.Animator.SetBool("Attacking", true);
            var target = npc.HostileTarget.transform;
            npc.TurnTowardsTarget = true;

            yield return new WaitForSeconds(0.7f);
            if (npc.CurrentState != this) npc.StopCoroutine(Routine(state, npc));

            DmgTargetInRange(npc, target);
            npc.Animator.ResetTrigger("Attack");
            npc.Animator.SetBool("Attacking", false);

            yield return new WaitForSeconds(1);
            CheckTargetOutOfRange(state, npc, target);
        }
    }

    private void DmgTargetInRange(NpcBase npc, Transform target)
    {
        float distance = Vector3.Distance(npc.transform.position, target.position);
        if (distance <= DistanceToAttack)
        {
            InRange = true;
            npc.Agent.updateRotation = false;
            npc.Agent.isStopped = true;
            var playerHealth = target.gameObject.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(npc.AttackDmg);
            npc.Animator.SetBool("Run", false);
            npc.Animator.SetBool("Idle", true);
        }
    }

    private void CheckTargetOutOfRange(NpcStateManager state, NpcBase npc, Transform target)
    {
        float distanceAfter = Vector3.Distance(npc.transform.position, target.position);
        if (distanceAfter > DistanceToAttack)
        {
            InRange = false;
            npc.TurnTowardsTarget = false;
            npc.Agent.isStopped = false;
            npc.Agent.updateRotation = true;
            npc.Animator.SetBool("Idle", false);
            state.SwitchState(state.ChasingState, npc);
            npc.StopCoroutine(Routine(state, npc));
        }
    }
}

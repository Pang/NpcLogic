using System;
using System.Collections;
using UnityEngine;

public class NpcInvestigateState : NpcBaseState
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    public Vector3 areaToInvestigate;
    private bool _reachedInvestigatePoint;

    public override void EnterState(NpcStateManager state, NpcBase npc)
    {
        SetNpcStats(state, npc);
        areaToInvestigate = npc.TargetLastKnownLocation;
        npc.Agent.SetDestination(areaToInvestigate);

        npc.StartCoroutine(Routine(state, npc));
        npc.StartCoroutine(npc.AlarmRoutine());
    }
    public override void LeaveState(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Walk", false);
        npc.StopAllCoroutines();
    }

    protected override void SetNpcStats(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Walk", true);

        _reachedInvestigatePoint = false;
        startPosition = npc.transform.position;
        startRotation = npc.transform.rotation;

        npc.Agent.speed = npc.WalkSpeed;
        npc.Agent.angularSpeed = npc.TurningWalkSpeed;
    }

    protected override IEnumerator Routine(NpcStateManager state, NpcBase npc)
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (npc.CurrentState == this)
        {
            yield return wait;
            CheckReachedDestination(state, npc);

            foreach (var player in players)
            {
                if (HelperMethods.CheckInRange(player.transform.position, npc.transform.position, npc.Radius))
                {
                    PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                    if (!playerMovement.crouching && playerMovement.move != Vector2.zero) npc.Agent.SetDestination(player.transform.position);
                }
            }

            if (npc.HostileTarget != null) state.SwitchState(state.ChasingState, npc);
        }
    }

    private void CheckReachedDestination(NpcStateManager state, NpcBase npc)
    {
        if (Vector3.Distance(npc.transform.position, npc.Agent.pathEndPosition) < npc.Agent.stoppingDistance)
        {
            if (!_reachedInvestigatePoint)
            {
                _reachedInvestigatePoint = true;
                npc.Agent.SetDestination(startPosition);
            }
            else 
            {
                npc.transform.rotation = startRotation;
                state.SwitchState(state.IdleState, npc);
            }
        }
    }
}


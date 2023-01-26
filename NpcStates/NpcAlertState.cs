using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAlertState : NpcBaseState
{
    private float showAlertSymbolTime;
    private float alertTime;
    public override void EnterState(NpcStateManager state, NpcBase npc)
    {
        SetNpcStats(state, npc);
        npc.QuestionMarkAlert.SetActive(true);
        npc.StartCoroutine(Routine(state, npc));
        npc.StartCoroutine(npc.AlarmRoutine());
        npc.StartCoroutine(ShakeAlertSymbol(npc));
    }

    public override void LeaveState(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Alert", false);
        npc.QuestionMarkAlert.SetActive(false);
        npc.StopAllCoroutines();
    }

    protected override void SetNpcStats(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Alert", true);
        npc.Agent.SetDestination(npc.transform.position);
        showAlertSymbolTime = 2f;
        alertTime = 2f;
        npc.TargetLastKnownLocation = GameObject.FindGameObjectWithTag("Player").transform.position;
    }

    protected override IEnumerator Routine(NpcStateManager state, NpcBase npc)
    {
        float delay = 0.02f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (alertTime > 0f)
        {
            yield return wait;
            alertTime -= delay;
            HelperMethods.SpriteToFaceCamera(npc.QuestionMarkAlert.transform);
        }
        
        state.SwitchState(state.InvestigateState, npc);
    }

    public IEnumerator ShakeAlertSymbol(NpcBase npc)
    {
        float delay = 0.02f;
        float lacunarity = 3f;
        int scale = 100;

        WaitForSeconds wait = new WaitForSeconds(delay);
        while (showAlertSymbolTime > 0f)
        {
            for (float x = 0; x < 1; x += 0.1f)
            {
                yield return wait;
                HelperMethods.ShakeRotational(npc.QuestionMarkAlert.transform, new Vector2(x, 0.0f), lacunarity, scale);
                showAlertSymbolTime -= delay;
            }
        }
        npc.StateManager.StopCoroutine(ShakeAlertSymbol(npc));
        npc.QuestionMarkAlert.SetActive(false);
    }
}

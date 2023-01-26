using System.Collections;
using UnityEditor;
using UnityEngine;

public class NpcDeadState : NpcBaseState
{
    public override void EnterState(NpcStateManager state, NpcBase npc)
    {
        SetNpcStats(state, npc);
        DropDead(npc);
        DropWeapon(npc);
        state.NpcKilled.Invoke();
    }

    public override void LeaveState(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Dead", false);
        npc.StopAllCoroutines();
    }

    protected override void SetNpcStats(NpcStateManager state, NpcBase npc)
    {
        npc.Animator.SetBool("Dead", true);
        npc.TurnTowardsTarget = false;
    }

    protected override IEnumerator Routine(NpcStateManager state, NpcBase npc)
    {
        float delay = 5f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        yield return wait;
        GameObject.Destroy(state.gameObject);
    }

    private void DropDead(NpcBase npc)
    {
        npc.Agent.isStopped = true;
        npc.Agent.velocity = Vector3.zero;
        npc.Rb.isKinematic = true;
        npc.GetComponent<Collider>().enabled = false;
    }

    private void DropWeapon(NpcBase npc)
    {
        npc.WeaponObj.transform.parent = null;
        var wepRB = npc.WeaponObj.AddComponent<Rigidbody>();
        wepRB.mass = 40;
        wepRB.drag = 1;
        npc.WeaponObj.AddComponent<CapsuleCollider>();
    }
}
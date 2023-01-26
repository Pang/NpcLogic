using System;
using System.Collections;
using UnityEngine;

public abstract class NpcBaseState
{
    public abstract void EnterState(NpcStateManager state, NpcBase npc);
    public abstract void LeaveState(NpcStateManager state, NpcBase npc);
    protected abstract IEnumerator Routine(NpcStateManager state, NpcBase npc);
    protected abstract void SetNpcStats(NpcStateManager state, NpcBase npc);
}

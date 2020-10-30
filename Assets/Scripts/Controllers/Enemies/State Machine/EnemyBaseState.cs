using UnityEngine;

public abstract class EnemyBaseState
{
    // EnterState is called whenever the enemy enters a state
    public abstract void EnterState(EnemyBase enemy);

    // LeaveState is called whenever the enemy leaves a state
    public abstract void LeaveState(EnemyBase enemy);

    // Update is called once per frame
    public abstract void Update(EnemyBase enemy);

    // OnDrawGizmos is called once every time gizmos are drawn to the scene view
    public abstract void OnDrawGizmos(EnemyBase enemy);
}

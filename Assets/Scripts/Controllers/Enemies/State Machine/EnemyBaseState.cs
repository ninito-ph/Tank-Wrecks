using UnityEngine;

public abstract class EnemyBaseState
{
    // EnterState is called whenever the enemy enters a state
    public abstract void EnterState(EnemyController enemy);

    // LeaveState is called whenever the enemy leaves a state
    public abstract void LeaveState(EnemyController enemy);

    // Update is called once per frame
    public abstract void Update(EnemyController enemy);

    // OnDrawGizmos is called once every time gizmos are drawn to the scene view
    public abstract void OnDrawGizmos(EnemyController enemy);
}

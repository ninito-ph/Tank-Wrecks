using UnityEngine;

public abstract class EnemyBaseState
{
    // Start is called whenever the enemy enters a state
    public abstract void EnterState(EnemyBase enemy);

    // Update is called once per frame
    public abstract void Update(EnemyBase enemy);
}

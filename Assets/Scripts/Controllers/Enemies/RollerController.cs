using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RollerController : EnemyBase
{
    private Coroutine gotoPlayerRoutine;
    public NavMeshAgent agent;
    public GameObject player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.Find("Tank (Player)");
        agent = gameObject.GetComponent<NavMeshAgent>();

        Debug.Log("Pathfinding: " + agent + ", player: " + playerReference);

        gotoPlayerRoutine = StartCoroutine(AIPathToPlayer());
    }

    private IEnumerator AIPathToPlayer()
    {
        TankFire();
        Vector3 destination = player.GetComponent<PlayerController>().bodyPosition;
        agent.SetDestination(destination);
        yield return new WaitForSeconds(10f);
    }
}

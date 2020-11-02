using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RepositionState : EnemyBaseState
{
    #region Field Declarations

    // Holds a reference to the pathing coroutine
    private Coroutine pathingRoutine;

    // Information used for AI debugging
    private Vector3 aiDestination = new Vector3(0f, 0f, 0f);
    private bool destinationIsValid = false;

    #endregion

    #region Abstract Implementation

    public override void EnterState(EnemyController enemy)
    {
        // Marks enemy as non-stationary
        enemy.IsStationary = false;
        // Starts pathing coroutine
        pathingRoutine = enemy.StartCoroutine(PathToPlayer(enemy, enemy.transform, enemy.PlayerReference.transform, enemy.PlayerReference.SafeFireAreaRadius));
        // Enables steering
        enemy.NavigationAgent.isStopped = false;
    }

    public override void LeaveState(EnemyController enemy)
    {
        // Stops and clears the coroutine
        enemy.StopCoroutine(pathingRoutine);
    }

    public override void Update(EnemyController enemy)
    {
        // Resets head rotation
        ResetHeadRotation(enemy, 1.65f);
    }

    public override void OnDrawGizmos(EnemyController enemy)
    {
        // If the AI debug mode is enabled
        if (enemy.AIDebugMode == true && enemy.PlayerReference != null)
        {
            // If the destination is valid, draw debug gizmos with green color, otherwise draw with red color
            if (destinationIsValid == true) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;

            // Draw a line from the enemy to the destination and from the destination to the player
            Gizmos.DrawLine(enemy.transform.position, enemy.NavigationAgent.destination);
            Gizmos.DrawSphere(enemy.NavigationAgent.destination, 0.65f);
            Gizmos.DrawLine(enemy.NavigationAgent.destination, enemy.PlayerReference.transform.position);
        }
    }

    #endregion

    #region Custom Methods

    #region Coroutines

    // Coroutine that constantly paths towards the player
    private IEnumerator PathToPlayer(EnemyController enemy, Transform enemyTransform, Transform playerTransform, float areaRadius)
    {
        // Caches waitforseconds return
        WaitForSeconds pathingInterval = new WaitForSeconds(0.65f);
        // Stores the player's previous position
        Vector3 destination = GetValidPoint(enemy, 15f);
        // Stores a safety cooldown to prevent stack overflows when changing states
        float safetyCooldown = 1f;

        // Loops eternally until the inevitable and somber heat death of the universe
        while (enemy.PlayerReference != null)
        {
            // Updates destination
            destination = GetValidPoint(enemy, 15f);

            // Stores the distance between the enemy and the player
            float distance = Vector3.Distance(enemy.PlayerReference.transform.position, enemy.transform.position);
            // The error margin to compare the distance
            float errorMargin = 1f;

            // Sets the destination
            if (destination != enemy.NavigationAgent.destination) enemy.NavigationAgent.SetDestination(destination);

            // Ticks down the safety cooldown
            safetyCooldown -= 0.65f;

            // Checks if the distance with the error margin is lesser than the safe fire area radius
            if (distance - errorMargin < enemy.PlayerReference.SafeFireAreaRadius && distance + errorMargin > enemy.PlayerReference.RushAreaFireRadius && ValidatePoint(enemy.TankParts["Head"].transform.position, enemy.NavigationAgent, enemy.PlayerReference.transform) == true && safetyCooldown <= 0)
            {
                // Transitions to the fire state
                enemy.TransitionToState(enemy.FireState);
            }

            // Waits for the pathing interval before pathing again
            yield return pathingInterval;
        }

        yield break;
    }

    #endregion

    // Turns the tank towards the player
    private void ResetHeadRotation(EnemyController enemy, float rotationStrenght)
    {
        // Checks if the rotation is not already 0 before running lerp
        if (enemy.TankParts["Head"].transform.localRotation != Quaternion.identity)
        {
            // The time point in the interpolation
            float rotationTime = Mathf.Min(rotationStrenght * Time.deltaTime, 1);
            // Sets the rotation to the lerp
            enemy.TankParts["Head"].transform.localRotation = Quaternion.Lerp(enemy.TankParts["Head"].transform.localRotation, Quaternion.identity, rotationTime);
        }

        // Checks if the rotation is not already the target rotation before running lerp
        if (enemy.TankParts["Cannon Anchor"].transform.localRotation != Quaternion.identity)
        {
            // The time point in the interpolation
            float rotationTime = Mathf.Min(rotationStrenght * Time.deltaTime, 1);
            // Sets the rotation to the lerp
            enemy.TankParts["Cannon Anchor"].transform.localRotation = Quaternion.Lerp(enemy.TankParts["Cannon Anchor"].transform.localRotation, Quaternion.identity, rotationTime);
        }
    }

    // Gets the nearest spatial point in a given circular radius, and returns the relative angle of that point in relation to the origin of the cirumference
    private Vector3 GetNearestPointInCircumference(Transform circumferenceOriginTransform, float radius, Vector3 origin, out float pointAngle)
    {
        // Calculates the difference between our position and the center of the radius
        Vector3 originPositionDelta = (origin - circumferenceOriginTransform.position);
        // Declares a vector3 nearestpoint
        Vector3 nearestPoint;
        // Calculates the nearest point
        nearestPoint = circumferenceOriginTransform.position + originPositionDelta / originPositionDelta.magnitude * radius;
        // Ignores the Y component
        nearestPoint.y = origin.y;

        // Outputs the relative angle to the 
        pointAngle = Vector3.Angle(circumferenceOriginTransform.forward * -1f, (circumferenceOriginTransform.position - origin));

        // Returns the nearest point
        return nearestPoint;
    }

    // Gets a relative point in a radius given an angle
    private Vector3 GetPointInCircumference(Transform radiusCenterTransform, float radius, float angle)
    {
        // Creates a new vector 3 to store the radius of the circle
        Vector3 pointInRadius = new Vector3(0f, 0f, 0f);
        float angleOffset = radiusCenterTransform.rotation.y;

        // Uses parametric circle equation to get point
        pointInRadius.x = radiusCenterTransform.position.x + (radius * Mathf.Cos(Mathf.Deg2Rad * (angle + angleOffset)));
        pointInRadius.z = radiusCenterTransform.position.z + (radius * Mathf.Sin(Mathf.Deg2Rad * (angle + angleOffset)));
        // Y axis is not used, so it isn't calculated

        // Returns the point in the radius
        return pointInRadius;
    }

    // Checks whether a point is valid by checking whether it exists in the navmesh and whether the player is or isn't obstructed
    private bool ValidatePoint(Vector3 point, NavMeshAgent agent, Transform targetTransform)
    {
        // Stores information about the navmesh sampled position, if it exists
        NavMeshHit sampledPosition;
        // Whether the NavMesh was able to sample a position in the navmesh
        bool navMeshSampleSucceeded = NavMesh.SamplePosition(point, out sampledPosition, agent.height * 2, 1);

        // Calculates the direction of the raycast
        Vector3 raycastDirection = (sampledPosition.position - targetTransform.position) * -1f;
        raycastDirection.y = 0f;

        /*     
        No longer any useful but interesting to know it can also be done that way
        // Creates a layermask out of a bitmask by left shifting 1 by the desired mask's index
        int layerMask = 1 << 8;
        // Inverts layermask by using complement operator, because we want to IGNORE layer 8.
        layerMask = ~layerMask; 
        */

        // Creates layermask by using
        LayerMask layerMask = LayerMask.GetMask("TankBodies", "Tanks");
        // Inverts layermask because we want to ignore the aforementioned layers
        layerMask = ~layerMask;

        // Whether the object is obstructed
        bool objectIsObstructed = Physics.Linecast(sampledPosition.position, targetTransform.position, layerMask, QueryTriggerInteraction.Ignore);

        // Returns true if the navmesh could sample and the linecast did not encounter any obstruction
        if (navMeshSampleSucceeded == true && objectIsObstructed == false)
        {
            // Updates destination validity for debug purposes
            destinationIsValid = true;
            return true;
        }
        else
        {
            // Updates destination invalidity for debug purposes
            destinationIsValid = false;
            return false;
        }
    }

    // Tries getting a new point until it is valid
    private Vector3 GetValidPoint(EnemyController enemy, float tryIncrement)
    {
        // Declares a variable to store whether the point is valid
        bool pointIsValid = false;
        // Declares a vector3 to store the point
        Vector3 validPoint = new Vector3(0f, 0f, 0f);
        Vector3 tryOffset = new Vector3(0f, 0f, 0f);
        // The number of the try
        int tryCount = 0;
        // Creates variable to store point angle in relation to player transform
        float pointAngle;

        // Sets the valid point to the closest point in the circumference
        validPoint = GetNearestPointInCircumference(enemy.PlayerReference.transform, enemy.PlayerReference.SafeFireAreaRadius, enemy.transform.position, out pointAngle);
        // Checks if the point is valid
        pointIsValid = ValidatePoint(validPoint, enemy.NavigationAgent, enemy.PlayerReference.transform);

        // While the point is not valid
        while (pointIsValid == false && tryCount <= 12)
        {
            // Gets a point in the circumference and 
            validPoint = GetPointInCircumference(enemy.PlayerReference.transform, enemy.PlayerReference.SafeFireAreaRadius, pointAngle + (tryCount * tryIncrement * -1f));

            // Increases the try count
            tryCount++;

            // Checks if point is valid
            pointIsValid = ValidatePoint(validPoint, enemy.NavigationAgent, enemy.PlayerReference.transform);
        }

        // Returns a valid point
        return validPoint;
    }

    #endregion
}

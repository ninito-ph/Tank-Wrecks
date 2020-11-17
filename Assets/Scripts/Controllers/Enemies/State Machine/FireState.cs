using System.Collections;
using UnityEngine;

public class FireState : EnemyBaseState
{
    #region Abstract Implementations

    // Runs upon entering the state
    public override void EnterState(EnemyController enemy)
    {
        // Stops steering
        enemy.NavigationAgent.isStopped = true;
        // Marks the enemy as stationary
        enemy.IsStationary = true;
        // Sets a reference for the coroutine and starts it
        enemy.CurrentStateRoutine = enemy.StartCoroutine(FireAtPlayer(enemy));
    }

    // Runs every frame during the state
    public override void Update(EnemyController enemy)
    {

    }

    // Runs on draw gizmos
    public override void OnDrawGizmos(EnemyController enemy)
    {
        if (enemy.AIDebugMode == true && enemy.PlayerReference != null)
        {
            // Calculates how close to being able to fire the tank is
            float fireReadiness = Mathf.Min(enemy.FireCooldown / enemy.MaxFireCooldown, 1f);

            // Sets draw color to be responsive to fire cooldown
            Gizmos.color = Color.Lerp(new Color(0.5f, 0.5f, 0.5f, 0.5f), new Color(1f, 0.5f, 0f, 1f), fireReadiness);

            // Draws a line pointing to target and a sphere at target
            Gizmos.DrawLine(enemy.transform.position, enemy.PlayerReference.transform.position);
            Gizmos.DrawSphere(enemy.PlayerReference.transform.position, 1f);
        }
    }

    // Runs upon leaving the state
    public override void LeaveState(EnemyController enemy)
    {
        // Stops the aim coroutine
        enemy.StopCoroutine(enemy.CurrentStateRoutine);
    }

    #endregion

    #region Custom Methods

    // Turns the head of the tank towards the player
    private bool AimHeadAtPlayer(EnemyController enemy, float rotationSpeed)
    {
        // The position of the player ignoring the Y component
        Vector3 playerPositionNoY = new Vector3(enemy.PlayerReference.transform.position.x, enemy.TankParts["Head"].transform.position.y, enemy.PlayerReference.transform.position.z);
        // The facing rotation between the player and the tank's head
        Quaternion targetRotation = Quaternion.LookRotation(playerPositionNoY - enemy.TankParts["Head"].transform.position);

        // Rotates head
        // Checks if the rotation is not already the target rotation before running lerp
        if (enemy.TankParts["Head"].transform.rotation != targetRotation)
        {
            // The time point in the interpolation
            float rotationTime = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            // Sets the rotation to the lerp
            enemy.TankParts["Head"].transform.rotation = Quaternion.Lerp(enemy.TankParts["Head"].transform.rotation, targetRotation, rotationTime);

            // Returns false to signify the tank has not finished aiming its head
            return false;
        }
        else
        {
            // Returns true to signify the tank has finished aiming its head
            return true;
        }
    }

    // Inclines the cannon of the tank towards the player
    private bool AimCannonAtPlayer(EnemyController enemy, float rotationSpeed)
    {
        // Gets the range the projectile needs to have to land on the player
        float projectileRange = Vector3.Distance(enemy.transform.position, enemy.PlayerReference.transform.position);
        // Caches launch data, launch speed velocity squared and launch gravity for brevity
        ProjectileLaunchData launch = enemy.LaunchData;
        float velocitySquared = launch.launchVelocity.magnitude * launch.launchVelocity.magnitude;
        float gravity = launch.projectileGravity;

        // Calculates at what angle the projectile should be launched at
        float cannonAngle = -1f * GetLaunchAngle(new Vector3(0f, launch.projectileGravity, 0f), launch.launchVelocity.magnitude, enemy.PlayerReference.transform.position, enemy.TankParts["Fire Transform 1"].transform.position);

        // Stores the target rotation
        Quaternion targetRotation = Quaternion.Euler(cannonAngle, 0f, 0f);

        // Rotates the cannon anchor
        // Checks if the rotation is not already the target rotation before running lerp
        if (enemy.TankParts["Cannon Anchor"].transform.localRotation != targetRotation)
        {
            // The time point in the interpolation
            float rotationTime = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            // Sets the rotation to the lerp
            enemy.TankParts["Cannon Anchor"].transform.localRotation = Quaternion.Lerp(enemy.TankParts["Cannon Anchor"].transform.localRotation, targetRotation, rotationTime);

            // Returns false to signify the tank has not finished aiming his its cannon
            return false;
        }
        else
        {
            // Returns true to signify the tank has finished aiming its cannon
            return true;
        }
    }

    // Returns the angle at which a projectile must be launched to achieve a given range with a given speed
    private float GetLaunchAngle(Vector3 gravity, float launchSpeed, Vector3 targetPosition, Vector3 originPosition)
    {
        // Calculates the delta between the target and the gun origin
        Vector3 targetPositionDelta = targetPosition - originPosition;
        // Gets Y distance of target
        float yOffset = targetPositionDelta.y;
        // Reset Y so we can get the horizontal distance x
        targetPositionDelta.y = 0f;
        // Gets horizontal distance
        float xOffset = targetPositionDelta.magnitude;

        // Calculate the angles
        // Squares the launch speed for brevity
        float launchSpeedSquared = launchSpeed * launchSpeed;
        // Calculates the discriminant of the quadratic equation
        float discriminant = (launchSpeedSquared * launchSpeedSquared) - gravity.magnitude * (gravity.magnitude * xOffset * xOffset + 2 * yOffset * launchSpeedSquared);

        // Calculates the shortest (fastest) possible arc 
        float shortArc = launchSpeedSquared - Mathf.Sqrt(discriminant);

        float bottom = gravity.magnitude * xOffset;

        // Returns launch angle theta
        return Mathf.Atan2(shortArc, bottom) * Mathf.Rad2Deg;
    }

    // Checks if the player is being obstructed by something
    private bool CheckIfObstructed(EnemyController enemy)
    {
        // Creates layermask by using
        LayerMask layerMask = LayerMask.GetMask("TankBodies", "Tanks", "Projectiles");
        // Inverts layermask because we want to ignore the aforementioned layers
        layerMask = ~layerMask;

        // Whether the object is obstructed
        bool isObstructed = Physics.Linecast(enemy.TankParts["Head"].transform.position, enemy.PlayerReference.transform.position, layerMask, QueryTriggerInteraction.Ignore);
        return isObstructed;
    }

    #region Coroutines

    // Coroutine that aims and fires at the player
    private IEnumerator FireAtPlayer(EnemyController enemy)
    {
        // Resets fire cooldown
        enemy.FireCooldown = 0;

        // Loops permanently
        while (enemy.PlayerReference != null)
        {
            // Stores the distance between the enemy and the player
            float distance = Vector3.Distance(enemy.PlayerReference.transform.position, enemy.transform.position);
            // The error margin to compare the distance
            float errorMargin = 1f;

            // Checks if the tank is within the fire range (inside fire range but ouside rush range), and if the player is obstructed by anything
            if (distance + errorMargin > enemy.PlayerReference.FireAreaRadius || distance + errorMargin < enemy.PlayerReference.RushAreaFireRadius || CheckIfObstructed(enemy) == true)
            {
                // Transitions to the reposition state
                enemy.TransitionToState(enemy.RepositionState);
            }

            // Aims head and cannon at player
            AimCannonAtPlayer(enemy, enemy.AimSpeeds.x);
            AimHeadAtPlayer(enemy, enemy.AimSpeeds.y);

            // Fire shell if cooldown is over & game is unpaused
            if (enemy.FireCooldown >= enemy.MaxFireCooldown && !Mathf.Equals(Time.timeScale, 0f))
            {
                // Makes tank fire
                enemy.TankFire();
            }

            // Waits until the end of next frame
            yield return null;
        }

        yield break;
    }

    #endregion

    #endregion
}

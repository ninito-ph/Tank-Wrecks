using UnityEngine;

/// <summary>
/// Contains the information about a projectile's launch necessary to calculate its trajectory
/// </summary>
public struct ProjectileLaunchData
{
    /// <summary>
    /// The velocity at which the projectile is launched
    /// </summary>
    public Vector3 launchVelocity;
    /// <summary>
    /// The gravity affecting the projectile during its flight
    /// </summary>
    public float projectileGravity;
    /// <summary>
    /// The launch angle of the projectile
    /// </summary>
    public float launchAngle;
    /// <summary>
    /// How far from the ground the projectile was launched from
    /// </summary>
    public float launchHeight;

    /// <summary>
    /// Constructs the projectile launch data
    /// </summary>
    /// <param name="launchVelocity">The velocity at which the projectile is launched</param>
    /// <param name="projectileGravity">The gravity affecting the projectile during its flight</param>
    /// <param name="launchAngle">The launch angle of the projectile</param>
    /// <param name="launchHeight">How far from the ground the projectile was launched from</param>
    public ProjectileLaunchData(Vector3 launchVelocity, float projectileGravity, float launchAngle, float launchHeight)
    {
        // Initializes the struct's variables
        this.launchVelocity = launchVelocity;
        this.projectileGravity = projectileGravity;
        this.launchAngle = launchAngle;
        this.launchHeight = launchHeight;
    }

    /// <summary>
    /// Gets the "splash" of the projectile. Splash is the artillery term for projectile airtime.
    /// </summary>
    /// <param name="timeStep">The lenght of the timestep for the physics calculation. Numbers too large will be innacurate, as will numbers too small due to rounding innacuracies.</param>
    /// <returns></returns>
    public float GetProjectileSplash(float timeStep)
    {
        // Stores the current velocity and position
        Vector3 currentVelocity = launchVelocity;
        Vector3 currentPosition = new Vector3(0f, launchHeight, 0f);

        // Stores the new position and velocity
        Vector3 newVelocity = Vector3.zero;
        Vector3 newPosition = Vector3.zero;

        // The time it will take to reach target (splash)
        float splash = 0f;

        // Limit time to prevent infinite loops
        for (splash = 0f; splash < 30f; splash += timeStep)
        {
            // The bullet's position is calculted using numerical integration,
            // more specifically, backwards euler numerical integration. This
            // method is not identical, but almost undistinguishable from what
            // Unity uses. This yields the best results for rigidbody projectile
            // motion prediction.
            BackwardsEuler(timeStep, currentPosition, currentVelocity, out newPosition, out newVelocity);

            // If we are moving downwards and are below ground level, we have
            // obligatorily hit something
            if (newPosition.y < currentPosition.y && newPosition.y < 0f)
            {
                // Adds twice the timestep to to make sure ground is reached
                splash += timeStep * 2f;
                // Ends loop prematurely to save resources
                break;
            }

            // Saves predicted position and velocity
            currentPosition = newPosition;
            currentVelocity = newVelocity;
        }

        // Returns the projectile's splash
        return splash;
    }

    /// <summary>
    /// Numerical integration method to simulate projectile motion. Backwards Euler is very similar to what is used in Unity, and thus yields the best results in this situation
    /// </summary>
    /// <param name="timeStep">The lenght of the timestep for the physics calculation. Numbers too large will be innacurate, as will numbers too small due to rounding innacuracies.</param>
    /// <param name="currentPosition">The current position of the projectile.</param>
    /// <param name="currentVelocity">The current velocity of the projectile.</param>
    /// <param name="newPosition">The output position of the projectile.</param>
    /// <param name="newVelocity">The output velocity of the projectile.</param>
    public void BackwardsEuler(float timeStep, Vector3 currentPosition, Vector3 currentVelocity, out Vector3 newPosition, out Vector3 newVelocity)
    {
        // Gets accelerations acting on the projectile
        Vector3 accelerationForces = Physics.gravity;

        // Calculates the new position and acceleration accourding to timestep
        newVelocity = currentVelocity + timeStep * accelerationForces;
        newPosition = currentPosition + timeStep * newVelocity;
    }
}
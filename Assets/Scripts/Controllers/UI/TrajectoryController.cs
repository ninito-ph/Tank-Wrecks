using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryController : MonoBehaviour
{
    #region Field Declarations

    [SerializeField]
    [Tooltip("The amount of samples the trajectory preview line will display")]
    private int sampleCount;

    private Vector3 finalPosition = new Vector3(0, 0, 0);

    // The line renderer component cache
    private LineRenderer lineRenderer;
    // The reference to the tank who fired the projectile
    private TankBase tank;
    // The coroutine responsile for updating the trajectory
    private Coroutine updateRoutine;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        // Gets the tank who fired the projectile
        tank = transform.root.GetComponent<TankBase>();
        // Gets the line renderer's component cache
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        // Starts the update routine
        updateRoutine = StartCoroutine(UpdateTrajectory());
    }

    #endregion

    #region Coroutines
    // Coroutine to update the trajectory
    private IEnumerator UpdateTrajectory()
    {
        // Loops eternally
        while (true)
        {
            // Retrieves the launch info from the tank
            ProjectileLaunchData launchData = tank.LaunchData;

            // Updates the trajectory preview
            UpdateTrajectoryPreview(launchData);

            // Waits until the next frame
            yield return null;
        }
    }
    #endregion


    #region Custom Methods

    /// <summary>
    /// Updates the line renderer with the trajectory's preview
    /// </summary>
    /// <param name="launchData">Information about the projectile's launch</param>
    private void UpdateTrajectoryPreview(ProjectileLaunchData launchData)
    {
        // Updates the line renderer's position count and passes it an array of
        // vectors containing the sampled positions
        lineRenderer.positionCount = sampleCount + 1;
        lineRenderer.SetPositions(GetTrajectoryPreview(sampleCount, launchData));
    }

    /// <summary>
    /// Gets the preview of a projectile's trajectory
    /// </summary>
    /// <param name="samples">The number of times to sample the trajectory</param>
    /// <param name="launchData">The information about the projetile's launch</param>
    /// <returns>A Vector3 array containing the points sampled</returns>
    private Vector3[] GetTrajectoryPreview(int samples, ProjectileLaunchData launchData)
    {
        // The array containing the sampled points
        Vector3[] previewPoints = new Vector3[samples + 1];

        // Gets the interval in time in the trajectory between samples
        float sampleTimeInverval = launchData.GetProjectileAirTime() / samples;

        // Gathers samples progressively until it reaches the end of the projectile range
        for (int sample = 0; sample < previewPoints.Length; sample++)
        {
            float sampleTime = sampleTimeInverval * sample;
            previewPoints[sample] = GetTrajectoryPoint(launchData.launchVelocity, sampleTime, launchData.projectileGravity);
        }

        // Returns the sampled points aray
        return previewPoints;
    }

    /// <summary>
    /// Gets point in projectile trajectory accouring to time
    /// </summary>
    /// <param name="launchVelocity">The launch velocity of the projectile</param>
    /// <param name="sampleTime">The time at which to take the sample of the trajectory</param>
    /// <param name="projectileGravity">The gravity affecting the projectile</param>
    /// <returns></returns>
    private Vector3 GetTrajectoryPoint(Vector3 launchVelocity, float sampleTime, float projectileGravity)
    {
        // Gets the X, Y and Z positions of the sample
        float positionX = (launchVelocity.x * sampleTime);
        float positionY = (launchVelocity.y * sampleTime) - (projectileGravity * Mathf.Pow(sampleTime, 2) / 2);

        // Returns positions in a Vector3
        return new Vector3(0f, positionY, positionX);
    }

    #endregion
}

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
    /// Calculates how long for the projectile will remain on the air
    /// </summary>
    /// <returns>How long the projectile will remain on the air for</returns>
    public float GetProjectileAirTime()
    {
        // Calculates the quadractic equation necessary to define the airtime
        float airTime = (launchVelocity.y + Mathf.Sqrt((launchVelocity.y * launchVelocity.y) + 2 * projectileGravity * launchHeight)) / projectileGravity;

        // Returns the airtime
        return airTime;
    }
}
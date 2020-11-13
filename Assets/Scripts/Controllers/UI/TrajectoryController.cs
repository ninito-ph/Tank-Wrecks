using System.Collections;
using UnityEngine;

public class TrajectoryController : MonoBehaviour
{
    #region Field Declarations

    // The speed at which the projectile is launched
    private ProjectileLaunchData launchData;
    // The line renderer component cache
    private LineRenderer lineRenderer;
    // The coroutine responsile for updating the trajectory
    private Coroutine updateRoutine;

    public ProjectileLaunchData LaunchData { get => launchData; set => launchData = value; }

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
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
        // Caches WaitFor class
        WaitForFixedUpdate waitFixedFrame = new WaitForFixedUpdate();

        // Loops eternally
        while (true)
        {
            // Updates the trajectory preview
            DrawTrajectoryPreview();

            // Waits until the next frame
            yield return waitFixedFrame;
        }
    }
    #endregion

    #region Custom Methods

    private void DrawTrajectoryPreview()
    {
        // Gets the timestep
        float timeStep = Time.fixedDeltaTime * 1f;
        // Calculates the target splash
        float targetSplash = LaunchData.GetProjectileSplash(timeStep);

        // Calculates the amount of necessary segments in the line
        int positionCount = Mathf.RoundToInt(targetSplash / timeStep);
        // Updates linerenderer's position amount
        lineRenderer.positionCount = positionCount;

        // Initializes necessary values to keep track of projectile path
        Vector3 currentVelocity = LaunchData.launchVelocity;
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = Vector3.zero;
        Vector3 newVelocity = Vector3.zero;

        // Calculates and draws the trajectory line
        for (int positionIndex = 0; positionIndex < positionCount; positionIndex++)
        {
            // Gives the linerenderer the current position
            lineRenderer.SetPosition(positionIndex, currentPosition);
            // Uses backwards euler method to increment position
            LaunchData.BackwardsEuler(timeStep, currentPosition, currentVelocity, out newPosition, out newVelocity);

            // Updates current position and velocity
            currentPosition = newPosition;
            currentVelocity = newVelocity;
        }
    }

    #endregion
}
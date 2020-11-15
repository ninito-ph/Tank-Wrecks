using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Field Declarations

    // The virtual camera of the scene
    private CinemachineVirtualCamera virtualCamera;
    // The multi channel perlin noise
    private CinemachineBasicMultiChannelPerlin perlinNoise;
    // The transform of the player's tank
    private Transform tankTransform;

    [Header("Camera Shake")]
    [Tooltip("The magnitude of the camera's shake")]
    [SerializeField]
    private float traumaMagnitude;
    [Tooltip("How fast the camera stops shaking (recovers from trauma)")]
    [SerializeField]
    private float recoverySpeed;
    [Tooltip("How far away from, in units, something can induce trauma from")]
    [SerializeField]
    private float traumaRange;

    [Header("Debug Info")]
    [SerializeField]
    private bool displayTraumaRange;

    // How much the camera is shaking at a given moment
    private float trauma = 0f;

    #endregion

    #region Unity Methods

    // Runs once before start
    private void Awake()
    {
        // Caches virtualcamera component
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        // Stores the perlin noise channels
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        // Subscribes InduceStress to InduceStress event
        EventBroker.ShakeCamera += InduceStress;
    }

    // Runs once per frame
    private void Update()
    {
        // Reduces trauma with time
        trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);

        // Applies the perlin noise update if necessary
        if (perlinNoise.m_AmplitudeGain != trauma * traumaMagnitude)
        {
            // Applies the shake to the cinemachine camera
            perlinNoise.m_AmplitudeGain = trauma * traumaMagnitude;
        }
    }

    // Runs when the editor draws the gizmos
    private void OnDrawGizmos()
    {
        // If the debug trauma range is enabled
        if (displayTraumaRange)
        {
            // Sets color to red-orange-ish
            Gizmos.color = new Color(0.89f, 0.19f, 0.13f, 1f);
            // Draws a wire sphere
            Gizmos.DrawWireSphere(transform.position, traumaRange);
        }
    }

    // Runs once when the object is destroyed
    private void OnDestroy()
    {
        // Unsubscribes from shake camera event
        EventBroker.ShakeCamera -= InduceStress;
    }

    #endregion

    #region Custom Methods

    // Induces stress on the camera causing it to shake
    private void InduceStress(float stress, Vector3 stressOrigin)
    {
        // Calculates the distance to the camera
        float stressDistance = Vector3.Distance(transform.position, stressOrigin);

        // The proportion of the distance compared to the trauma range
        float distanceProportional = Mathf.Clamp01(stressDistance / traumaRange);

        // Induces stress with quadractic falloff
        stress = (1 - Mathf.Pow(distanceProportional, 2)) * traumaMagnitude;

        // Adds stress to the trauma
        trauma += stress;
    }

    #endregion
}

using UnityEngine;

public class ExplosionGenerator : MonoBehaviour
{
    [Header("Particle System Prefabs")]
    [SerializeField]
    [Tooltip("The particle system prefab for ground explosions")]
    private ParticleSystem aerialExplosion;
    [SerializeField]
    [Tooltip("The particle system prefab for aerial explosions")]
    private ParticleSystem groundExplosion;

    [Header("Sounds")]
    [SerializeField]
    [Tooltip("The array containing all possible explosion sounds")]
    private AudioClip[] explosionSounds;
    [SerializeField]
    [Tooltip("Whether the explosion emits any sound at all")]
    private bool muteExplosion = false;
    private AudioSource explosionSoundSource;

    [Header("Explosion settings")]
    [SerializeField]
    [Tooltip("How much the explosion will shake the camera")]
    private float explosionForce;
    [SerializeField]
    [Tooltip("The size of the explosion")]
    private float explosionSize = 1f;
    [SerializeField]
    [Tooltip("The overall color of the explosion")]
    private Color explosionColor = new Color(1, 1, 1, 1);
    // The duration of the particle system
    private float explosionDuration;

    public Color ExplosionColor { get => explosionColor; set => explosionColor = value; }

    // Start is called before the first frame update
    private void Start()
    {
        // If the explosion isn't mute
        if (muteExplosion == false)
        {
            // Caches audiosource component
            explosionSoundSource = GetComponent<AudioSource>();

            // Plays one of the possible explosion sounds
            explosionSoundSource.clip = explosionSounds[Random.Range(0, explosionSounds.Length)];
            // Randomizes pitch to increase sound variance
            explosionSoundSource.pitch = Random.Range(1f, 1.3f);
            // Makes the sound source play its sound
            explosionSoundSource.Play();
        }

        // Shakes camera
        if (explosionForce > 0)
        {
            EventBroker.CallShakeCamera(explosionForce, transform.position);
        }

        // If the position is close to the ground, play the ground variant particle system.
        if (transform.position.y > 0.5)
        {
            // Plays aerial particle explosion
            TriggerExplosion(aerialExplosion);
        }
        else
        {
            // Plays ground particle explosion
            TriggerExplosion(groundExplosion);
        }

        // Gets how long the explosion lasts
        explosionDuration = Mathf.Max(aerialExplosion.main.duration, groundExplosion.main.duration);

        // Destroys the gameobject after the particle system finishes playing
        Destroy(gameObject, explosionDuration);
    }

    // Triggers an explosion particle system
    private void TriggerExplosion(ParticleSystem explosionSystem)
    {
        // Plays particle explosion
        explosionSystem.gameObject.SetActive(true);
        // Gets main particle system module and changes color
        ParticleSystem.MainModule mainModule = explosionSystem.main;
        mainModule.startColor = ExplosionColor;
        // Changes scale of explosion and plays
        explosionSystem.gameObject.transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize);
        explosionSystem.Play(true);
    }
}

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
        EventBroker.CallShakeCamera(explosionForce, transform.position);

        // If the position is close to the ground, play the ground variant particle system.
        if (transform.position.y > 0.5)
        {
            // Plays aerial particle explosion
            aerialExplosion.gameObject.SetActive(true);
            aerialExplosion.gameObject.transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize);
            aerialExplosion.Play(true);
        }
        else
        {
            // Plays ground particle explosion
            groundExplosion.gameObject.SetActive(true);
            groundExplosion.gameObject.transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize);
            groundExplosion.Play(true);
        }

        // Destroys the gameobject in five seconds
        Destroy(gameObject, 5f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShell : MonoBehaviour
{
    [Header("Properties")]
    [Tooltip("The shell explosion radius")]
    public float m_explosionRadius = 100f;
    [Tooltip("The damage in hearts the shell's explosion will deal")]
    public int m_explosionDamage = 1;
    [Tooltip("The speed at which the shell will fly through the air")]
    public float shellSpeed = 20f;

    // The shell's rb reference
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        // Gets the rigidbody component
        rb = GetComponent<Rigidbody>();
        // Applies force on local space, based on shellSpeed variable
        rb.AddRelativeForce(shellSpeed * Vector3.up, ForceMode.Impulse);
    }

    // OnCollisionEnter is called whenever the GameObject collides with something
    void OnCollisionEnter(Collision other) 
    {
        // If the colliding object isnt the player (This prevents the shells from exploding on contact with the player cannon)
        if (!other.gameObject.CompareTag("Player"))
        {
            // Destroys the shell object
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Gets shell velocity
        Vector3 velocityPointing = rb.velocity;
        // Gets creates a rotation Vector3 that points towards the velocity. It also adds 90 degrees of rotation to the x axis, so the tank shell is flying with its pointy end
        Vector3 pointing = Quaternion.LookRotation(velocityPointing).eulerAngles + new Vector3(90, 0, 0);
        // Applies the rotation
        transform.rotation = Quaternion.Euler(pointing);
    }
}

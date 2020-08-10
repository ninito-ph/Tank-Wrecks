using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBodyController : MonoBehaviour
{
    [Tooltip("The speed at which the tank turns.")]
    public float tankTurnRate = 30f;
    [Tooltip("The max velocity at which the tank can move.")]
    public float maxSpeed = 5f;
    [Tooltip("How fast the tank accelerates")]
    public float acceleration = 3f;
    [HideInInspector]
    // The speed at which the tank is moving
    public float tankSpeed = 0f;


    // The tank's rigidbody reference
    private Rigidbody tankBody;
    // An array containing the colliders of children objects (head, cannon)
    private Component[] childrenColliders;
    // Whether the tank is moving frontally
    private int moveFrontal;
    // Whether the tank is moving laterally
    private int moveLateral;


    // Start is called before the first frame update
    void Start()
    {
        // Get main rigidbody reference
        tankBody = GetComponent<Rigidbody>();

        // Fill childrenColliders array with Collider references from children objects
        childrenColliders = GetComponentsInChildren<Collider>();

        // For every collider in the childrenColliders array
        foreach (Collider col in childrenColliders)
        {
            // If the collider is not our own
            if (col != GetComponent<Collider>())
            {
                // Ignore collisions between said collider and our own
                Physics.IgnoreCollision(col, GetComponent<Collider>());
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Input Handling
        // Alters moveFrontal value from 1 to -1 and vice versa depending on whether frontal movement keys are being pressed.
        // TODO: Make remappable controls
        if (Input.GetKey(KeyCode.W))
        {
            moveFrontal = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveFrontal = -1;
        }
        else
        {
            moveFrontal = 0;
        }

        // Alters moveLateral value from 1 to -1 and vice versa depending on whether lateral movement keys are being pressed.
        // TODO: Make remappable controls
        if (Input.GetKey(KeyCode.D))
        {
            moveLateral = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveLateral = -1;
        }
        else
        {
            moveLateral = 0;
        }

        // Acceleration handling
        // Checks to see if the acceleration increase/decrease would make the speed value go over the maximum speed value.
        // It also checks whether the move keys are being pressed at all. If not, it doesn't update the speed, so as to not waste computational power.
        if (Mathf.Abs((tankSpeed + (acceleration * moveFrontal * Time.deltaTime))) < maxSpeed && moveFrontal != 0)
        {
            // The acceleration value, multiplied by the direction the player is accelerating towards, multiplied by delta time, is added to the tank's speed.
            tankSpeed = tankSpeed + (acceleration * moveFrontal * Time.deltaTime);
        }
        // If no frontal keys are bring pressed, the tank will deaccelerate until it halts.
        if (moveFrontal == 0 && tankSpeed != 0)
        {
            // Checks to see if current speed is smaller than the current frame's deacceleration value.
            if (Mathf.Abs(tankSpeed + (acceleration * Time.deltaTime) * Mathf.Sign(tankSpeed) * -1) <= Mathf.Abs(acceleration * Time.deltaTime))
            {
                // Halt tank entirely
                tankSpeed = 0f;
            }
            else
            {
                // Deaccelerates the tank by adding negative acceleration
                tankSpeed = tankSpeed + (acceleration * Time.deltaTime) * Mathf.Sign(tankSpeed) * -1;
            }
        }

        //Movement
        //The tank moves based on its speed multiplied by delta time, and it turns based on its turn rate multiplied by delta time. The tank cannot turn if it is stationary.
        transform.Translate(Time.deltaTime * tankSpeed * -1, 0, 0, Space.Self); // X component is multiplied by -1, because the model's axes are inverted
        transform.Rotate(0, tankTurnRate * Time.deltaTime * moveLateral * moveFrontal, 0);
    }

    // Fixed update is called a fixed number of frames
    // void FixedUpdate()
    // {
    //     Debug.Log(Time.deltaTime * acceleration);
    //     Debug.Log(tankBody.velocity);

    //     if (Mathf.Abs(tankBody.velocity.x) <= maxSpeed && Mathf.Abs(tankBody.velocity.y) <= maxSpeed)
    //     {
    //         Debug.Log("True");
    //         tankBody.AddForce(new Vector3(Time.deltaTime * acceleration * -1 * moveFrontal, 0, 0));
    //     }

    // }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankHeadController : MonoBehaviour
{
    [Tooltip("The speed at which the tank head turns.")]
    public float headTurnRate = 30f;
    [Tooltip("The limit amount in euler angles the tank head can turn.")]
    public float turnMax = 120f;
    [HideInInspector]
    // The angle the head is currently rotated at
    public float turnAngle = 0f;

    // The direction the head is turning towards. -1 means left, 1 means right.
    private int turnDirection;
    // An array to store children collider references (cannon)
    private Collider[] childrenColliders;

    // Start is called before the first frame update
    void Start()
    {
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
        // Checks to see if the arrow keys are being pressed, and if the current turn angle does not exceed the maximum turn angle. If so, update the turn angle and the turn direction.
        // TODO: Make remappable controls
        if (Input.GetKey(KeyCode.LeftArrow) && turnAngle > turnMax * -1)
        {
            turnAngle = turnAngle - headTurnRate * Time.deltaTime;
            turnDirection = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && turnAngle < turnMax)
        {
            turnAngle = turnAngle + headTurnRate * Time.deltaTime;
            turnDirection = 1;
        }
        else
        {
            turnDirection = 0;
        }

        // Rotates the tank head on the Y axis, based on the current TurnDirection.
        transform.Rotate(0, headTurnRate * Time.deltaTime * turnDirection, 0);
    }
}

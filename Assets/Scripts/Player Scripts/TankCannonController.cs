using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCannonController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The speed at which the tank cannon rises/falls.")]
    public float cannonInclineRate = 30f;
    [Tooltip("The range in euler angles the tank's cannon can rise and stoop, respectively.")]
    public Vector2 inclineRange = new Vector2(25f, -50f);

    [Header("Firing")]
    [Tooltip("The curve controlling the cannon recoil from each shot.")]
    public AnimationCurve cannonRecoil;
    [Tooltip("The recoil amount applied to the cannon")]
    public float cannonRecoilAmount = 0.3f;
    [Tooltip("The recoil acceleration applied to the tank's body.")]
    public float bodyRecoil = 3f;
    [Tooltip("The cooldown in seconds the player must wait before firing again.")]
    public float reloadTime = 2.5f;
    [Tooltip("The object reference of the player's tank body.")]
    public TankBodyController tankBody;
    [Tooltip("The object reference of the player's tank head.")]
    public TankHeadController tankHead;
    [Tooltip("The player controller object reference")]
    public PlayerController playerController;
    [Tooltip("The object reference for the cannon anchor")]
    public GameObject cannonAnchor;
    [Tooltip("Tank shell GameObject reference")]
    public GameObject tankShell;
    [Tooltip("The FireTransform GameObject reference")]
    public GameObject fireTransform;

    // The angle the cannon is currently rotated at
    private float inclineAngle = 0f;
    // The direction the cannon is inclining towards. -1 means up, 1 means down.
    private int inclineDirection;
    // The time in seconds before the tank can shoot again.
    [HideInInspector]
    public float reloadCooldown = 2.5f;
    // Whether the cannon fire animation has started.
    private bool cannonAnimStarted = false;
    // The current time of the cannon animation.
    private float cannonAnimTime = 0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Checks to see if the arrow keys are being pressed, and if the current turn angle does not exceed the maximum/minimum incline angle. If so, update the incline angle and the incline direction.
        // TODO: Make remappable controls
        if (Input.GetKey(KeyCode.UpArrow) && inclineAngle > inclineRange.y)
        {
            inclineAngle = inclineAngle - cannonInclineRate * Time.deltaTime;
            inclineDirection = -1;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && inclineAngle < inclineRange.x)
        {
            inclineAngle = inclineAngle + cannonInclineRate * Time.deltaTime;
            inclineDirection = 1;
        }
        else
        {
            inclineDirection = 0;
        }
        // Rotates the tank head on the X axis, based on the current inclineDirection.
        cannonAnchor.transform.Rotate(0, 0, cannonInclineRate * Time.deltaTime * inclineDirection);

        // Firing
        // If the firing key is being pressed, if reload cooldown is below zero, if ammo is greater than zero, perform firing action
        if (Input.GetKey(KeyCode.Space) && reloadCooldown >= reloadTime && playerController.m_Ammo > 0)
        {
            // Assign transform to shell
            Vector3 shellOrigin = new Vector3(fireTransform.transform.position.x, fireTransform.transform.position.y, fireTransform.transform.position.z);
            Vector3 shellRotation = new Vector3(cannonAnchor.transform.rotation.eulerAngles.x, cannonAnchor.transform.rotation.eulerAngles.y, cannonAnchor.transform.rotation.eulerAngles.z + 90);
            // Create shell based on transform
            Instantiate(tankShell, shellOrigin, Quaternion.Euler(shellRotation));
            // Set cooldown
            reloadCooldown = 0;
            // Diminish ammo by one
            playerController.m_Ammo --;

            // Apply body recoil
            int recoilDirection = 1;
            if (Mathf.Abs(tankHead.turnAngle) >= 90)
            {
                recoilDirection = -1;
            }
            tankBody.tankSpeed = tankBody.tankSpeed + (bodyRecoil * -1) * recoilDirection;

            // Apply cannon recoil
            cannonAnimStarted = true;

        }
        // Tick up reloadCooldown timer
        // This cooldown timer unusually ticks up instead of down, to facilitate the implementation of the fire readiness icon.
        reloadCooldown = reloadCooldown + (1 * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // If the cannon animation flag is true
        if (cannonAnimStarted)
        {
            // Move the cannon animation time forward
            cannonAnimTime += Time.deltaTime;
            // Translate the cannon's position accourding to the cannonRecoil accel curve
            transform.Translate(cannonRecoil.Evaluate(cannonAnimTime) * cannonRecoilAmount * Time.deltaTime, 0, 0, Space.Self);
        }

        // 2 is the animation curve's lenght
        // If the animation exceeds the cannonRecoil accel curve's lenght
        if (cannonAnimTime >= 2)
        {
            // End the animation by setting its flag to false
            cannonAnimStarted = false;
            // Reset the animation time
            cannonAnimTime = 0f;
        }

    }
}

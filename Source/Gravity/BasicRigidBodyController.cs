using FlaxEditor.Surface.Archetypes;
using FlaxEngine;
using System.ComponentModel;

#if USE_LARGE_WORLDS
using Real = System.Double;
#else
using Real = System.float;
using Mathr = FlaxEngine.Mathf;
#endif

namespace Gravity;

/// <summary>
/// BasicRigidBodyController Script.
/// </summary>
public class BasicRigidBodyController : GravityObject
{

    /// <summary>
    /// Camera rotation smoothing factor
    /// </summary>
    [DefaultValue(20.0f)]
    public float CameraSmoothing { get; set; } = 20.0f;

    /// <summary>
    /// The maximum on-foot movement speed in cm/s
    /// </summary>
    public Vector3 MaxFootSpeed { get; set; } = Vector3.One * 1000;

    /// <summary>
    /// The maximum on-foot movement acceleration in cm/s
    /// </summary>
    public Vector3 MaxFootAcceleration { get; set; } = Vector3.One * 1000;

    // Layers upon which the controller will be considered grounded
    public LayersMask GroundLayers { get; set; }

    /// <summary>
    /// Whether or not the player's feet are touching the ground
    /// </summary>
    [ShowInEditor]
    public bool IsGrounded
    {
        get
        {
            return Physics.SphereCast(center: this.Actor.Position, radius: 5, direction: this.RigidBody.Transform.Down, layerMask: this.GroundLayers,maxDistance: 5);
        }
    }

    private float pitch = 0;

    private float yaw = 0;

    private float roll = 0;

    // Prefab components
    // TODO: update with RequireChildActor attribute

    // private RigidBody rigidBody;
    private Collider collider;

    private Camera viewCamera;

    private StaticModel head;


    /// <inheritdoc/>
    public override void OnStart()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update

        this.RigidBody = this.Actor.As<RigidBody>();
        this.collider = this.Actor.GetChild<Collider>();
        this.viewCamera = this.Actor.GetChild<Camera>();

        StaticModel[] staticModels = this.Actor.GetChildren<StaticModel>();
        foreach (StaticModel staticModel in staticModels)
        {
            if (staticModel.Name == "Head")
            {
                this.head = staticModel;
                break;
            }
        }

    }

    /// <inheritdoc/>
    public override void OnEnable()
    {
        SetUpCursor();
        // register for events
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // unregister for events
    }

    private static void SetUpCursor()
    {
        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;
    }


    /// <inheritdoc/>
    public override void OnUpdate()
    {
    }

    public override void OnFixedUpdate()
    {
        this.UpdateRotation();
        this.Move();
    }

    private void UpdateRotation()
    {
        GetRotationInput();
        float rotationFactor = Mathf.Saturate(CameraSmoothing * Time.DeltaTime);
        RotateHead(rotationFactor);
        RotateBody(rotationFactor);
    }

    private void GetRotationInput()
    {
        Float2 viewInputDelta = new Float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        this.pitch = Mathf.Clamp(pitch + viewInputDelta.Y, -88, 88);
        this.yaw += viewInputDelta.X;
    }


    private void RotateHead(float rotationFactor)
    {
        this.head.LocalOrientation = Quaternion.Lerp(this.head.LocalOrientation, Quaternion.Euler(this.pitch, 0, 0), rotationFactor);
    }


    private void RotateBody(float rotationFactor)
    {

        Real strongestAcceleration = this.GetStrongestGravitationalVector(out Real sourceMass).Length / sourceMass;
        Debug.Log("strongestAcceleration " + strongestAcceleration);
        if (this.IsInGravity && strongestAcceleration > 1) // FIXME: This condition is borked.
        {
            this.SelfRight();
        }
        else
        {
            this.Actor.Orientation = Quaternion.Lerp(this.Actor.Orientation, Quaternion.Euler(this.pitch, 0, this.roll), rotationFactor);
        }
        this.Actor.Orientation = Quaternion.Lerp(this.Actor.Orientation, Quaternion.Euler(0, this.yaw, 0), rotationFactor);

        if (this.IsGrounded)
            this.RigidBody.Constraints = RigidbodyConstraints.LockRotationX | RigidbodyConstraints.LockRotationZ;
        else
            this.RigidBody.Constraints &= RigidbodyConstraints.None;
    }

    private void Move()
    {
        Vector3 movementDirection = GetMovementInputDirection();
        this.RigidBody.AddRelativeForce(movementDirection * this.MaxFootAcceleration, mode: ForceMode.Acceleration);

        if (this.RigidBody.LinearVelocity.Absolute.Length > this.MaxFootSpeed.Length)
            this.RigidBody.LinearVelocity = Vector3.Clamp(this.RigidBody.LinearVelocity, min: -this.MaxFootSpeed, max: this.MaxFootSpeed);
    }

    /// <returns>the movement input direction, in local space</returns>
    private Vector3 GetMovementInputDirection()
    {
        Vector3 movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        return movementDirection;
    }
}

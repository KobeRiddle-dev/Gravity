using System;
using System.Collections.Generic;
using FlaxEngine;



#if USE_LARGE_WORLDS
using Real = System.Double;
using Mathr = FlaxEngine.Mathd;
using System.Linq;
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
    public float CameraSmoothing { get; set; } = 20.0f;

    /// <summary>
    /// The maximum on-foot movement speed in cm/s
    /// </summary>
    public Vector3 MaxFootSpeed { get; set; } = Vector3.One * 1000;

    /// <summary>
    /// The maximum on-foot movement acceleration in cm/s
    /// </summary>
    public Vector3 MaxFootAcceleration { get; set; } = Vector3.One * 1000;

    [ReadOnly]
    public bool IsInGravity
    {
        get
        {
            return this.GravitySources.Count > 0 || this.rigidBody.PhysicsScene.Gravity.Length > 0;
        }
    }

    private float pitch = 0;

    private float yaw = 0;

    private float roll = 0;


    /// <summary>
    /// Whether or not the player's feet are touching the ground
    /// </summary>
    private bool isGrounded = false;

    // Prefab components
    // TODO: update with RequireChildActor attribute

    private RigidBody rigidBody;
    private Collider collider;

    private Camera viewCamera;

    private StaticModel head;


    /// <inheritdoc/>
    public override void OnStart()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update

        this.rigidBody = this.Actor.As<RigidBody>();
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

    private static void SetUpCursor()
    {
        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;
    }


    /// <inheritdoc/>
    public override void OnDisable()
    {
        // unregister for events
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        this.UpdateRotation();
    }

    public override void OnFixedUpdate()
    {
        this.Move();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision entered with: " + collision.OtherActor.TypeName);
        GravitySource gravitySource = collision.OtherActor.FindScript<GravitySource>();

        if (gravitySource != null)
            GravitySources.Add(gravitySource);
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collision exited from: " + collision.OtherActor.TypeName);
        GravitySource gravitySource = collision.OtherActor.FindScript<GravitySource>();
        if (gravitySource != null)
            GravitySources.Remove(gravitySource);
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
        this.Actor.Orientation = Quaternion.Lerp(this.Actor.Orientation, Quaternion.Euler(0, this.yaw, 0), rotationFactor);

        if (this.IsInGravity)
        {
            this.SelfRight();
        }
    }


    private void SelfRight()
    {
        Vector3 strongestGravitationalPull = this.GetStrongestGravitationalPull();

        Vector3 gravityUp = -strongestGravitationalPull.Normalized;

        float rightingStrength = 0.1f;
        // if (this.isGrounded)
        //     rightingStrength = 1;
            
        Quaternion rightedOrientation = Quaternion.GetRotationFromTo(this.Actor.Transform.Up, gravityUp, Vector3.Zero) * this.Actor.Orientation;

        this.Actor.Orientation = Quaternion.Lerp(this.Actor.Orientation, rightedOrientation, rightingStrength);
    }

    private Vector3 GetStrongestGravitationalPull()
    {
        Vector3 strongestGravitationalPull = this.rigidBody.PhysicsScene.Gravity;

        foreach (GravitySource gravitySource in this.GravitySources)
        {
            Vector3 fromGravitySourceToThis = gravitySource.Transform.Translation - this.Actor.Transform.Translation;

            Real gravitationalForce = GravitySource.GRAVITATIONAL_CONSTANT * (this.rigidBody.Mass * gravitySource.Mass) / fromGravitySourceToThis.LengthSquared; // TODO: Make this a static function to get gravitational force

            Vector3 acceleration = fromGravitySourceToThis.Normalized * gravitationalForce;

            // Find body with strongest gravitational pull 
            if (acceleration.LengthSquared > strongestGravitationalPull.LengthSquared)
            {
                strongestGravitationalPull = acceleration;
            }
        }

        return strongestGravitationalPull;
    }


    private void Move()
    {
        Vector3 movementDirection = GetMovementInputDirection();

        this.rigidBody.AddRelativeForce(movementDirection * this.MaxFootAcceleration, mode: ForceMode.Acceleration);
        if (this.rigidBody.LinearVelocity.Absolute.Length > this.MaxFootSpeed.Length)
            this.rigidBody.LinearVelocity = Vector3.Clamp(this.rigidBody.LinearVelocity, min: -this.MaxFootSpeed, max: this.MaxFootSpeed);
    }

    private Vector3 GetMovementInputDirection()
    {
        Vector3 movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        movementDirection.Normalize();
        // movementDirection = this.Actor.Transform.TransformDirection(movementDirection);

        return movementDirection;
    }
}

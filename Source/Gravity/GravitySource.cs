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
/// GravitySource Script.
/// </summary>
public class GravitySource : Script
{
    // TODO: Move this into a constants class (something editable in a Flax settings editor)
    public const Real GRAVITATIONAL_CONSTANT = 0.0001;


    // Properties //

    // /// <summary>
    // /// The direction of gravity relative to the GravitySource.
    // /// </summary>
    // public Vector3 GravitationalDirection { get => gravitationalDirection; set => gravitationalDirection = value; }
    // private Vector3 gravitationalDirection = -Vector3.One.Normalized;

    // /// <summary>
    // /// The radius within which rigidbodies will be affected by this GravitySource.
    // /// </summary>
    // public Real GravitationalRadius { get => gravitationalRadius; set => gravitationalRadius = value; }
    // private Real gravitationalRadius;

    // TODO: update with RequireChildActor attribute

    public float GForce { get => SurfaceGravity / 980; set => SurfaceGravity = 980 * value; }

    /// <summary>
    /// The length of the gravitational acceleration vector at the surface, in cm/2^2. Earth's is 980 cm/s^2. 
    /// Changing this will update the mass accordingly.
    /// </summary>
    public float SurfaceGravity
    {
        get => surfaceGravity;
        set
        {
            this.surfaceGravity = value;
            UpdateMassBasedOnGravity();
        }
    }
    private float surfaceGravity = 980;

    /// <summary>
    /// The radius at the "surface" of the GravitySource, where the acceleration on other objects from the GravitySource's gravity will equal SurfaceGravity.
    /// </summary>
    public Real SurfaceRadius
    {
        get => surfaceRadius;
        set
        {
            surfaceRadius = value;
            this.UpdateMassBasedOnGravity();
        }
    }
    private Real surfaceRadius;

    /// <summary>
    /// The Mass of the GravitySource.
    /// Changing this will update SurfaceGravity accordingly.
    /// If MassUpdatesToRigidBody is enabled, the mass of the rigidbody associated with the GravitySource will also be updated upon changes to Mass.
    /// </summary>
    public float Mass
    {
        get
        {
            if (this.UseRigidBodyMass && this.rigidBody != null)
                return this.rigidBody.Mass;

            return mass;
        }
        set
        {
            mass = value;
            if (this.UseRigidBodyMass && this.rigidBody != null)
                this.rigidBody.Mass = this.mass;

            UpdateGravityBasedOnMass();
        }
    }
    private float mass;

    /// <summary>
    /// Determines whether a rigidbody the GravitySource script is attached to will have its mass synchronized with the GravitySource's mass. Turning this off is useful if you wish to create less realistic simulations, i.e. a planet with high gravitational force that is small in mass as far as other GravitySources are concerned.
    /// </summary>
    public bool UseRigidBodyMass { get; set ; } = true;

    /// <summary>
    /// If true, and the GravitySource script is attached to a rigidbody, the rigidbody will be mutually attracted to other rigidbodies in it's gravitational volume.
    /// </summary>
    public bool AffectedByMutualGravitation { get; set; } = false;


    private List<RigidBody> rigidBodiesInGravity;

    // Components //
    public RigidBody rigidBody;
    public Collider GravitationalBoundCollider;

    /// <summary>
    /// Calculates and updates the mass of the GravitySource based on its surface gravity and surface radius.
    /// this.Mass = r^2 * surfaceGravity / G
    /// </summary>
    public void UpdateMassBasedOnGravity()
    {
        this.Mass = (float)(this.surfaceGravity * this.SurfaceRadius * this.SurfaceRadius / GRAVITATIONAL_CONSTANT);

        // if (this.UseRigidBodyMass && this.rigidBody != null)
        //     this.rigidBody.Mass = this.mass;
    }

    /// <summary>
    /// Calculates and updates the surface gravity of the GravitySource based on its mass and surface radius.
    /// surfaceGravity = (G * this.Mass) / r^2
    /// </summary>
    public void UpdateGravityBasedOnMass()
    {
        this.surfaceGravity = (float)((GRAVITATIONAL_CONSTANT * this.Mass) / (SurfaceRadius * SurfaceRadius));
    }

    /// <inheritdoc/>
    public override void OnStart()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update
        this.rigidBody = this.Actor.FindActor<RigidBody>();
        // this.gravitationalBoundCollider = this.Actor.GetChild<Collider>();

        this.rigidBodiesInGravity = new List<RigidBody>();

        if (this.GravitationalBoundCollider != null)
            this.GravitationalBoundCollider.IsTrigger = true;
    }

    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
        this.GravitationalBoundCollider.TriggerEnter += this.OnObjectEnterGravity;
        this.GravitationalBoundCollider.TriggerExit += this.OnObjectExitGravity;
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
        this.GravitationalBoundCollider.TriggerEnter -= this.OnObjectEnterGravity;
        this.GravitationalBoundCollider.TriggerExit -= this.OnObjectExitGravity;
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Here you can add code that needs to be called every frame
    }

    public override void OnFixedUpdate()
    {
        this.AttractAllRigidBodiesInGravity();
    }

    private void OnObjectEnterGravity(PhysicsColliderActor collider)
    {
        if (collider.AttachedRigidBody == null)
            return;
        if (this.Actor.GetChildren<PhysicsColliderActor>().Contains(collider))
            return;

        // Debug.Log("Object entered " + this.Actor.Name + "'s gravity: " + collider.AttachedRigidBody.Name);

        GravitySourceTracker gravityObject;
        if (collider.AttachedRigidBody.TryGetScript<GravitySourceTracker>(out gravityObject))
            gravityObject.GravitySources.Add(this);

        if (collider.AttachedRigidBody != null)
            this.rigidBodiesInGravity.Add(collider.AttachedRigidBody);
    }

    private void OnObjectExitGravity(PhysicsColliderActor collider)
    {
        Debug.Log("Object exited " + this.Actor.Name + "'s gravity: " + collider.AttachedRigidBody.Name);

        GravitySourceTracker gravityObject;
        if (collider.AttachedRigidBody.TryGetScript<GravitySourceTracker>(out gravityObject))
            gravityObject.GravitySources.Remove(this);

        if (collider.AttachedRigidBody != null)
            this.rigidBodiesInGravity.Remove(collider.AttachedRigidBody);
    }

    private void AttractAllRigidBodiesInGravity()
    {
        foreach (RigidBody rigidBody in rigidBodiesInGravity)
        {
            if (rigidBody.EnableGravity)
                this.Attract(rigidBody);
        }
    }

    private void Attract(RigidBody rigidBody)
    {
        // Debug.Log("Attracting object: " + rigidBody.Name);


        Vector3 fromRigidBodyToThis = rigidBody.Transform.Translation - this.Actor.Transform.Translation;

        Real gravitationalForce = GRAVITATIONAL_CONSTANT * (this.Mass * rigidBody.Mass) / fromRigidBodyToThis.LengthSquared; // TODO: Make this a static function to get gravitational force between two objects

        rigidBody.AddForce(gravitationalForce * -fromRigidBodyToThis.Normalized, mode: ForceMode.Force);

        if (this.rigidBody != null && this.AffectedByMutualGravitation)
            this.rigidBody.AddForce(gravitationalForce * fromRigidBodyToThis.Normalized, mode: ForceMode.Force);
    }

}

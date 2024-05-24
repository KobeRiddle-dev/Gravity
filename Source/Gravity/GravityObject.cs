using System.Collections.Generic;
using FlaxEngine;

#if USE_LARGE_WORLDS
using Real = System.Double;
using Mathr = FlaxEngine.Mathd;
#else
using Real = System.float;
using Mathr = FlaxEngine.Mathf;
#endif

namespace Gravity;

/// <summary>
/// GravityObject Script.
/// </summary>
public class GravityObject : Script
{
    public List<GravitySource> GravitySources { get; private set; } = new List<GravitySource>();
    public float RightingStrength { get; set; } = 0.5f;
    public bool SelfRightWhenInGravity { get; set; }

    /// <summary>
    /// Whether or not this is in the gravity of a GravitySource
    /// </summary>
    [ReadOnly]
    public bool IsInGravity
    {
        get
        {
            return this.GravitySources.Count > 0 || this.RigidBody.PhysicsScene.Gravity.Length > 0;
        }
    }

    protected RigidBody RigidBody
    {
        get
        {
            if (!this.rigidBody)
                this.rigidBody = this.Actor.As<RigidBody>();
            return this.rigidBody;
        }
        set => this.rigidBody = value;
    }
    private RigidBody rigidBody;


    /// <inheritdoc/>
    public override void OnStart()
    {
        this.RigidBody = this.Actor.As<RigidBody>();

        // Here you can add code that needs to be called when script is created, just before the first game update
    }

    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Here you can add code that needs to be called every frame
    }

    public override void OnFixedUpdate()
    {
        if (this.SelfRightWhenInGravity && this.IsInGravity)
            this.SelfRight();
    }

    public override void OnDebugDraw()
    {
        // Debug.Log("Drawing!");
        DebugDraw.DrawRay(this.Actor.Position, this.GetStrongestGravitationalVector(), Color.PaleGreen, length: 20);
    }
    
    /// <summary>
    /// Points the bottom of this GravityObject towards the strongest gravitational force
    /// </summary>
    public void SelfRight()
    {
        Vector3 gravityDown = this.GetStrongestGravitationalVector().Normalized;

        Quaternion rightedOrientation = Quaternion.GetRotationFromTo(this.Actor.Transform.Down, gravityDown, Vector3.Zero) * this.Actor.Orientation;

        this.Actor.Orientation = Quaternion.Lerp(this.Actor.Orientation, rightedOrientation, RightingStrength);
    }

    /// <param name="sourceMass">Outputs the mass of the GravitySource to which the vector points</param>
    /// <returns>the strongest gravitational vector of all the gravity sources in this.GravitySources</returns>
    public Vector3 GetStrongestGravitationalVector(out Real sourceMass)
    {
        sourceMass = 1;
        Vector3 strongestGravitationalVector = this.Actor.As<RigidBody>().PhysicsScene.Gravity * this.RigidBody.Mass;

        foreach (GravitySource gravitySource in this.GravitySources)
        {
            Vector3 gravityTowardsSource = -gravitySource.GetGravitationalVectorTowards(this.Actor.As<RigidBody>());

            if (gravityTowardsSource.LengthSquared > strongestGravitationalVector.LengthSquared)
            {
                strongestGravitationalVector = gravityTowardsSource;
                sourceMass = gravitySource.Mass;
            }
        }

        return strongestGravitationalVector;
    }

    /// <returns>the strongest gravitational vector of all the gravity sources in this.GravitySources</returns>
    public Vector3 GetStrongestGravitationalVector()
    {
        return GetStrongestGravitationalVector(out _);
    }

    /// <returns>the strongest gravitational force of all the gravity sources in this.GravitySources</returns>
    public Real GetStrongestGravitationalPull()
    {
        Real strongestGravitationalPull = this.Actor.As<RigidBody>().PhysicsScene.Gravity.Length;

        foreach (GravitySource gravitySource in this.GravitySources)
        {
            Real gravityWithSource = -gravitySource.GetGravitationalForceBetween(this.Actor.As<RigidBody>());

            if (gravityWithSource > strongestGravitationalPull)
            {
                strongestGravitationalPull = gravityWithSource;
            }
        }

        return strongestGravitationalPull;
    }
}

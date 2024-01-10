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
    protected RigidBody rigidBody;

    /// <summary>
    /// Whether or not this is in the gravity of a GravitySource
    /// </summary>
    [ReadOnly]
    public bool IsInGravity
    {
        get
        {
            return this.GravitySources.Count > 0 || this.rigidBody.PhysicsScene.Gravity.Length > 0;
        }
    }

    /// <inheritdoc/>
    public override void OnStart()
    {
        this.rigidBody = this.Actor.As<RigidBody>();
        
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
        DebugDraw.DrawRay(this.Actor.Position, this.GetStrongestGravitationalPull(), Color.PaleGreen);
    }

    public void SelfRight()
    {
        Vector3 gravityDown = this.GetStrongestGravitationalPull().Normalized;

        Quaternion rightedOrientation = Quaternion.GetRotationFromTo(this.Actor.Transform.Down, gravityDown, Vector3.Zero) * this.Actor.Orientation;

        this.Actor.Orientation = Quaternion.Lerp(this.Actor.Orientation, rightedOrientation, RightingStrength);
    }

    private Vector3 GetStrongestGravitationalPull()
    {

        Vector3 strongestGravitationalPull = this.Actor.As<RigidBody>().PhysicsScene.Gravity;

        foreach (GravitySource gravitySource in this.GravitySources)
        {
            Vector3 gravityTowardsSource = -gravitySource.GetGravitationalVectorTowards(this.Actor.As<RigidBody>());

            if (gravityTowardsSource.LengthSquared > strongestGravitationalPull.LengthSquared)
                strongestGravitationalPull = gravityTowardsSource;
        }

        return strongestGravitationalPull;
    }
}

using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace GameFish;

/// <summary>
/// Lets you access and affect something's ragdoll(assuming it has one).
/// </summary>
public interface IRagdoll
{
    public ModelPhysics Ragdoll { get; set; }

    public bool IsRagdolled
    {
        get
        {
            if ( !Ragdoll.IsValid() )
                return false;

            return Ragdoll.Enabled && Ragdoll.MotionEnabled;
        }
        set
        {
            if ( value )
                EnableRagdoll();
            else
                DisableRagdoll();
        }
    }

    /// <param name="enableComponent"> Enable the <see cref="ModelPhysics"/> component. </param>
    public void EnableRagdoll( bool enableComponent = true )
    {
        var ragdoll = Ragdoll;

        if ( !ragdoll.IsValid() )
            return;

        if ( enableComponent && !ragdoll.Enabled )
            ragdoll.Enabled = true;

        ragdoll.MotionEnabled = true;

        OnRagdollEnabled();
    }

    public void DisableRagdoll()
    {
        var ragdoll = Ragdoll;

        if ( !ragdoll.IsValid() )
            return;

        ragdoll.MotionEnabled = false;

        OnRagdollDisabled();
    }

    public void OnRagdollEnabled() { }
    public void OnRagdollDisabled() { }
}

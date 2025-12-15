namespace Playground;

partial class Editor
{
	/// <remarks>
	/// <b> NOTE: </b> A method that should be obsolete once an impulse extension unifies this.
	/// </remarks>
	[Rpc.Broadcast( NetFlags.UnreliableNoDelay )]
	public virtual void BroadcastImpulse( GameObject obj, Vector3 vel )
	{
		if ( !obj.IsValid() || obj.IsProxy )
			return;

		const FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors;

		if ( obj.Components.TryGet<IVelocity>( out var iVel, findMode ) )
			iVel.Velocity += vel;
		else if ( obj.Components.TryGet<Rigidbody>( out var rb, findMode ) )
			rb.Velocity += vel;
	}
}

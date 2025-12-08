namespace GameFish;

public interface ISessionEvent : ISceneEvent<ISessionEvent>
{
	/// <summary> Called right after the session has started. </summary>
	public virtual void OnSessionStart( Session s ) { }

	/// <summary> Called right after the session has ended. </summary>
	public virtual void OnSessionStop( Session s ) { }

	/// <summary> Called for resetting your session related data. </summary>
	public virtual void OnSessionFlush( Session s ) { }
}

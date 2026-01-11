namespace Fishbox;

partial class FishboxController
{
	public partial struct TraceResult
	{
		public readonly TraceSettings Settings { get; }
		public readonly float Skin => Settings.Skin;

		public Transform WorldStart { get; set; }

		/// <summary>
		/// The world-space difference in position between the start and attempted destination.
		/// </summary>
		public Vector3 Delta { get; set; }

		public SceneTraceResult BodyTrace { get; set; }
		public SceneTraceResult HeadTrace { get; set; }

		public readonly bool BothHit => BodyTrace.Hit && HeadTrace.Hit;

		/// <summary>
		/// The origin of entire body that started this trace.
		/// </summary>
		public readonly Vector3 StartPosition => WorldStart.Position;

		/// <summary> The world position of where the physics body would be at. </summary>
		public readonly Vector3 EndPosition { get; }

		/// <summary> The main trace that we decided hit(if any). </summary>
		public readonly SceneTraceResult HitTrace { get; }

		/// <summary> Did either the body or head trace hit? </summary>
		public readonly bool Hit => HitTrace.Hit;

		/// <summary> Did the body or head start solid? </summary>
		public readonly bool StartedSolid => BodyTrace.StartedSolid || HeadTrace.StartedSolid;

		public readonly Vector3 Normal => HitTrace.Normal;
		public readonly Vector3 HitPosition => HitTrace.HitPosition;
		public readonly float Distance => (HitTrace.Distance - Skin).Positive();

		public readonly GameObject GameObject => HitTrace.GameObject;
		public readonly Collider Collider => HitTrace.Collider;
		public readonly Surface Surface => HitTrace.Surface;

		public TraceResult() { }

		public TraceResult( in TraceSettings s, in Transform tStart, in Vector3 delta, in SceneTraceResult trBody, in SceneTraceResult trHead )
		{
			Settings = s;

			WorldStart = tStart;
			Delta = delta;

			BodyTrace = trBody;
			HeadTrace = trHead;

			if ( TryGetHitTrace( out var tr, out var endPos ) )
			{
				// Cache what was decidedly our hit trace.
				HitTrace = tr;

				// Pre-resolve the end position of the physics body.
				EndPosition = endPos;
			}
			else
			{
				// If there was no hit then we'll go where we meant to.
				EndPosition = tStart.Position + delta;
			}
		}

		public readonly Vector3 GetHitEndPosition( in SceneTraceResult tr )
		{
			if ( tr.StartedSolid )
				return StartPosition;

			var vDelta = tr.Direction * tr.Distance;
			vDelta -= (Delta.Normal * Skin).ClampLength( vDelta.Length );

			return StartPosition + vDelta;
		}

		/// <summary> Which trace hit, if either? </summary>
		private readonly bool TryGetHitTrace( out SceneTraceResult tr, out Vector3 endPos )
		{
			// If they both hit then choose the trace with less distance.
			if ( BodyTrace.Hit && HeadTrace.Hit )
			{
				if ( BodyTrace.Distance <= HeadTrace.Distance )
					goto BodyHit;
				else
					goto HeadHit;
			}

			BodyHit:

			if ( BodyTrace.Hit )
			{
				tr = BodyTrace;
				endPos = GetHitEndPosition( in tr );
				return true;
			}

			HeadHit:

			if ( HeadTrace.Hit )
			{
				tr = HeadTrace;
				endPos = GetHitEndPosition( in tr );
				return true;
			}

			tr = default;
			endPos = StartPosition;

			return false;
		}
	}
}

namespace Fishbox;

partial class FishboxController
{
	public partial struct TraceResult
	{
		public float Skin { get; set; }

		public Vector3 Direction { get; set; }
		public Transform WorldStart { get; set; }

		/// <summary>
		/// The origin of entire body that started this trace.
		/// </summary>
		public readonly Vector3 StartPosition { get; }

		public Vector3 BodyWorldOffset { get; set; }
		public Vector3 HeadWorldOffset { get; set; }

		public SceneTraceResult BodyTrace { get; set; }
		public SceneTraceResult HeadTrace { get; set; }

		/// <summary> Did the body or head hit? </summary>
		public readonly bool Hit => BodyTrace.Hit || HeadTrace.Hit;
		public readonly bool BothHit => BodyTrace.Hit && HeadTrace.Hit;

		/// <summary> Did the body or head start solid? </summary>
		public readonly bool StartedSolid => BodyTrace.StartedSolid || HeadTrace.StartedSolid;

		/// <summary> Which trace hit, if either? </summary>
		public readonly SceneTraceResult Trace => BodyTrace.Hit ? BodyTrace : (HeadTrace.Hit ? HeadTrace : default);

		public readonly Vector3 Normal { get; }
		public readonly Vector3 HitPosition { get; }

		public readonly GameObject GameObject { get; }
		public readonly Collider Collider { get; }
		public readonly Surface Surface { get; }

		public TraceResult() { }

		public TraceResult( in float skin, in Transform tStart, in Vector3 dir, in Vector3 bodyOffset, in Vector3 headOffset, in SceneTraceResult trBody, in SceneTraceResult trHead )
		{
			Skin = skin;

			WorldStart = tStart;
			StartPosition = tStart.Position;

			BodyWorldOffset = bodyOffset;
			HeadWorldOffset = headOffset;

			BodyTrace = trBody;
			HeadTrace = trHead;

			// Trace Result Cache
			var tr = Trace;

			Normal = tr.Normal;
			HitPosition = tr.HitPosition;

			GameObject = tr.GameObject;
			Collider = tr.Collider;
			Surface = tr.Surface;
		}

		public readonly bool TryGetEndPosition( out Vector3 endPos )
		{
			if ( BodyTrace.Hit )
			{
				endPos = BodyTrace.EndPosition - BodyWorldOffset;
				return true;
			}

			if ( HeadTrace.Hit )
			{
				endPos = HeadTrace.EndPosition - HeadWorldOffset;
				return true;
			}

			endPos = StartPosition;
			return false;
		}
	}
}

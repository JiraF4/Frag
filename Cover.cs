using Godot;
using System;

public partial class Cover : Node3D
{
	[Export] public Vector3 Direction;
	[Export] public float Side;
	[Export] public Vector3 AimPosition;

	private Vector3 _position;
	public Vector3 CoverPosition => _position;
	public float MinAgentDistance = 5000.0f;
	public bool SeeTarget { get; private set; }

	public override void _Ready()
	{
		_position = GlobalPosition;
		base._Ready();
	}

	public float GetProne()
	{
		return Mathf.DegToRad(25.0f * Side);
	}

	public void UpdateTarget(PhysicsDirectSpaceState3D directState, Vector3 targetPosition, Node3D target)
	{
		var query = new PhysicsRayQueryParameters3D()
		{
			From = AimPosition,
			To = targetPosition,
			CollideWithBodies = true, 
			CollisionMask = 3
		};
		var result = directState.IntersectRay(query);
		if (result.Count == 0)
		{
			SeeTarget = false;
			return;
		}

		SeeTarget = ((Node3D) result["collider"]) == target;
	}

}

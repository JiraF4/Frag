using Godot;
using System;

public partial class AIController : Node
{
	private Character _character;

	public Character Character => _character;

	private NavigationAgent3D _navigationAgent3D;

	public Character MainTarget;
	public Cover MainCover;
	
	public override void _Ready()
	{
		_character = (Character) GetParent();
		_navigationAgent3D = (NavigationAgent3D) _character.FindChild("NavigationPoint").FindChild("NavigationAgent3D");

		MainTarget = (Character) _character.GetParent().FindChild("Player");

		CoversServer.RegisterAgent(this);
	}
	
	
	
	public override void _PhysicsProcess(double delta)
	{
		//_mainCover = _coverSystem.GetCover(_character.GlobalPosition, _mainTarget.GlobalPosition);
		
		if (MainCover != null) _navigationAgent3D.TargetPosition = MainCover.GlobalPosition;
		//_navigationAgent3D.DebugEnabled = true;

		var nextPosition = _navigationAgent3D.GetNextPathPosition();
		var moveVector = (nextPosition - _character.GlobalPosition);
		var angle = 0.0f;
		if (_navigationAgent3D.DistanceToTarget() > 1.5f)
		{
			moveVector.Y = 0.0f;
			angle = _character.Basis.Z.SignedAngleTo(-moveVector.Normalized(), Vector3.Up);
			moveVector = moveVector.Normalized().Rotated(Vector3.Up, -_character.Rotation.Y);
			_character.MoveVector = moveVector;
			
			_character.Prone = 0.0f;
			_character.Crouch = false;
			_character.Aim = false;
		}
		else
		{
			_character.MoveVector = Vector3.Zero;
			if (MainCover != null)
			{
				
				//_character.Crouch = _mainCover.GetCrouch();
				
				_character.Aim = true;
			}
		}

		var target = _character.GetTarget();
		_character.Trigger = target == MainTarget;

		if (_navigationAgent3D.DistanceToTarget() > 10.0f)
			_character.Prone = 0.0f;
		else if (MainCover != null) _character.Prone = MainCover.GetProne();

		var nextTargetPosition = MainTarget.GlobalPosition + MainTarget.LinearVelocity * 0.15f;
		var vectorToTarget = (nextTargetPosition - _character.Camera.GlobalPosition).Normalized();
		var groundVectorToTarget = new Vector3(vectorToTarget.X, 0.0f, vectorToTarget.Z).Normalized();
		if (_navigationAgent3D.DistanceToTarget() < 10.0f) angle = _character.Basis.Z.SignedAngleTo(-groundVectorToTarget.Normalized(), Vector3.Up);
		vectorToTarget = vectorToTarget.Normalized().Rotated(Vector3.Up, -_character.Rotation.Y);
		var headAngle = _character.Camera.Basis.Z.SignedAngleTo(vectorToTarget * 10.0f, Vector3.Right);

		if (angle > 0.5f) angle = 0.5f;
		_character.RotationVector = new Vector3(0.0f, Mathf.RadToDeg(angle * 10.0f), 0.0f);
		_character.CameraRotationVector = new Vector3(Mathf.DegToRad(headAngle * 10.0f), 0.0f, 0.0f);
	}
}

using Godot;
using System;

public partial class AIController : Node
{
	private Character _character;
	private NavigationAgent3D _navigationAgent3D;

	private Character _mainTarget;
	private Cover _mainCover;
	
	public override void _Ready()
	{
		_character = (Character) GetParent();
		_navigationAgent3D = (NavigationAgent3D) _character.FindChild("NavigationPoint").FindChild("NavigationAgent3D");

		_mainTarget = (Character) _character.GetParent().FindChild("Player");
		//_mainCover = (Cover) _character.GetParent().FindChild("Cover2");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (_mainCover != null) _navigationAgent3D.TargetPosition = _mainCover.GlobalPosition;
		_navigationAgent3D.DebugEnabled = true;

		var nextPosition = _navigationAgent3D.GetNextPathPosition();
		var moveVector = (nextPosition - _character.GlobalPosition);
		var speed = moveVector.Length() / 8;
		if (speed > 0.4f)
		{
			moveVector.Y = 0.0f;
			//var angle = _character.Basis.Z.SignedAngleTo(-moveVector.Normalized(), Vector3.Up);
			moveVector = moveVector.Normalized().Rotated(Vector3.Up, -_character.Rotation.Y);
			if (speed > 1.0f) speed = 1.0f;
			_character.MoveVector = moveVector * speed;
		}
		else
			_character.MoveVector = Vector3.Zero;

		if (speed > 0.8f)
		{
			_character.Prone = 0.0f;
			_character.Crouch = false;
			_character.Aim = false;
		}
		else
		{
			if (_mainCover != null)
			{
				_character.Prone = _mainCover.GetProne();
				_character.Crouch = _mainCover.GetCrouch();
				
				_character.Aim = true;
			}
		}

		var nextTargetPosition = _mainTarget.GlobalPosition + _mainTarget.LinearVelocity * 0.15f;
		var vectorToTarget = (nextTargetPosition - _character.Camera.GlobalPosition).Normalized();
		var groundVectorToTarget = new Vector3(vectorToTarget.X, 0.0f, vectorToTarget.Z).Normalized();
		var angle = _character.Basis.Z.SignedAngleTo(-groundVectorToTarget.Normalized(), Vector3.Up);
		vectorToTarget = vectorToTarget.Normalized().Rotated(Vector3.Up, -_character.Rotation.Y);
		var headAngle = _character.Camera.Basis.Z.SignedAngleTo(vectorToTarget, Vector3.Right);
		
		_character.RotationVector = new Vector3(0.0f, Mathf.RadToDeg(angle * 10.0f), 0.0f);
		_character.CameraRotationVector = new Vector3(Mathf.DegToRad(headAngle * 10.0f), 0.0f, 0.0f);
	}
}

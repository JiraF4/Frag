using System;
using Godot;

public partial class Player : RigidBody3D
{
	public Vector3 MoveVector;
	public bool Jump;
	public bool Crouch;
	public Vector3 RotationVector;
	public Vector3 CameraRotationVector;
	
	private float _moveFriction = 0.05f;
	private float _moveFrictionMultiplier = 0.3f;
	private float _moveFrictionMax = 0.25f;
	private float _moveFrictionMin = 0.05f;
	
	private float _moveSpeed = 1.2f;
	
	private float _jumpDelay = 0.5f;
	private float _jumpTimer = 0.0f;

	private float _bodyShake = 0.0f;
	
	private Vector2 _mouseMove;
	private Vector2 _mousePosition;
	private int _mouseWheelPosition;
	private bool _mousePressed;

	private Node3D _body;
	private Camera3D _camera;
	private RayCast3D _legCast;
	private RayCast3D _headCast;

	public override void _Ready()
	{
		_body = (Node3D) FindChild("Body");
		_camera = (Camera3D) _body.FindChild("Camera3D");
		_legCast = (RayCast3D) FindChild("LegCast");
		_headCast = (RayCast3D) FindChild("HeadCast");
			
		base._Ready();
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion eventMouseMotion:
				_mouseMove += eventMouseMotion.Relative;
				_mousePosition = eventMouseMotion.Position;
				break;
			case InputEventMouseButton eventMouseButton:
				switch (eventMouseButton.ButtonIndex)
				{
					case MouseButton.None:
						break;
					case MouseButton.Left:
						_mousePressed = eventMouseButton.Pressed;
						break;
					case MouseButton.Right:
						break;
					case MouseButton.Middle:
						break;
					case MouseButton.WheelUp:
						if (eventMouseButton.Pressed)
							_mouseWheelPosition++;
						break;
					case MouseButton.WheelDown:
						if (eventMouseButton.Pressed)
							_mouseWheelPosition--;
						break;
					case MouseButton.WheelLeft:
						break;
					case MouseButton.WheelRight:
						break;
					case MouseButton.Xbutton1:
						break;
					case MouseButton.Xbutton2:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				break;
		}
	}

	public override void _Process(double delta)
	{
		
		MoveVector = Vector3.Zero;
		RotationVector = Vector3.Zero;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		MoveVector.X += Input.IsActionPressed("right") ? 1.0f : 0.0f;
		MoveVector.X += Input.IsActionPressed("left") ? -1.0f : 0.0f;
		MoveVector.Z += Input.IsActionPressed("forward") ? -1.0f : 0.0f;
		MoveVector.Z += Input.IsActionPressed("backward") ? 1.0f : 0.0f;
		MoveVector = MoveVector.Normalized();
		
		Jump = Input.IsActionPressed("jump");
		Crouch = Input.IsActionPressed("crouch");
			
		RotationVector = new Vector3(0, -_mouseMove.X, 0);
		CameraRotationVector = new Vector3(_mouseMove.Y, 0, 0);
		
		_mouseMove = Vector2.Zero;
		
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		
		var standPoint = _legCast.GetCollisionPoint();
		var standDistance = (standPoint - _legCast.GlobalPosition).Length();
		if (!_legCast.IsColliding()) standDistance = 4.0f;
		var headPoint = _headCast.GetCollisionPoint();
		var headDistance = (headPoint - _headCast.GlobalPosition).Length();
		if (!_headCast.IsColliding()) headDistance = 4.0f;
		if (Crouch && headDistance > 2.0f) headDistance = 2.0f;
		if (headDistance > 4.0f) headDistance = 4.0f;
		var globalMoveVector = MoveVector.Rotated(Vector3.Up, Rotation.Y);
		var groundVelocity = new Vector3(LinearVelocity.X, 0, LinearVelocity.Z);
		
		_jumpTimer -= (float) delta;
		

		if (globalMoveVector.Length() == 0.0f) _moveFriction += _moveFrictionMultiplier * (float) delta;
		if (_moveFriction > _moveFrictionMax) _moveFriction = _moveFrictionMax;
		if (globalMoveVector.Dot(groundVelocity) < 0.5f) _moveFriction = _moveFrictionMax;

		// move on ground
		if (standDistance < 4.0f && LinearVelocity.Y < 4.0f)
		{
			// body shake
			_bodyShake += globalMoveVector.Length() * 0.1f;
			if (globalMoveVector.Length() == 0.0f) _bodyShake = 0.0f;
			standDistance += Mathf.Sin(_bodyShake*2.0f) * 0.15f;
			if (standDistance < 0.0f) standDistance = 0.0f;
			if (standDistance > 4.0f) standDistance = 4.0f;
			
			if (globalMoveVector.Length() > 0.0f) _moveFriction -= _moveFrictionMultiplier * (float) delta * 2.0f;
			if (_moveFriction < _moveFrictionMin) _moveFriction = _moveFrictionMin;
			LinearVelocity += globalMoveVector * (_moveSpeed * (standDistance / 4.0f));
			LinearVelocity += new Vector3(0, (4.0f - standDistance - (4.0f - headDistance)) * 4.0f - LinearVelocity.Y*0.3f, 0);
			LinearVelocity -= groundVelocity * _moveFriction;

			if (Jump && _jumpTimer <= 0)
			{
				_jumpTimer = _jumpDelay;
				LinearVelocity += new Vector3(0, 35.0f - LinearVelocity.Y, 0);
			}
		}
		
		AngularVelocity = new Vector3(0, Mathf.DegToRad(RotationVector.Y * 10.0f), 0);

		// body rotation
		var forwardVelocity = LinearVelocity.Rotated(Vector3.Up, -Rotation.Y);
		var needBodyRotation = new Vector3(forwardVelocity.Z * 0.005f, 0, forwardVelocity.X * -0.005f + AngularVelocity.Y * 0.01f + Mathf.Cos(_bodyShake) * 0.01f);
		var bodyRotation = _body.Rotation;
		bodyRotation = bodyRotation.Lerp(needBodyRotation, (float) delta * 5.0f);
		var bodyRotationCompensation = _body.Rotation - bodyRotation;
		_body.Rotation = bodyRotation;
		
		// rotation
		var rotation = _camera.RotationDegrees;
		var newAngle = _camera.RotationDegrees.X - CameraRotationVector.X * 0.1f;
		if (newAngle > 70) newAngle = 70;
		if (newAngle < -70) newAngle = -70;
		_camera.RotationDegrees = new Vector3(newAngle, rotation.Y, rotation.Z) - bodyRotationCompensation;
		
		base._PhysicsProcess(delta);
	}
}

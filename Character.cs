using System;
using Godot;

public partial class Character : RigidBody3D
{
	public Vector3 MoveVector;
	public bool Jump;
	public bool Crouch;
	public bool Trigger;
	public bool Aim;
	public float Prone;
	public Vector3 RotationVector;
	public Vector3 CameraRotationVector;
	
	private float _moveFriction = 0.05f;
	private float _moveFrictionMultiplier = 0.3f;
	private float _moveFrictionMax = 0.25f;
	private float _moveFrictionMin = 0.05f;
	
	private float _moveSpeed = 1.1f;
	
	private float _jumpDelay = 0.5f;
	private float _jumpTimer = 0.0f;

	private float _bodyShake = 0.0f;

	private Node3D _body;
	public Camera3D Camera;
	private MeshInstance3D _head;
	private RayCast3D _weaponCast;
	private RayCast3D _legCast;
	private RayCast3D _headCast;
	
	private RayCast3D _leftFootCast;
	private RayCast3D _rightFootCast;
	
	private Weapon _weapon;
	private Node3D _weaponTarget;
	private float _recoilTimer;
	
	private Node3D _leftFoot;
	private Node3D _rightFoot;

	public override void _Ready()
	{
		_body = (Node3D) FindChild("Body");
		Camera = (Camera3D) _body.FindChild("Camera3D");
		_weaponCast = (RayCast3D) Camera.FindChild("WeaponCast");
		_head = (MeshInstance3D) Camera.FindChild("Head");
		_weapon = (Weapon) _body.FindChild("Weapon");
		_weaponTarget = (Node3D) _body.FindChild("WeaponTarget");
		_legCast = (RayCast3D) FindChild("LegCast");
		_headCast = (RayCast3D) FindChild("HeadCast");
			
		_leftFootCast = (RayCast3D) FindChild("LeftFootCast");
		_rightFootCast = (RayCast3D) FindChild("RightFootCast");
		
		_leftFoot = (Node3D) _leftFootCast.FindChild("LeftFoot");
		_rightFoot = (Node3D) _rightFootCast.FindChild("RightFoot");
		
		_weaponCast.AddException(this);

		if (Camera.Current)
		{
			_head.Visible = false;
		}
		
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		var standPoint = _legCast.GetCollisionPoint();
		var standNormal = _legCast.GetCollisionNormal();
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
			
			var slopeDot = standNormal.Dot(_legCast.TargetPosition.Normalized());
			var slopeFactor = -(slopeDot - 0.1f);
			if (slopeFactor < 0.0f) slopeFactor = 0.0f; 
			if (slopeFactor > 1.0f) slopeFactor = 1.0f; 
			
			// move
			if (globalMoveVector.Length() > 0.0f) _moveFriction -= _moveFrictionMultiplier * (float) delta * 2.0f;
			if (_moveFriction < _moveFrictionMin) _moveFriction = _moveFrictionMin;
			LinearVelocity += globalMoveVector * (_moveSpeed * (standDistance / 4.0f)) * slopeFactor;
			LinearVelocity += new Vector3(0, (4.0f - standDistance - (4.0f - headDistance)) * 3.0f - LinearVelocity.Y*0.5f, 0);
			LinearVelocity -= groundVelocity * _moveFriction;

			// jump
			if (Jump && _jumpTimer <= 0)
			{
				_jumpTimer = _jumpDelay;
				LinearVelocity += new Vector3(0, 35.0f - LinearVelocity.Y, 0);
			}
			
			// slide
			if (slopeFactor < 1.0f)
			{
				var slideDirection = new Vector3(standNormal.X, -standNormal.Y, standNormal.Z);
				LinearVelocity += slideDirection * 3.0f * (1.0f - slopeFactor);
			}
		}

		// body rotation
		var forwardVelocity = LinearVelocity.Rotated(Vector3.Up, -Rotation.Y);
		var needBodyRotation = new Vector3(Camera.RotationDegrees.X * 0.005f + forwardVelocity.Z * 0.005f, 0, Prone + forwardVelocity.X * -0.005f + AngularVelocity.Y * 0.01f + Mathf.Cos(_bodyShake) * 0.01f);
		var bodyRotation = _body.Rotation;
		bodyRotation = bodyRotation.Lerp(needBodyRotation, (float) delta * 5.0f);
		var bodyRotationCompensation = _body.Rotation - bodyRotation;
		_body.Rotation = bodyRotation;
		
		// rotation
		var rotation = Camera.RotationDegrees;
		var newAngle = Camera.RotationDegrees.X - CameraRotationVector.X;
		newAngle += (_recoilTimer > 0 ? _weapon.GetRecoilForce() * (float) delta * 3.0f : 0.0f); // Add recoil
		if (newAngle > 70) newAngle = 70;
		if (newAngle < -70) newAngle = -70;
		Camera.RotationDegrees = new Vector3(newAngle, rotation.Y, 0.0f) - bodyRotationCompensation;

		var yRotation = Mathf.DegToRad(RotationVector.Y);
		yRotation -= (_recoilTimer > 0 ? _weapon.GetRecoilForce() * (float) delta : 0.0f); // Add recoil
		AngularVelocity = new Vector3(0, yRotation, 0);
		
		// foots
		var leftFootPoint = _leftFootCast.GetCollisionPoint();
		var leftFootDistance = (leftFootPoint - _leftFootCast.GlobalPosition).Length();
		var rightFootPoint = _rightFootCast.GetCollisionPoint();
		var rightFootDistance = (rightFootPoint - _rightFootCast.GlobalPosition).Length();
		if (leftFootDistance > 3.5f) leftFootDistance = 3.5f;
		if (rightFootDistance > 3.5f) rightFootDistance = 3.5f;
		
		_leftFoot.Position = new Vector3(0.0f, -leftFootDistance, 0.0f);
		_leftFoot.LookAt(_leftFoot.GlobalPosition + _leftFootCast.GetCollisionNormal().Rotated(Basis.X, Mathf.RadToDeg(90.0f)), _leftFootCast.GetCollisionNormal());
		_rightFoot.Position = new Vector3(0.0f, -rightFootDistance, 0.0f);
		_rightFoot.LookAt(_rightFoot.GlobalPosition + _rightFootCast.GetCollisionNormal().Rotated(Basis.X, Mathf.RadToDeg(90.0f)), _rightFootCast.GetCollisionNormal());

		// weapon
		var weaponPoint = _weaponCast.GetCollisionPoint();
		var weaponNormal = _weaponCast.GetCollisionNormal();
		var weaponTargetVector = weaponPoint - _weaponCast.GlobalPosition;
		var weaponDistance = (weaponTargetVector).Length();
		if (!_weaponCast.IsColliding()) weaponDistance = 50.0f;
		
		// weapon push
		var minWeaponDistance = 2.0f + (Aim ? 2.0f : 0.0f);
		if (weaponDistance < minWeaponDistance)
		{
			var axis = weaponNormal.Cross(weaponTargetVector.Normalized());
			var angle = weaponNormal.SignedAngleTo(weaponTargetVector.Normalized(), axis);
			AngularVelocity += new Vector3(0.0f, -axis.Y * (Mathf.Pi - angle) / (weaponDistance / minWeaponDistance + 0.1f), 0.0f);
			
			var pushVector = GlobalPosition - weaponPoint;
			pushVector.Y = 0.0f;
			pushVector = pushVector.Normalized();
			LinearVelocity += pushVector * 0.5f / (weaponDistance / minWeaponDistance + 0.1f);
		}
		
		// weapon target
		if (!_weaponCast.IsColliding()) weaponPoint = _weaponCast.ToGlobal(_weaponCast.Position + _weaponCast.TargetPosition);
		_weaponTarget.LookAt(weaponPoint, Camera.ToGlobal(Camera.Basis.Y) - Camera.GlobalPosition);
		var targetTransform = _weaponTarget.GlobalTransform.Translated(_weaponTarget.ToGlobal(new Vector3(0, 0, (weaponDistance < 3.0f ? 3.0f - weaponDistance : 0.0f))) - _weaponTarget.GlobalPosition);
		if (Aim)
		{
			targetTransform = Camera.GlobalTransform.Translated(Camera.ToGlobal(new Vector3(0.0f, -0.15f, -1.0f)) - Camera.GlobalPosition);
		}
		_weapon.GlobalTransform = _weapon.GlobalTransform.InterpolateWith(targetTransform, (float) delta * 10.0f * (1.0f - (_recoilTimer * 4)));
		
		
		// recoil offset
		if (_recoilTimer > 0)
		{
			_weapon.Translate(_weapon.Basis.Z * _weapon.GetRecoilForce() * (float) delta);
			_weapon.RotateX(_weapon.GetRecoilForce() * (float) delta / 2.0f);
		}
		
		_recoilTimer -= (float) delta;
		if (_recoilTimer < 0) _recoilTimer = 0;
		if (Trigger && weaponDistance > minWeaponDistance)
		{
			var recoil = _weapon.Trigger();
			if (_recoilTimer < recoil) _recoilTimer = recoil;
		}
		
		base._PhysicsProcess(delta);
	}
}

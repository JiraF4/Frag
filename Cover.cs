using Godot;
using System;

public partial class Cover : Node3D
{
	private bool _up;
	private bool _leftProne;
	private bool _rightProne;
	private bool _leftProneCrouch;
	private bool _rightProneCrouch;
	
	private RayCast3D _upCast;
	private RayCast3D _leftProneCast;
	private RayCast3D _rightProneCast;
	private RayCast3D _leftProneCrouchCast;
	private RayCast3D _rightProneCrouchCast;
	
	private RayCast3D _mainRayCast3D1;
	private RayCast3D _mainRayCast3D2;
	
	private RayCast3D _groundCast;
	
	public override void _Ready()
	{
		_upCast = (RayCast3D) FindChild("UpCast");
		_leftProneCast = (RayCast3D) FindChild("LeftProneCast");
		_rightProneCast = (RayCast3D) FindChild("RightProneCast");
		_leftProneCrouchCast = (RayCast3D) FindChild("LeftProneCrouchCast");
		_rightProneCrouchCast = (RayCast3D) FindChild("RightProneCrouchCast");
		
		_mainRayCast3D1 = (RayCast3D) FindChild("MainRayCast3D1");
		_mainRayCast3D2 = (RayCast3D) FindChild("MainRayCast3D2");
		
		_groundCast = (RayCast3D) FindChild("GroundCast");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		_mainRayCast3D1.ForceRaycastUpdate();
		_mainRayCast3D2.ForceRaycastUpdate();
		if (!_mainRayCast3D1.IsColliding() && !_mainRayCast3D2.IsColliding())
		{
			GetParent().RemoveChild(this);
			return;
		}

		_upCast.ForceRaycastUpdate();
		_leftProneCast.ForceRaycastUpdate();
		_rightProneCast.ForceRaycastUpdate();
		_leftProneCrouchCast.ForceRaycastUpdate();
		_rightProneCrouchCast.ForceRaycastUpdate();
		
		_up = _upCast.IsColliding();
		_leftProne = _leftProneCast.IsColliding();
		_rightProne = _rightProneCast.IsColliding();
		_leftProneCrouch = _leftProneCrouchCast.IsColliding();
		_rightProneCrouch = _rightProneCrouchCast.IsColliding();

		if (_up) ((MeshInstance3D) _upCast.GetParent()).Visible = false;
		if (_leftProne) ((MeshInstance3D) _leftProneCast.GetParent()).Visible = false;
		if (_rightProne) ((MeshInstance3D) _rightProneCast.GetParent()).Visible = false;
		if (_leftProneCrouch) ((MeshInstance3D) _leftProneCrouchCast.GetParent()).Visible = false;
		if (_rightProneCrouch) ((MeshInstance3D) _rightProneCrouchCast.GetParent()).Visible = false;

		if (_up && _leftProne && _rightProne && _leftProneCrouch && _rightProneCrouch)
		{
			GetParent().RemoveChild(this);
			return;
		}

		ProcessMode = ProcessModeEnum.Disabled;
	}

	public bool GetCrouch()
	{
		return !_up && !_leftProne && !_rightProne;
	}
	
	public float GetProne()
	{
		if (!_up) return 0.0f;
		if (!_leftProne  || !_leftProneCrouch ) return  Mathf.DegToRad(25.0f);
		if (!_rightProne || !_rightProneCrouch) return -Mathf.DegToRad(25.0f);
		return 0.0f;
	}

	public override void _Process(double delta)
	{
	}
}

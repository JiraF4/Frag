using Godot;
using System;

public partial class PlayerController : Node
{
	private Character _character;
	
	private Vector2 _mouseMove;
	private Vector2 _mousePosition;
	private int _mouseWheelPosition;
	private bool _mousePressed;
	
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
	
	public override void _Ready()
	{
		_character = (Character) GetParent();
	}
	
	public override void _Process(double delta)
	{
		
		_character.MoveVector = Vector3.Zero;
		_character.RotationVector = Vector3.Zero;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		_character.Trigger = Input.IsActionPressed("trigger");
		
		_character.MoveVector.X += Input.IsActionPressed("right") ? 1.0f : 0.0f;
		_character.MoveVector.X += Input.IsActionPressed("left") ? -1.0f : 0.0f;
		_character.MoveVector.Z += Input.IsActionPressed("forward") ? -1.0f : 0.0f;
		_character.MoveVector.Z += Input.IsActionPressed("backward") ? 1.0f : 0.0f;
		_character.MoveVector = _character.MoveVector.Normalized();
		
		_character.Jump = Input.IsActionPressed("jump");
		_character.Crouch = Input.IsActionPressed("crouch");
		_character.Aim = Input.IsActionPressed("aim");

		_character.Prone = 0.0f;
		_character.Prone += Input.IsActionPressed("proneLeft")  ? 25.0f : 0.0f;
		_character.Prone -= Input.IsActionPressed("proneRight") ? 25.0f : 0.0f;
		_character.Prone = Mathf.DegToRad(_character.Prone);
		
		_character.RotationVector = new Vector3(0, -_mouseMove.X* 10.0f, 0);
		_character.CameraRotationVector = new Vector3(_mouseMove.Y * 0.1f, 0, 0);
		
		_mouseMove = Vector2.Zero;
		
		base._Process(delta);
	}
	
}

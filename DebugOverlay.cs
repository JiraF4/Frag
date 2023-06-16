using Godot;
using System;

public partial class DebugOverlay : VBoxContainer
{
	private Label _FPS;
	private Label _Memory;
	
	public override void _Ready()
	{
		_FPS = (Label) FindChild("FPS");
		_Memory = (Label) FindChild("Memory");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_FPS.Text = "FPS " + Engine.GetFramesPerSecond();
		_Memory.Text = "Memory " + OS.GetStaticMemoryUsage();
	}
}

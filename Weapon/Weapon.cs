using Godot;
using System;

public partial class Weapon : Node3D
{
	protected Node3D ProjectilePosition; 
	protected GpuParticles3D Particles;

	protected float FireDelay = 0.6f;
	protected float FireTimer = 0.0f;
	
	protected float EmitTime = 0.04f;
	protected float EmitTimer = 0.0f;
	
	protected float RecoilForce = 8.4f;
	protected float RecoilTime = 0.07f;

	public float GetRecoilForce()
	{
		return RecoilForce;
	}
	
	public override void _Ready()
	{
		ProjectilePosition = (Node3D) FindChild("ProjectilePosition");
		Particles = (GpuParticles3D) ProjectilePosition.FindChild("Particles");
		
		base._Ready();
	}

	public override void _Process(double delta)
	{
		Particles.Emitting = EmitTimer > 0;
		
		FireTimer -= (float) delta;
		EmitTimer -= (float) delta;
		
		base._Process(delta);
	}

	public virtual float Trigger()
	{
		return 0.0f;
	}
}

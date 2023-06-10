using Godot;
using System;

public partial class Shotgun : Weapon
{
    public override float Trigger()
    {
        if (FireTimer <= 0)
        {
            FireTimer = FireDelay;
            EmitTimer = EmitTime;
        
            return RecoilTime;
        }
        
        return 0.0f;
    }
}

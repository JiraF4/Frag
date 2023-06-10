using Godot;
using System;

public partial class CoverSystem : Node
{
	PackedScene _coverScene;
	
	public override void _Ready()
	{
		_coverScene = (PackedScene) ResourceLoader.Load("res://cover.tscn");
		CallDeferred(nameof(CreateCovers));
	}

	public void CreateCovers()
	{
		var navigationEdges = ((NavigationRegion3D) GetParent().FindChild("NavigationRegion3D")).NavigationMesh.Vertices;
		foreach (var navigationEdge in navigationEdges)
		{
			for (float r = 0; r < 4; r++)
			{
				//var cover = (Cover) _coverScene.Instantiate();
				//cover.Rotation = new Vector3(0.0f, Mathf.DegToRad(90.0f * r), 0.0f);
				//cover.Position = navigationEdge;
				//GetParent().AddChild(cover);
			}
		}
	}
}

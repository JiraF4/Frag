using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class CoverSystem : Node
{
	PackedScene _coverScene;
	PhysicsDirectSpaceState3D _directState;

	[Export] private Shape3D _shape;

	private bool updating = false;
	[Export]
	private bool Update
	{
		set
		{
			SetPhysicsProcess(true);
			updating = true;
		}
		get => false;
	}

	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
			return;
	}

	public override void _PhysicsProcess(double delta)
	{
		SetPhysicsProcess(false);
		if (!updating)
			return;
		if (!Engine.IsEditorHint())
			return;
		
		SetPhysicsProcess(false);
		var worldRef = GetViewport().FindWorld3D();
		_directState = worldRef.DirectSpaceState;

		var debugDraw = GetNode("DebugDraw");
		var coversNode = GetNode("CoversServer");
		foreach (var node in debugDraw.GetChildren())
		{
			debugDraw.RemoveChild(node);
			node.Free();
		}
		foreach (var node in coversNode.GetChildren())
		{
			coversNode.RemoveChild(node);
			node.Free();
		}
		
		var mesh = (MeshInstance3D) GetParent().GetNode("NavigationRegion3D/QodotMap/entity_0_worldspawn/entity_0_mesh_instance");
		var vertices = mesh.Mesh.GetFaces();
		
		var debugSphereMaterial = new OrmMaterial3D {ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, AlbedoColor = Colors.Red, NoDepthTest = true};
		var debugSphereMesh = new SphereMesh {Material = debugSphereMaterial, Radius = 0.1f};
		
		var points = new List<Vector3>();
		foreach (var vertex in vertices)
		{
			var groundPoint = GetGround(vertex + new Vector3(0.0f, 1.0f, 0.0f));
			
			
			if (IsEmptySpace(groundPoint) || IsEmptySpace(groundPoint + new Vector3(0.0f, 3.0f, 0.0f)) || IsEmptySpace(groundPoint + new Vector3(0.0f, 6.0f, 0.0f)))
				continue;

			
			var result = new NavigationPathQueryResult3D();
			NavigationServer3D.QueryPath(new NavigationPathQueryParameters3D() {Map = worldRef.NavigationMap, StartPosition = new Vector3(0.0f, 0.0f, 0.0f), TargetPosition = groundPoint}, result);
			
			if (result.Path.Last().DistanceTo(groundPoint) > 5.0f)
				continue;


			/*
			const float standDistance = 1.25f;
			var standCount = 0;
			for (var d = 0.0f; d < 360.0f; d += 45.0f)
			{
				if (CanStand(groundPoint + new Vector3(standDistance * Mathf.Sin(Mathf.RadToDeg(d)), 0.0f, standDistance * Mathf.Cos(Mathf.RadToDeg(d))) * standDistance)) standCount++;
			}
			
			if (standCount < 4)
				continue;
			*/
			
			if (!points.Any(p => p.DistanceTo(groundPoint) < 1.0f))
				points.Add(groundPoint);
		}
		
		var debugLineMaterial = new OrmMaterial3D {ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, VertexColorUseAsAlbedo = true, NoDepthTest = true};
		var covers = new List<Cover>();
		foreach (var point in points)
		{
			const float standDistance = 4.0f;
			for (var d = 0.0f; d < 360.0f; d += 24.5f)
			{
				var standPoint = new Vector3(standDistance * Mathf.Sin(Mathf.DegToRad(d)), 0.0f, standDistance * Mathf.Cos(Mathf.DegToRad(d)));
				standPoint = GetGround(standPoint + point + new Vector3(0.0f, 2.0f, 0.0f)) - point;
				
				if (!CanStand(standPoint + point))
				{
					continue;
				}
				
				var result = new NavigationPathQueryResult3D();
				NavigationServer3D.QueryPath(new NavigationPathQueryParameters3D() {Map = worldRef.NavigationMap, StartPosition = Vector3.Zero, TargetPosition = standPoint + point}, result);
				if (result.Path.Last().DistanceTo(standPoint + point) > 0.8f)
					continue;
				
				var targetPoint = -standPoint;
				var aimDistance = 2.0f;
				var aimPoint = new Vector3(aimDistance * Mathf.Sin(Mathf.DegToRad(d + 90.0f)), 5.0f, aimDistance * Mathf.Cos(Mathf.DegToRad(d + 90.0f)));
				var side = -1.0f;
				if (LineIntersect(standPoint + aimPoint + point, targetPoint + point + aimPoint))
				{
					side = 1.0f;
					aimPoint = new Vector3(aimDistance * Mathf.Sin(Mathf.DegToRad(d - 90.0f)), 5.0f, aimDistance * Mathf.Cos(Mathf.DegToRad(d - 90.0f)));
					if (LineIntersect(standPoint + aimPoint + point, targetPoint + point + aimPoint))
					{
						continue;
					}
				}
				
				covers.Add(new Cover()
				{
					Direction = targetPoint.Normalized(),
					GlobalPosition = point + standPoint,
					AimPosition = standPoint + aimPoint + point,
					Side = side
				});
				
				/*
				var debugMeshInstance = new MeshInstance3D {CastShadow = GeometryInstance3D.ShadowCastingSetting.Off, Mesh = debugSphereMesh};
				debugDraw.AddChild(debugMeshInstance);
				debugMeshInstance.Owner = GetTree().EditedSceneRoot;
				debugMeshInstance.GlobalPosition = point;

				var debugImmediateMesh = new ImmediateMesh();
				
				debugMeshInstance = new MeshInstance3D
				{
					CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
					Mesh = debugImmediateMesh,
					MaterialOverride = debugLineMaterial,
					GlobalPosition = point
				};

				debugDraw.AddChild(debugMeshInstance);
				debugMeshInstance.Owner = GetTree().EditedSceneRoot;
				
				debugImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
				debugImmediateMesh.SurfaceSetColor(Colors.Red);
				debugImmediateMesh.SurfaceAddVertex(Vector3.Zero);
				debugImmediateMesh.SurfaceAddVertex(standPoint);
				debugImmediateMesh.SurfaceAddVertex(standPoint + new Vector3(0.0f, 5.0f, 0.0f));
				debugImmediateMesh.SurfaceSetColor(Colors.Blue);
				debugImmediateMesh.SurfaceAddVertex(standPoint + aimPoint);
				debugImmediateMesh.SurfaceAddVertex(targetPoint + aimPoint * 0.5f);

				debugImmediateMesh.SurfaceEnd();
				*/
			}
		}

		foreach (var cover in covers)
		{
			/*
			var debugMeshInstance = new MeshInstance3D {CastShadow = GeometryInstance3D.ShadowCastingSetting.Off, Mesh = debugSphereMesh};
			debugDraw.AddChild(debugMeshInstance);
			debugMeshInstance.Owner = GetTree().EditedSceneRoot;
			debugMeshInstance.GlobalPosition = cover.Position;
			*/
			coversNode.AddChild(cover);
			cover.Owner = GetTree().EditedSceneRoot;
		}
	}

	Vector3 GetGround(Vector3 position)
	{
		var query = new PhysicsRayQueryParameters3D()
		{
			From = position,
			To = position + new Vector3(0.0f, -1500.0f, 0.0f),
			CollideWithBodies = true, 
			CollisionMask = 2
		};
		var result = _directState.IntersectRay(query);
		if (result.Count == 0) return position;
		return (Vector3) result["position"];
	}
	
	bool LineIntersect(Vector3 from, Vector3 to)
	{
		var query = new PhysicsRayQueryParameters3D()
		{
			From = from,
			To = to,
			CollideWithBodies = true, 
			CollisionMask = 2
		};
		var result = _directState.IntersectRay(query);
		return result.Count > 0;
	}

	bool CanStand(Vector3 position)
	{
		return !IsEmptySpace(position - new Vector3(0.0f, 0.0f, 0.0f)) && 
		       IsEmptySpace(position + new Vector3(0.0f, 2.5f, 0.0f)) &&
		       IsEmptySpace(position + new Vector3(0.0f, 5.0f, 0.0f));
	}
	
	bool IsEmptySpace(Vector3 position)
	{
		var query = new PhysicsShapeQueryParameters3D()
		{
			Transform = new Transform3D(new Basis(), position), 
			CollideWithBodies = true, 
			CollisionMask = 2,
			Shape = _shape
		};
		var result = _directState.IntersectShape(query, 1);
		return result.Count == 0;
	}
}

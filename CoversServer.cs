using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class CoversServer : Node
{
	private static List<AIController> _agents = new();
	private static Task _processTask;
	private static List<Cover> _covers;
	private static bool _run;
	
	public static void RegisterAgent(AIController controller)
	{
		_agents.Add(controller);
	}
	
	public override void _Ready()
	{
		_run = true;
		_processTask = Task.Run(CoversProcess);
		_covers = GetChildren().Cast<Cover>().ToList();
	}

	void CoversProcess()
	{
		Thread.Sleep(100);
		while (_run)
		{
			foreach (var cover in _covers)
			{
				cover.MinAgentDistance = 5000.0f;
				foreach (var agent in _agents)
				{
					var distanceToAgent = cover.CoverPosition.DistanceTo(agent.Character.GlobalPosition);
					if (cover.MinAgentDistance > distanceToAgent)
						cover.MinAgentDistance = distanceToAgent;
				}	
			}
			foreach (var agent in _agents)
			{
				agent.MainCover = GetCover(agent, agent.Character.GlobalPosition, agent.MainTarget.GlobalPosition);
			}
		}
	}
	
	public Cover GetCover(AIController agent, Vector3 agentPosition, Vector3 targetPosition)
	{ 
		var worldRef = GetViewport().FindWorld3D();

		var covers = _covers;
		return covers.OrderBy(c =>
		{
			if (!_run) return 0;
			var coverPosition = c.CoverPosition;
			var targetVector = targetPosition - coverPosition;
			
			var score = 0.0f;
			var targetDistance = targetVector.Length();
			score -= (targetVector / targetDistance).Dot(c.Direction) * 800.0f;
			if (targetDistance < 100.0f)
			{
				score += Mathf.Abs(100.0f - targetDistance) * 10.0f;
			}

			score += agentPosition.DistanceTo(coverPosition) * 15.0f;
			score += (!c.SeeTarget ? 1000.0f : 0.0f);
			score += (c != agent.MainCover ? 150.0f : 0.0f);
			
			foreach (var otherAgent in _agents)
			{
				if (!_run) return 0;
				if (otherAgent != agent)
				{
					var otherAgentDistance = otherAgent.Character.GlobalPosition.DistanceTo(coverPosition);
					if (otherAgentDistance < 50.0f)
					{
						score += (50.0f - otherAgentDistance) * 25.0f;
					}

					if (otherAgent.MainCover == c)
					{
						score += 350.0f;
					}
				}
			}
			
			return score;
		}).FirstOrDefault(c =>
		{
			if (!_run) return true;
			var coverPosition = c.CoverPosition;
			var path = NavigationServer3D.MapGetPath(worldRef.NavigationMap, agentPosition, coverPosition, false);
			for (var i = 1; i < path.Length; i++)
			{
				if (path[i].DistanceTo(targetPosition) < 10.0f) return false;
			}
			return true;
		}, null);
	}

	private int _coverNum = 0;
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		var worldRef = GetViewport().FindWorld3D();
		var directState = worldRef.DirectSpaceState;

		var target = (Character) _agents[0].MainTarget;
		for (var i = 0; i < 50; i++)
		{
			_covers[_coverNum].UpdateTarget(directState, target.GlobalPosition, target);
			_coverNum++;
			if (_coverNum >= _covers.Count) _coverNum = 0;
		}
	}

	public override void _ExitTree()
	{
		_run = false;
		_processTask.Wait();
		base._ExitTree();
	}
}

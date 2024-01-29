using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Map : Node2D
{
	private double _enemyCd = 0;
	private const double ENEMY_SPAWN_TIMER = 2.0;
	private const int MAX_NUM_ENEMIES = 5; // for each spawn point

  private static Random randomizer = new();
	private List<List<Node2D>> _waypoints = new();
	private List<int> _spawnPointsNumEnemies = new();

  private CharacterBody2D playerObj;
	private PackedScene[] _enemyScenes = {null, null};
	public override void _Ready()
	{
		this.playerObj = GetNode<CharacterBody2D>("./Player");
		Node2D spawnPointsRoot = GetNode<Node2D>("./Waypoints");
		_enemyScenes[0] = GD.Load<PackedScene>("res://Scenes/Enemies/Muteret_mand.tscn");
  	_enemyScenes[1] = GD.Load<PackedScene>("res://Scenes/Enemies/Muteret_hund.tscn");

		List<Node2D> spawnPoints = new();
		
		Array<Node> children = spawnPointsRoot.GetChildren();
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is Node2D c) {
				spawnPoints.Add(c);
				_waypoints.Add(new());
				_spawnPointsNumEnemies.Add(0);
			}

			foreach (Node waypoint in children[i].GetChildren())
			{
				if (waypoint is Node2D d)
					_waypoints[i].Add(d);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_enemyCd -= delta;
		if (_enemyCd <= 0) {
	  	SpawnEnemy();
			_enemyCd = ENEMY_SPAWN_TIMER;
		}
	}


	private void SpawnEnemy() {
		List<List<Node2D>> nonFullSpawnPoints = new();
		for (int i = 0; i < _waypoints.Count; i++) {
			if (_spawnPointsNumEnemies[i] > MAX_NUM_ENEMIES)
				continue;

			nonFullSpawnPoints.Add(_waypoints[i]);
			_spawnPointsNumEnemies[i]++;
		}

		if (nonFullSpawnPoints.Count == 0)
			return;

		int spawnPointIdx = randomizer.Next(0, nonFullSpawnPoints.Count);
		List<Node2D> waypoints = nonFullSpawnPoints[spawnPointIdx];

		GD.Print("Spawning enemy!");
		Enemy instance = (Enemy)_enemyScenes[randomizer.Next(0, 2)].Instantiate();
		instance.Initialize(waypoints);
		instance.Position = waypoints[0].GlobalPosition;

		AddChild(instance);
  }

  public void DestroyEnemySpawnTimer() {
	GD.Print("Destroying enemy respawn timer");
  }

}

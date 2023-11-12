using Godot;
using System;
using System.Data;

public partial class Map : Node2D
{

  private const float enemySpawnDistance = 200.0f;
  private static Vector4 ENEMY_SPAWN_BOUNDARY = new(10, 25, 1142, 630);
  private static System.Threading.Timer enemySpawnTimer;
  private PackedScene enemyScene;
  private static Random randomizer = new();

  private CharacterBody2D playerObj;
	public override void _Ready()
	{
	this.playerObj = GetNode<CharacterBody2D>("./Player");
	this.enemyScene = GD.Load<PackedScene>("res://Scenes/Enemies/Muteret_mand.tscn");
	this.SetEnemySpawnTimer();
	}


  private void SetEnemySpawnTimer() {
	
	enemySpawnTimer = new System.Threading.Timer(this.SpawnEnemy, this.playerObj, 0, 2000);
  }

  private void SpawnEnemy(object obj) {
	CharacterBody2D playerObj = obj as CharacterBody2D;
	Vector2 playerPos = playerObj.GetPositionDelta();
	playerPos = new Vector2(-playerPos.X, -playerPos.Y);

	Vector2 spawnPoint = this.GetSpawnPoint(playerPos);
	  
	Node2D instance = (Node2D)this.enemyScene.Instantiate();
	instance.Position = spawnPoint;

	this.CallDeferred("add_child", instance);
  }

  private Vector2 GetSpawnPoint(Vector2 playerPos) {
	double v = randomizer.NextDouble() * 2 * Math.PI;
	
	Vector2 spawnPoint = new Vector2(playerPos.X + enemySpawnDistance * (float)Math.Cos(v), playerPos.Y + enemySpawnDistance * (float)Math.Sin(v));


	if (spawnPoint.X < ENEMY_SPAWN_BOUNDARY.X)
	  spawnPoint.X = ENEMY_SPAWN_BOUNDARY.X;
	else if (spawnPoint.X > ENEMY_SPAWN_BOUNDARY.Z)
	  spawnPoint.X = ENEMY_SPAWN_BOUNDARY.Z;
	
	if (spawnPoint.Y < ENEMY_SPAWN_BOUNDARY.Y)
	  spawnPoint.Y = ENEMY_SPAWN_BOUNDARY.Y;
	else if (spawnPoint.Y > ENEMY_SPAWN_BOUNDARY.W)
	  spawnPoint.Y = ENEMY_SPAWN_BOUNDARY.W;

	return spawnPoint;
  }

}
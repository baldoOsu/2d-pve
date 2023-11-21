using Godot;
using System;
using System.Data;
using System.Threading;

public partial class Map : Node2D
{

  private const float ENEMY_SPAWN_DISTANCE = 200.0f;
  private static Vector4 ENEMY_SPAWN_BOUNDARY = new(10, 25, 1142, 630);
  private System.Threading.Timer enemySpawnTimer;
  private PackedScene[] enemyScenes = {null, null};
  private static Random randomizer = new();

  private CharacterBody2D playerObj;
	public override void _Ready()
	{
	this.playerObj = GetNode<CharacterBody2D>("./Player");
	this.enemyScenes[0] = GD.Load<PackedScene>("res://Scenes/Enemies/Muteret_mand.tscn");
  this.enemyScenes[1] = GD.Load<PackedScene>("res://Scenes/Enemies/Muteret_hund.tscn");
  

	this.InitiateEnemySpawnTimer();
	}


  private void InitiateEnemySpawnTimer() {
	GD.Print("Initializing enemy spawn timer");
	  this.enemySpawnTimer = new System.Threading.Timer(this.SpawnEnemy, this.playerObj, 0, 500);
  }

  private void SpawnEnemy(object obj) {
	CharacterBody2D playerObj = obj as CharacterBody2D;
	Vector2 playerPos = playerObj.GetPositionDelta();
	playerPos = new Vector2(-playerPos.X, -playerPos.Y);

	Vector2 spawnPoint = this.GetSpawnPoint(playerPos);
	  
	Node2D instance = (Node2D)this.enemyScenes[randomizer.Next(0, 2)].Instantiate();
	instance.Position = spawnPoint;

	this.CallDeferred("add_child", instance);
  }

  private Vector2 GetSpawnPoint(Vector2 playerPos) {
	double v = randomizer.NextDouble() * 2 * Math.PI;
	
	Vector2 spawnPoint = new Vector2(playerPos.X + ENEMY_SPAWN_DISTANCE * (float)Math.Cos(v), playerPos.Y + ENEMY_SPAWN_DISTANCE * (float)Math.Sin(v));


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

  public void DestroyEnemySpawnTimer() {
	GD.Print("Destroying enemy respawn timer");
	this.enemySpawnTimer.Dispose();
  }

}

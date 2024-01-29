using Godot;
using System;

using BehaviorTree;
using System.Collections.Generic;

public partial class Enemy : CharacterBody2D {
private CharacterBody2D _playerObj;


  private const float SPEED = 50.0f;
  private const double DAMAGE_CD = 1.0f;
  private double currentDamageCd = 1.0f;
  private const int DAMAGE = 10;
  public enum Direction
	{
		Down,
		Right,
		Left,
		Up
	}
	public static readonly string[] DirectionTable = { "up_", "side_", "side_", "front_" };
	protected Direction dir = Direction.Left;
  AnimatedSprite2D _anim;

	List<Node2D> _waypoints;
	private RootEnemyNode _AI;

  public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("./Movement");
	  _playerObj = GetNode<CharacterBody2D>("/root/Root/Map/Player");
		_AI = new(this, _playerObj, _waypoints, SPEED);
	}

  public override void _PhysicsProcess(double delta)
  {
		_AI.Evaluate();
		// this.currentDamageCd -= delta;

		// Vector2 direction = this.playerObj.Position - this.Position;

		// PlayAnim(VecToMovement(direction));

		// this.Velocity = direction.Normalized() * Enemy.SPEED;
		// MoveAndSlide();

		// if (this.currentDamageCd > 0)
		// 	return;

		// int collisionCount = GetSlideCollisionCount();
		// for (int i = 0; i < collisionCount; i++) {
		// 	// sikrer ikke at gøre mere end 10 damage ad gangen
		// 	if (this.currentDamageCd > 0)
		// 	return;

		// 	KinematicCollision2D collision = GetSlideCollision(i);
		// 	GodotObject collider = collision.GetCollider();
		// 	if (collider.HasMethod("Damage"))  {
		// 	collider.CallDeferred("Damage", Enemy.DAMAGE);
		// 	this.currentDamageCd = Enemy.DAMAGE_CD;
		// 	}
		// }
  }

	public void Initialize(List<Node2D> waypoints)
	{
		_waypoints = waypoints;
	}

  private void PlayAnim(string movement)
	{
		switch (this.dir)
		{
			case Direction.Left:
				this._anim.FlipH = true;
				break;

			case Direction.Right:
				this._anim.FlipH = false;
				break;
		}

	this._anim.Play(DirectionTable[(int)this.dir] + movement);
	}

  protected virtual string VecToMovement(Vector2 vec) {
		throw new NotImplementedException();
	}
}

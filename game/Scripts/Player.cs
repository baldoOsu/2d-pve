using Godot;
using System;
using System.Linq;
using System.Threading;

public partial class Player : CharacterBody2D
{
  private Viewport viewport;
  private Texture[] crosshairs = { null, null };
	private AnimatedSprite2D anim;
  private AudioStreamPlayer2D skudPlayer;
  private TextureProgressBar HPBar;
  private Global global;
  private System.Threading.Timer crosshairAnimTimer;

	public enum Direction
	{
		Down,
		Right,
		Left,
		Up
	}
	public static readonly string[] DirectionTable = { "up_", "side_", "side_", "front_" };

	public const float Speed = 8500.0f;
	private Direction dir = Direction.Down;

  private static double BULLET_CD_RESET_TIMER = 0.15;
  private double bulletCd = -1.0;
  private int _hp = 100;
  public int HP
  { 
	get { return _hp; }
	set {
	  this._hp = value;
	  this.HPBar.Value = this._hp;
	}
  }

	public override void _Ready()
	{
	  this.InitCrosshairAnim();
	this.global = GetNode<Global>("/root/Global");
	this.HPBar = GetNode<TextureProgressBar>("./HPBar");
	this.skudPlayer = GetNode<AudioStreamPlayer2D>("./Skud");
	this.anim = GetNode<AnimatedSprite2D>("./Movement");

		this.anim.Play("front_idle");
	  this.global.RenderScore();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = this.Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction != Vector2.Zero)
		{
			velocity = direction * Player.Speed * (float)delta;
		}
		else
		{
			velocity.X = Mathf.MoveToward(this.Velocity.X, 0, Player.Speed * (float)delta);
			velocity.Y = 0;
		}

	

	this.bulletCd -= delta;
	bool isShooting = Input.IsActionPressed("shoot");
	if (isShooting && this.bulletCd < 0) {
	Vector2 globalMousePos = this.GetGlobalMousePosition();
	  this.SetDirByMouse(globalMousePos);
	  this.Shoot(this.GlobalTransform.Origin, globalMousePos);
	}

	

	this.PlayAnim(this.VecToMovement(velocity, isShooting), isShooting);

	this.Velocity = velocity;
	MoveAndSlide();
	}

	private void PlayAnim(string movement, bool isShooting)
	{
		switch (this.dir)
		{
			case Direction.Left:
				this.anim.FlipH = true;
				break;

			case Direction.Right:
				this.anim.FlipH = false;
				break;
		}
	if (isShooting) {
	  this.anim.Play("shoot");
	  return;
	}

	if (movement != "idle")
	  this.anim.Play(DirectionTable[(int)this.dir] + movement);
	else
	  this.anim.Play("front_idle");
	
	}

	private string VecToMovement(Vector2 vec, bool isShooting)
	{
		if (vec.X == 0 && vec.Y == 0)
			return "idle";

	// undgå at opdatere this.dir hvis man skyder
	if (isShooting)
	  return "";

		if (vec.X > 0)
		{
			this.dir = Direction.Right;
			return "walk";
		}
		else if (vec.X < 0)
		{
			this.dir = Direction.Left;
			return "walk";
		}

		if (vec.Y > 0)
		{
			this.dir = Direction.Up;
			return "walk";
		}
		else if (vec.Y < 0)
		{
			this.dir = Direction.Down;
			return "walk";
		}

	return ""; // vil aldrig ske

	}

  private void SetDirByMouse(Vector2 globalMousePos) {
	if ((globalMousePos.X - this.Position.X) > 0) {
	  this.dir = Direction.Right;
	  return;
	}

	this.dir = Direction.Left;
  }

  private void Shoot(Vector2 startPos, Vector2 dir) {
	this.bulletCd = BULLET_CD_RESET_TIMER;

	this.skudPlayer.Play();
	var spaceState = GetWorld2D().DirectSpaceState;

	// det her gør så man ikke kan skyde igennem generators
	// 6 er bit mask til 2, 3 collision layers (0b110)
	var RayQuery = PhysicsRayQueryParameters2D.Create(startPos, dir, 6);
	var RayResult = spaceState.IntersectRay(RayQuery);

	Variant rqVal = new();
	StaticBody2D sbVal;
	RayResult.TryGetValue("collider", out rqVal);
	try {
	  sbVal = rqVal.As<StaticBody2D>();
	} catch {
	  return;
	}

	if (sbVal?.HasMeta("Enemy_Hitbox") == false)
	  return;

	// det her gør så man skal have crosshair på enemy hitbox for at kunne ramme
	// hvis man bruger raycast alene, kan man ramme selv med crosshair bag enemy
	var PointQuery = new PhysicsPointQueryParameters2D();
	PointQuery.Position = dir;
	var PointResult = spaceState.IntersectPoint(PointQuery);
	if (PointResult.Count == 0)
	  return;

	Variant pqVal = new();
	PointResult[0].TryGetValue("collider", out pqVal);
	try {
	  sbVal = pqVal.As<StaticBody2D>();
	} catch {
	  return;
	}

	if (sbVal?.HasMeta("Enemy_Hitbox") == false)
	  return;

	sbVal.GetParent().Free();
	this.global.IncrementScore();
  }

  public void Damage(int val) {
	this.HP -= val;

	if (this.HP < 1)
	  global.ResetGame();
  }

  private void InitCrosshairAnim() {
	GD.Print("Initializing crosshair animation");

	this.crosshairs[0] = ResourceLoader.Load<Texture>("res://Assets/Cursors/crosshair-frame-0.png");
	this.crosshairs[1] = ResourceLoader.Load<Texture>("res://Assets/Cursors/crosshair-frame-1.png");
	this.crosshairAnimTimer = new System.Threading.Timer((object state) => {
	  this.CallDeferred("SetMouseCursor", null);
	}, null, 0, 300);
  }

  public void SetMouseCursor() {
	  Input.SetCustomMouseCursor(this.crosshairs[0], default, new Vector2(17, 17));
	  (this.crosshairs[0], this.crosshairs[1]) = (this.crosshairs[1], this.crosshairs[0]);
  }

  public void DestroyCrosshairAnimTimer() {
	GD.Print("Destroying crosshair animation timer");
	this.crosshairAnimTimer.Dispose();
  }
}

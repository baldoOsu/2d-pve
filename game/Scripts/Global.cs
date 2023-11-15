using Godot;
using System;

public partial class Global : Node
{
  private int score = 0;
  public int Score
  { get { return this.score; } }

  private int highScore = 0;
  public int HighScore
  { get { return this.highScore; } }

  private Label scoreLabel = null;
  // toggle til at resette scenen ved næste _process cycle
  // dette er nødvendigt for at sikre at alle delegate tråde (timers) er afsluttet, før scenen resetter
  private bool resetScene = false;

  // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    if (resetScene) {
      GD.Print("Reloading current scene");
      GetTree().ReloadCurrentScene();
      // GetTree().CallDeferred("reload_current_scene", null);
      this.resetScene = false;
    }
	}

  public void RenderScore() {
    this.scoreLabel ??= GetNode<Label>("/root/Root/Map/Player/UICanvas/GUI/Score");
    this.scoreLabel.Text = $"{this.score.ToString()} \\ {this.highScore.ToString()}";
  }

  public void IncrementScore(int by=1) {
    this.score += by;
    if (this.score > this.highScore) {
      this.highScore = this.score;
    }
    
    this.RenderScore();
  }

  public void ResetScore() {
    this.score = 0;
  }

  public void ResetGame() {
    // det er nødvendigt at afslutte delegate tråde, ellers vil de køre videre med slettede referencer
    // som hænger og crasher spillet efter restart
    GetNode<Player>("/root/Root/Map/Player").CallDeferred("DestroyCrosshairAnimTimer", null);
    GetNode<Map>("/root/Root/Map").CallDeferred("DestroyEnemySpawnTimer", null);
    this.scoreLabel = null;

    this.ResetScore();
    this.resetScene = true;
  }
}

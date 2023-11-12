using Godot;
using System;

public partial class Global : Node
{
  private int score = 0;
  public int Score
  { 
    get { return this.score; }
    set {
      this.scoreLabel.Text = $"{value.ToString()} / {this.highScore.ToString()}";
      this.score = value; 
    } 
  }

  private int highScore = 0;
  public int HighScore
  { get { return this.highScore; } set { this.highScore = value; } }

  private Label scoreLabel;

  // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    this.scoreLabel = GetNode<Label>("/root/Root/Map/Player/UICanvas/GUI/Score");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

  public void IncrementScore(int by=1) {
    this.score += by;
    this.scoreLabel.Text = $"{this.score.ToString()} / {this.highScore.ToString()}";
  }
}

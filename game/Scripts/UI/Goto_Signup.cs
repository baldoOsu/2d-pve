using Godot;
using System;

public partial class Goto_Signup : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
  	this.Pressed += () => this.GetTree().ChangeSceneToFile("res://Scenes/UI/Sign Up.tscn");
	}
}

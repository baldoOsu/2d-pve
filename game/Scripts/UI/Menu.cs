using Godot;
using System;

public partial class Menu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	Global global = GetNode<Global>("/root/Global");
	if (global.isLoggedIn) {
	  var vbox = GetNode<VBoxContainer>("%BtnsVBox");
	  vbox.RemoveChild(GetNode("%Log In"));
	  vbox.RemoveChild(GetNode("%Sign Up"));

	  Panel panel = GetNode<Panel>("Panel");
	  panel.SetSize(new Vector2(panel.Size.X, 100));

	  panel.SetPosition(new Vector2(455.0f, 267.5f));
	}
	}
}

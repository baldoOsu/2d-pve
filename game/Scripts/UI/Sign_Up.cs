using Godot;
using System;

public partial class Sign_Up : Button
{
	private Label feedbackLabel;
  
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    this.Pressed += this.BtnPressed;
	}

  public async void BtnPressed() {
    string username = GetNode<LineEdit>("%Username").Text;
    string password = GetNode<LineEdit>("%Password").Text;
    string confirmPassword = GetNode<LineEdit>("%Confirm Password").Text;

    if (password != confirmPassword) {
      ChangeLabeltext("Passwords do not match", new Color(0xff0000ff));
      return;
    }

    Global global = GetNode<Global>("/root/Global");
    bool signUpSuccess = await global.Signup(username, password);
    if (!signUpSuccess) {
      ChangeLabeltext($"User {username} already exists", new Color(0xff0000ff));
      return;
    }
    
    GetTree().ChangeSceneToFile("res://Scenes/Root.tscn");
  }

  private void ChangeLabeltext(string text, Color color) {
    if (this.feedbackLabel == null) {
      VBoxContainer vbox = GetNode<VBoxContainer>("%SignupCredsVBox");

      this.feedbackLabel = new Label()
      {
        Visible = true
      };

      vbox.AddChild(feedbackLabel);
    }

    this.feedbackLabel.Text = text;
    this.feedbackLabel.LabelSettings = new LabelSettings()
    {
      FontSize = 12,
      FontColor = color
    };
  }
}

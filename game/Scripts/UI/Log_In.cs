using Godot;
using System;

public partial class Log_In : Button
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

    Global global = GetNode<Global>("/root/Global");
    bool logInSuccess = await global.Login(username, password);
    if (!logInSuccess) {
      ChangeLabeltext("Incorrect credentials", new Color(0xff0000ff));
      return;
    }
    
    GetTree().ChangeSceneToFile("res://Scenes/Root.tscn");
  }

  private void ChangeLabeltext(string text, Color color) {
    if (this.feedbackLabel == null) {
      VBoxContainer vbox = GetNode<VBoxContainer>("%LoginCredsVBox");

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

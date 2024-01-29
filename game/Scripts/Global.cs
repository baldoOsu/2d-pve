using Godot;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;

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
  private bool _gameOver = false;


  private System.Net.Http.HttpClient client = new()
  {
    BaseAddress = new("http://pve-game-api.supernovaa.dev:3006")
  };
  public bool isLoggedIn = false;
  public User user;

  // nødvendigt for ikke at sende opdatering request flere gange på samme tid
  private bool isUpdatingUser = false; 

	public override void _Process(double delta)
	{
    if (this._gameOver && !this.isUpdatingUser) {
      GD.Print("Changing scene to main menu");
      
      GetTree().ChangeSceneToFile("res://Scenes/UI/Menu.tscn");
      this._gameOver = false;
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

  public async void ResetGame() {
    GD.Print("Game over");
    // det er nødvendigt at afslutte delegate tråde, ellers vil de køre videre med slettede referencer
    // som hænger og crasher spillet efter restart
    GetNode<Player>("/root/Root/Map/Player").CallDeferred("DestroyCrosshairAnimTimer", null);
    this.scoreLabel = null;

    this.ResetScore();
    this._gameOver = true;

    if (this.isLoggedIn) {
      this.isUpdatingUser = true;
      if (this.highScore > this.user.HighScore) {
        await UpdateHighscore(this.highScore);
      }
      await IncrementGamesPlayed(this.user.GamesPlayed);
      this.isUpdatingUser = false;
    }
  }

  public async Task<bool> Login(string username, string password) {
    string reqContent = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";

    GD.Print("Sending login request");
    var res = await this.client.PostAsync(
      "/users/login",
      new StringContent(reqContent, Encoding.UTF8, "application/json")
    );


    if (res.IsSuccessStatusCode) {
      string content = await res.Content.ReadAsStringAsync();
      try {
        User tmp = JsonSerializer.Deserialize<User>(content);
        this.user = new(username, password) {
          ID = tmp.ID,
          HighScore = tmp.HighScore,
          GamesPlayed = tmp.GamesPlayed,
          AccessToken = tmp.AccessToken
        };

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tmp.AccessToken);

        this.isLoggedIn = true;
        this.highScore = user.HighScore;

        GD.Print("User login successful");
        GD.Print(user.ToString());
        return true;
      } catch {
        GD.Print(content);
      }
    }

    GD.Print("User login failed...");
    return false;
  }

  public async Task<bool> Signup(string username, string password) {
    string reqContent = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";

    GD.Print("Sending signup request");
    var res = await this.client.PostAsync(
      "/users/signup",
      new StringContent(reqContent, Encoding.UTF8, "application/json")
    );

    if (res.IsSuccessStatusCode) {
      string content = await res.Content.ReadAsStringAsync();
      try {
        User tmp = JsonSerializer.Deserialize<User>(content);
        this.user = new(username, password)
        {
          ID = tmp.ID,
          HighScore = this.highScore,
          GamesPlayed = 0,
          AccessToken = tmp.AccessToken
        };

        
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tmp.AccessToken);
        this.isLoggedIn = true;

        GD.Print("User signup successful");
        GD.Print(user.ToString());
        return true;
      } catch {
        GD.Print(content);
      }
      
    }

    GD.Print("User signup failed...");
    return false;
  }
  
  public async Task<bool> UpdateHighscore(int highScore) {
    string reqContent = $"{{\"highscore\": {highScore}}}";

    GD.Print("Sending highscore update request");
    var res = await this.client.PutAsync(
      $"/users/{this.user.ID}/highscore",
      new StringContent(reqContent, Encoding.UTF8, "application/json")
    );

    if (res.IsSuccessStatusCode) {
      this.user.HighScore = highScore;

      GD.Print($"User highscore has been updated to {highScore}");
      return true;
    }

    GD.Print($"Failed setting user highscore to {highScore}");
    return false;
  }

  public async Task<bool> IncrementGamesPlayed(int gamesPlayed) {
    string reqContent = $"{{\"games_played\": {gamesPlayed}}}";

    GD.Print("Sending games played increment request");
    var res = await this.client.PutAsync(
      $"/users/{this.user.ID}/inc-games-played",
      new StringContent(reqContent, Encoding.UTF8, "application/json")
    );

    if (res.IsSuccessStatusCode) {
      this.user.GamesPlayed += 1;

      GD.Print($"User games played has been updated to {this.user.GamesPlayed}");
      return true;
    }

    GD.Print($"Failed setting user games played to {gamesPlayed+1}");
    return false;
  }
}

public class User {
  public int ID
  { get; set; }
  public string Username
  { get; private set; }

  public string Password
  { get; private set; }

  public int HighScore
  { get; set; }

  public int GamesPlayed
  { get; set; }

  public string AccessToken
  { get; set; }

  public User(string username, string password) {
    this.Username = username;
    this.Password = password;
  }
  public User() {}

  public override string ToString()
  {
    return $"ID: {this.ID}, Username: {this.Username}, Highscore: {this.HighScore}, Games Played: {this.GamesPlayed}";
  }
}

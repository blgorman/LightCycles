using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using Color = SFML.Graphics.Color;
using Font = SFML.Graphics.Font;
using Text = SFML.Graphics.Text;

namespace LightCycles;

public class Program
{
    enum GameState { Menu, Playing, GameOver }
    static GameState state = GameState.Menu;
    static bool twoPlayers = false;

    static void Main()
    {
        uint width = 800;
        uint height = 600;
        RenderWindow window = new RenderWindow(new VideoMode(width, height), "Light Cycles");
        window.Closed += (sender, e) => window.Close();

        var player1Controls = new Dictionary<Keyboard.Key, string>
        {
            { Keyboard.Key.W, "Up" }, { Keyboard.Key.S, "Down" }, { Keyboard.Key.A, "Left" }, { Keyboard.Key.D, "Right" }
        };
        var player2Controls = new Dictionary<Keyboard.Key, string>
        {
            { Keyboard.Key.Up, "Up" }, { Keyboard.Key.Down, "Down" }, { Keyboard.Key.Left, "Left" }, { Keyboard.Key.Right, "Right" }
        };

        Player player1 = null, player2 = null;
        Clock clock = new Clock();
        float moveTimer = 0;
        float moveInterval = 0.03f; // Decreased from 0.05f to 0.03f for faster updates

        Font font;
        try
        {
            font = new Font("arial.ttf");
        }
        catch (SFML.LoadingFailedException ex)
        {
            Console.WriteLine($"Font loading failed: {ex.Message}. No text will be displayed.");
            font = null;
        }

        while (window.IsOpen)
        {
            window.DispatchEvents();

            float deltaTime = clock.Restart().AsSeconds();
            moveTimer += deltaTime;

            if (state == GameState.Menu)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.Num1))
                {
                    twoPlayers = false;
                    player1 = new Player(200, 300, Color.Cyan, player1Controls);
                    player2 = new Player(600, 300, Color.Red, isAI: true);
                    state = GameState.Playing;
                }
                else if (Keyboard.IsKeyPressed(Keyboard.Key.Num2))
                {
                    twoPlayers = true;
                    player1 = new Player(200, 300, Color.Cyan, player1Controls);
                    player2 = new Player(600, 300, Color.Red, player2Controls);
                    state = GameState.Playing;
                }

                window.Clear(Color.Black);
                if (font != null)
                {
                    DrawText(window, font, "Press 1 for 1 Player (vs AI)", 200, 250, Color.White);
                    DrawText(window, font, "Press 2 for 2 Players", 200, 300, Color.White);
                }
                window.Display();
            }
            else if (state == GameState.Playing)
            {
                if (moveTimer >= moveInterval)
                {
                    if (player1.Alive)
                    {
                        foreach (var control in player1.Controls)
                        {
                            if (Keyboard.IsKeyPressed(control.Key))
                            {
                                if ((control.Value == "Up" && player1.Direction != "Down") ||
                                    (control.Value == "Down" && player1.Direction != "Up") ||
                                    (control.Value == "Left" && player1.Direction != "Right") ||
                                    (control.Value == "Right" && player1.Direction != "Left"))
                                    player1.Direction = control.Value;
                            }
                        }
                        player1.Move();
                    }

                    if (player2.Alive)
                    {
                        if (twoPlayers)
                        {
                            foreach (var control in player2.Controls)
                            {
                                if (Keyboard.IsKeyPressed(control.Key))
                                {
                                    if ((control.Value == "Up" && player2.Direction != "Down") ||
                                        (control.Value == "Down" && player2.Direction != "Up") ||
                                        (control.Value == "Left" && player2.Direction != "Right") ||
                                        (control.Value == "Right" && player2.Direction != "Left"))
                                        player2.Direction = control.Value;
                                }
                            }
                            player2.Move();
                        }
                        else
                        {
                            player2.AIMove(player1.Trail, width, height);
                        }
                    }

                    player1.CheckCollision(player2.Trail, width, height);
                    player2.CheckCollision(player1.Trail, width, height);
                    moveTimer = 0;
                }

                window.Clear(Color.Black);
                player1.Draw(window);
                player2.Draw(window);

                if (!player1.Alive || !player2.Alive)
                {
                    state = GameState.GameOver;
                    moveTimer = 0;
                }

                window.Display();
            }
            else if (state == GameState.GameOver)
            {
                window.Clear(Color.Black);
                player1.Draw(window);
                player2.Draw(window);

                if (font != null)
                {
                    string message = !player1.Alive && !player2.Alive ? "Draw!" :
                                     !player1.Alive ? "Red Wins!" : "Cyan Wins!";
                    Color textColor = !player1.Alive && !player2.Alive ? Color.White :
                                      !player1.Alive ? Color.Red : Color.Cyan;
                    DrawText(window, font, message, width / 2 - 100, height / 2 - 25, textColor);
                }

                window.Display();

                if (moveTimer >= 2.0f)
                {
                    state = GameState.Menu;
                    player1 = null;
                    player2 = null;
                }
            }
        }
    }

    static void DrawText(RenderWindow window, Font font, string message, float x, float y, Color color)
    {
        Text text = new Text(message, font, 50) { Position = new Vector2f(x, y), FillColor = color };
        window.Draw(text);
    }

}

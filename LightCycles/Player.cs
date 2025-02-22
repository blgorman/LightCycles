using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Drawing;
using Color = SFML.Graphics.Color;

namespace LightCycles;



class Player
{
    public Vector2f Position { get; set; }
    public Color Color { get; }
    public string Direction { get; set; }
    public List<Vector2f> Trail { get; }
    public bool Alive { get; set; }
    public Dictionary<Keyboard.Key, string> Controls { get; }
    public bool IsAI { get; }

    public Player(float x, float y, Color color, Dictionary<Keyboard.Key, string> controls = null, bool isAI = false)
    {
        Position = new Vector2f(x, y);
        Color = color;
        Direction = "Right";
        Trail = new List<Vector2f> { Position };
        Alive = true;
        Controls = controls ?? new Dictionary<Keyboard.Key, string>();
        IsAI = isAI;
    }

    public void Move()
    {
        float speed = 10f; // Increased from 5f to 10f for faster movement
        if (Direction == "Up") Position = new Vector2f(Position.X, Position.Y - speed);
        else if (Direction == "Down") Position = new Vector2f(Position.X, Position.Y + speed);
        else if (Direction == "Left") Position = new Vector2f(Position.X - speed, Position.Y);
        else if (Direction == "Right") Position = new Vector2f(Position.X + speed, Position.Y);
        Trail.Add(Position);
    }

    public void CheckCollision(List<Vector2f> otherTrail, uint width, uint height)
    {
        if (Position.X < 0 || Position.X >= width || Position.Y < 0 || Position.Y >= height)
            Alive = false;
        else if (Trail.FindLastIndex(p => p == Position) < Trail.Count - 1 || otherTrail.Contains(Position))
            Alive = false;
    }

    public void Draw(RenderWindow window)
    {
        foreach (var point in Trail)
        {
            var rect = new RectangleShape(new Vector2f(5, 5)) { Position = point, FillColor = Color };
            window.Draw(rect);
        }
    }

    public void AIMove(List<Vector2f> otherTrail, uint width, uint height)
    {
        if (!IsAI || !Alive) return;

        Vector2f nextPos = PredictNextPosition();
        if (nextPos.X < 0 || nextPos.X >= width || nextPos.Y < 0 || nextPos.Y >= height ||
            Trail.Contains(nextPos) || otherTrail.Contains(nextPos))
        {
            string[] directions = { "Right", "Left", "Up", "Down" };
            foreach (var newDir in directions)
            {
                if (CanTurn(newDir))
                {
                    Vector2f testPos = PredictNextPosition(newDir);
                    if (testPos.X >= 0 && testPos.X < width && testPos.Y >= 0 && testPos.Y < height &&
                        !Trail.Contains(testPos) && !otherTrail.Contains(testPos))
                    {
                        Direction = newDir;
                        break;
                    }
                }
            }
        }
        Move();
    }

    private Vector2f PredictNextPosition(string dir = null)
    {
        float speed = 10f; // Match the increased speed here
        string testDir = dir ?? Direction;
        if (testDir == "Up") return new Vector2f(Position.X, Position.Y - speed);
        if (testDir == "Down") return new Vector2f(Position.X, Position.Y + speed);
        if (testDir == "Left") return new Vector2f(Position.X - speed, Position.Y);
        return new Vector2f(Position.X + speed, Position.Y);
    }

    private bool CanTurn(string newDir)
    {
        return (newDir == "Up" && Direction != "Down") ||
               (newDir == "Down" && Direction != "Up") ||
               (newDir == "Left" && Direction != "Right") ||
               (newDir == "Right" && Direction != "Left");
    }
}


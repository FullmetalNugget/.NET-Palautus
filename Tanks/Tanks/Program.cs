using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;

namespace Tanks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Run();
        }
    }

    public class Game
    {
        const int ScreenWidth = 800;
        const int ScreenHeight = 600;

        List<Wall> walls = new List<Wall>();
        Tank player1;
        Tank player2;

        public void Run()
        {
            Init();
            GameLoop();
        }

        private void Init()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Tanks - Simple Raylib C#");
            Raylib.SetTargetFPS(60);

            walls.Clear();
            walls.Add(new Wall(300, 150, 200, 24));
            walls.Add(new Wall(100, 350, 24, 200));
            walls.Add(new Wall(500, 400, 200, 24));

            player1 = new Tank(
                startPosition: new Vector2(80, ScreenHeight / 2),
                color: Color.Blue,
                up: KeyboardKey.W,
                down: KeyboardKey.S,
                left: KeyboardKey.A,
                right: KeyboardKey.D,
                shoot: KeyboardKey.Space,
                bounds: new Rectangle(0, 0, ScreenWidth, ScreenHeight)
            );

            player2 = new Tank(
                startPosition: new Vector2(ScreenWidth - 80, ScreenHeight / 2),
                color: Color.Red,
                up: KeyboardKey.Up,
                down: KeyboardKey.Down,
                left: KeyboardKey.Left,
                right: KeyboardKey.Right,
                shoot: KeyboardKey.RightControl,
                bounds: new Rectangle(0, 0, ScreenWidth, ScreenHeight)
            );

            player1.OtherTank = player2;
            player2.OtherTank = player1;
        }

        private void GameLoop()
        {
            while (!Raylib.WindowShouldClose())
            {
                UpdateGame();
                DrawGame();
            }

            Raylib.CloseWindow();
        }

        private void UpdateGame()
        {
            float dt = Raylib.GetFrameTime();

            player1.Update(dt, walls);
            player2.Update(dt, walls);

            player1.Bullet?.Update(dt, walls, OnBulletHitTank);
            player2.Bullet?.Update(dt, walls, OnBulletHitTank);
        }

        private void OnBulletHitTank(Bullet bullet, Tank victim)
        {
            if (bullet == null || victim == null) return;

            Tank shooter = (bullet.Owner == player1) ? player1 : player2;
            shooter.Score++;

            player1.ResetToStart();
            player2.ResetToStart();

            player1.Bullet?.Deactivate();
            player2.Bullet?.Deactivate();
        }

        private void DrawGame()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Green);

            foreach (var w in walls) w.Draw();

            player1.Draw();
            player2.Draw();

            player1.Bullet?.Draw();
            player2.Bullet?.Draw();

            Raylib.DrawText($"Player 1: {player1.Score}", 10, 10, 20, Color.White);
            Raylib.DrawText($"P1: WASD move, SPACE shoot", 10, 34, 12, Color.Gray);

            Raylib.DrawText($"Player 2: {player2.Score}", ScreenWidth - 170, 10, 20, Color.White);
            Raylib.DrawText($"P2: Arrows move, RCTRL shoot", ScreenWidth - 230, 34, 12, Color.Gray);

            Raylib.EndDrawing();
        }
    }

    public class Tank
    {
        public Vector2 Position;
        public Vector2 StartPosition;
        Vector2 previousPosition;

        public Vector2 Direction;
        readonly Vector2 tankSize = new Vector2(36, 36);
        readonly Vector2 turretSize = new Vector2(18, 8);

        public Color Color;

        readonly KeyboardKey keyUp;
        readonly KeyboardKey keyDown;
        readonly KeyboardKey keyLeft;
        readonly KeyboardKey keyRight;
        readonly KeyboardKey keyShoot;

        readonly float speed = 160f;

        public Bullet Bullet;
        double lastShootTime = 0;
        readonly double shootInterval = 0.8;

        public int Score = 0;

        readonly Rectangle bounds;

        public Tank OtherTank;

        public Tank(Vector2 startPosition, Color color, KeyboardKey up, KeyboardKey down, KeyboardKey left, KeyboardKey right, KeyboardKey shoot, Rectangle bounds)
        {
            StartPosition = startPosition;
            Position = startPosition;
            previousPosition = Position;
            Direction = new Vector2(0, -1);
            Color = color;

            keyUp = up;
            keyDown = down;
            keyLeft = left;
            keyRight = right;
            keyShoot = shoot;

            this.bounds = bounds;
        }

        public void ResetToStart()
        {
            Position = StartPosition;
            previousPosition = Position;
            Direction = new Vector2(0, -1);
        }

        public void Update(float dt, List<Wall> walls)
        {
            previousPosition = Position;

            Vector2 vel = Vector2.Zero;

            if (Raylib.IsKeyDown(keyUp))
            {
                vel.Y -= 1;
                Direction = new Vector2(0, -1);
            }
            else if (Raylib.IsKeyDown(keyDown))
            {
                vel.Y += 1;
                Direction = new Vector2(0, 1);
            }

            if (Raylib.IsKeyDown(keyLeft))
            {
                vel.X -= 1;
                Direction = new Vector2(-1, 0);
            }
            else if (Raylib.IsKeyDown(keyRight))
            {
                vel.X += 1;
                Direction = new Vector2(1, 0);
            }

            if (vel != Vector2.Zero)
            {
                vel = Vector2.Normalize(vel) * speed;
            }

            Position += vel * dt;

            var half = tankSize / 2f;
            if (Position.X - half.X < bounds.X) Position.X = bounds.X + half.X;
            if (Position.X + half.X > bounds.X + bounds.Width) Position.X = bounds.X + bounds.Width - half.X;
            if (Position.Y - half.Y < bounds.Y) Position.Y = bounds.Y + half.Y;
            if (Position.Y + half.Y > bounds.Y + bounds.Height) Position.Y = bounds.Y + bounds.Height - half.Y;

            Rectangle myRect = GetRect();
            foreach (var w in walls)
            {
                if (Raylib.CheckCollisionRecs(myRect, w.Rect))
                {
                    Position = previousPosition;
                    myRect = GetRect();
                }
            }

            if (OtherTank != null)
            {
                if (Raylib.CheckCollisionRecs(myRect, OtherTank.GetRect()))
                {
                    Position = previousPosition;
                }
            }

            if (Raylib.IsKeyDown(keyShoot))
            {
                double t = Raylib.GetTime();
                if (t - lastShootTime > shootInterval)
                {
                    Shoot();
                    lastShootTime = t;
                }
            }

            if (Bullet == null)
            {
                Bullet = new Bullet();
                Bullet.Deactivate();
                Bullet.Owner = this;
            }
        }

        public void Shoot()
        {
            if (Bullet == null)
            {
                Bullet = new Bullet();
                Bullet.Owner = this;
            }

            if (Bullet.Active) return;

            Vector2 spawn = Position + Direction * ((tankSize.X / 2f) + (Bullet.Radius + 2));
            Bullet.Spawn(spawn, Direction, this);
        }

        public void Draw()
        {
            Vector2 topLeft = Position - tankSize / 2f;
            Raylib.DrawRectangleV(topLeft, tankSize, Color);

            Vector2 turretPos = Position + Direction * (tankSize.X / 2.0f + turretSize.X / 2.0f);
            Vector2 turretTopLeft = turretPos - turretSize / 2.0f;
            Raylib.DrawRectangleV(turretTopLeft, turretSize, Color.Black);
        }

        public Rectangle GetRect()
        {
            Vector2 topLeft = Position - tankSize / 2f;
            return new Rectangle(topLeft.X, topLeft.Y, tankSize.X, tankSize.Y);
        }
    }

    public class Bullet
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed = 380f;
        public float Radius = 6f;
        public bool Active = false;
        public Tank Owner;

        public void Spawn(Vector2 position, Vector2 direction, Tank owner)
        {
            Position = position;
            Direction = direction;
            Owner = owner;
            Active = true;
        }

        public void Deactivate()
        {
            Active = false;
        }

        public void Update(float dt, List<Wall> walls, Action<Bullet, Tank> onHitTank)
        {
            if (!Active) return;

            Position += Direction * Speed * dt;

            if (Position.X < 0 || Position.X > Raylib.GetScreenWidth() || Position.Y < 0 || Position.Y > Raylib.GetScreenHeight())
            {
                Deactivate();
                return;
            }

            foreach (var w in walls)
            {
                if (Raylib.CheckCollisionCircleRec(Position, Radius, w.Rect))
                {
                    Deactivate();
                    return;
                }
            }

            var other = Owner?.OtherTank;
            if (other != null)
            {
                if (Raylib.CheckCollisionCircleRec(Position, Radius, other.GetRect()))
                {
                    onHitTank?.Invoke(this, other);
                    Deactivate();
                }
            }
        }

        public void Draw()
        {
            if (!Active) return;
            Raylib.DrawCircleV(Position, Radius, Color.Black);
        }
    }

    public class Wall
    {
        public Rectangle Rect;

        public Wall(float x, float y, float w, float h)
        {
            Rect = new Rectangle(x, y, w, h);
        }

        public void Draw()
        {
            Raylib.DrawRectangleRec(Rect, Color.Gray);
        }
    }
}

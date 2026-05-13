using System.Numerics;
using Raylib_cs;
using FullmetalLibrary;

class Program
{
    const int screenWidth = 1200;
    const int screenHeight = 800;

    static Player player;

    static List<Asteroid> asteroids = new();
    static List<Bullet> bullets = new();

    static int lives = 3;

    static void Main()
    {
        Raylib.InitWindow(screenWidth, screenHeight, "Asteroids");
        Raylib.SetTargetFPS(60);

        Player();
        Asteroids();

        while (!Raylib.WindowShouldClose())
        {
            Update();
            Draw();
        }

        Raylib.CloseWindow();
    }

    static void Player()
    {
        player = new Player()
        {
            Position = new Vector2(screenWidth / 2, screenHeight / 2),
            Velocity = Vector2.Zero,
            Rotation = 0,
            Radius = 25
        };
    }

    static void Asteroids()
    {
        for (int i = 0; i < 6; i++)
        {
            Asteroid asteroid = new Asteroid();

            asteroid.Size = 4;

            asteroid.Position = new Vector2(
                Random.Shared.Next(0, screenWidth),
                Random.Shared.Next(0, screenHeight)
            );

            asteroid.Velocity = new Vector2(
                RandomFloat(-2f, 2f),
                RandomFloat(-2f, 2f)
            );

            asteroid.Radius = asteroid.Size * 15;

            asteroids.Add(asteroid);
        }
    }

    static void Update()
    {
        Input();

        MoveObject(player);

        foreach (var asteroid in asteroids)
            MoveObject(asteroid);

        foreach (var bullet in bullets)
        {
            MoveObject(bullet);
            bullet.LifeTime--;

            if (bullet.LifeTime <= 0)
                bullet.Destroyed = true;
        }

        BulletCollisions();
        Collision();

        bullets.RemoveAll(b => b.Destroyed);
        asteroids.RemoveAll(a => a.Destroyed);
    }

    static void Input()
    {
        if (Raylib.IsKeyDown(KeyboardKey.A))
            player.Rotation -= 4f;

        if (Raylib.IsKeyDown(KeyboardKey.D))
            player.Rotation += 4f;

        if (Raylib.IsKeyDown(KeyboardKey.W))
        {
            float r = DegreesToRadians(player.Rotation - 90);

            Vector2 thrust = new(
                MathF.Cos(r),
                MathF.Sin(r)
            );

            player.Velocity += thrust * 0.2f;
        }

        player.Velocity *= 0.99f;

        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            Shoot();
    }

    static void Shoot()
    {
        float r = DegreesToRadians(player.Rotation - 90);

        Vector2 dir = new(
            MathF.Cos(r),
            MathF.Sin(r)
        );

        bullets.Add(new Bullet()
        {
            Position = player.Position,
            Velocity = dir * 10f + player.Velocity,
            Radius = 5,
            LifeTime = 120
        });
    }

    static void MoveObject(FullmetalObject obj)
    {
        obj.Position += obj.Velocity;

        if (obj.Position.X < 0) obj.Position.X = screenWidth;
        if (obj.Position.X > screenWidth) obj.Position.X = 0;

        if (obj.Position.Y < 0) obj.Position.Y = screenHeight;
        if (obj.Position.Y > screenHeight) obj.Position.Y = 0;
    }

    static void BulletCollisions()
    {
        List<Asteroid> newAsteroids = new();

        foreach (var bullet in bullets)
        {
            foreach (var asteroid in asteroids)
            {
                if (bullet.Destroyed || asteroid.Destroyed)
                    continue;

                float dist = Vector2.Distance(bullet.Position, asteroid.Position);

                if (dist < bullet.Radius + asteroid.Radius)
                {
                    bullet.Destroyed = true;
                    asteroid.Destroyed = true;

                    if (asteroid.Size > 1)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            newAsteroids.Add(new Asteroid()
                            {
                                Size = asteroid.Size - 1,
                                Position = asteroid.Position,
                                Velocity = new Vector2(
                                    RandomFloat(-4f, 4f),
                                    RandomFloat(-4f, 4f)
                                ),
                                Radius = (asteroid.Size - 1) * 15
                            });
                        }
                    }
                }
            }
        }

        asteroids.AddRange(newAsteroids);
    }

    static void Collision()
    {
        foreach (var asteroid in asteroids)
        {
            float dist = Vector2.Distance(player.Position, asteroid.Position);

            if (dist < player.Radius + asteroid.Radius)
            {
                lives--;

                if (lives <= 0)
                    Raylib.CloseWindow();

                player.Position = new Vector2(screenWidth / 2, screenHeight / 2);
                player.Velocity = Vector2.Zero;

                bullets.Clear();
                asteroids.Clear();

               Asteroids();

                break;
            }
        }
    }

    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        DrawPlayer();

        foreach (var asteroid in asteroids)
            DrawAsteroid(asteroid);

        foreach (var bullet in bullets)
            DrawBullet(bullet);

        DrawLives();

        Raylib.EndDrawing();
    }

    static void DrawPlayer()
    {
        string text = "A";

        Vector2 size = Raylib.MeasureTextEx(
            Raylib.GetFontDefault(),
            text,
            60,
            1
        );

        Raylib.DrawTextPro(
            Raylib.GetFontDefault(),
            text,
            player.Position,
            size / 2,
            player.Rotation,
            60,
            1,
            Color.White
        );
    }

    static void DrawAsteroid(Asteroid asteroid)
    {
        Raylib.DrawCircleV(
            asteroid.Position,
            asteroid.Radius,
            Color.Gray
        );
    }

    static void DrawBullet(Bullet bullet)
    {
        Raylib.DrawCircleV(
            bullet.Position,
            bullet.Radius,
            Color.Yellow
        );
    }

    static void DrawLives()
    {
        string text = "";

        for (int i = 0; i < lives; i++)
            text += "A ";

        Raylib.DrawText(text, 20, 20, 40, Color.White);
    }

    static float DegreesToRadians(float degrees)
        => degrees * MathF.PI / 180f;

    static float RandomFloat(float min, float max)
        => (float)(Random.Shared.NextDouble() * (max - min) + min);
}

class FullmetalObject
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float Radius;
    public bool Destroyed;
}

class Player : FullmetalObject { }
class Asteroid : FullmetalObject { public int Size; }
class Bullet : FullmetalObject { public int LifeTime; }
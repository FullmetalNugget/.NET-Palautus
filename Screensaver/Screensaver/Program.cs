namespace Screensaver
{
    using System.Numerics;
    using Raylib_cs;
    using System;

    internal class Program
    {
        static Vector2 Vector2Scale(Vector2 v, float scale)
        {
            return new Vector2(v.X * scale, v.Y * scale);
        }

        static Vector2 Vector2Add(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        static void Main(string[] args)
        {
            Raylib.InitWindow(800, 800, "Screensaver");
            Raylib.SetTargetFPS(60);

            Vector2 TA = new Vector2(Raylib.GetScreenWidth() / 2f, 0f);
            Vector2 TB = new Vector2(0f, Raylib.GetScreenHeight() / 2f);
            Vector2 TC = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight() * 3/4);

            Vector2 speedA = new Vector2(1, 1);
            Vector2 speedB = new Vector2(1, -1);
            Vector2 speedC = new Vector2(-1, 1);

            float nopeus = 100f;

            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                int screenWidth = Raylib.GetScreenWidth();
                int screenHeight = Raylib.GetScreenHeight();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                Raylib.DrawLineV(TA, TB, Color.Yellow);
                Raylib.DrawLineV(TB, TC, Color.Blue);
                Raylib.DrawLineV(TC, TA, Color.Green);

                TA = Vector2Add(TA, Vector2Scale(speedA, nopeus * deltaTime));
                TB = Vector2Add(TB, Vector2Scale(speedB, nopeus * deltaTime));
                TC = Vector2Add(TC, Vector2Scale(speedC, nopeus * deltaTime));

                if (TA.X < 0 || TA.X > screenWidth) speedA.X *= -1f;
                if (TA.Y < 0 || TA.Y > screenHeight) speedA.Y *= -1f;

                if (TB.X < 0 || TB.X > screenWidth) speedB.X *= -1f;
                if (TB.Y < 0 || TB.Y > screenHeight) speedB.Y *= -1f;

                if (TC.X < 0 || TC.X > screenWidth) speedC.X *= -1f;
                if (TC.Y < 0 || TC.Y > screenHeight) speedC.Y *= -1f;

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}

using Raylib_cs;
using System.Numerics;

namespace PONG
{
    internal class Program
    {
        static void Main()
        {
            const int screenWidth = 800;
            const int screenHeight = 450;

            Raylib.InitWindow(screenWidth, screenHeight, "Pong");
            Raylib.SetTargetFPS(60);

            float paddleSpeed = 400f;
            float paddleWidth = 15f;
            float paddleHeight = 80f;

            Vector2 leftPaddlePos = new Vector2(40, screenHeight / 2 - paddleHeight / 2);
            Vector2 rightPaddlePos = new Vector2(screenWidth - 55, screenHeight / 2 - paddleHeight / 2);

            int leftScore = 0;
            int rightScore = 0;

            Vector2 ballPos = new Vector2(screenWidth / 2, screenHeight / 2);
            Vector2 ballDir = new Vector2(1, 1);
            float ballSpeed = 300f;

            while (!Raylib.WindowShouldClose())
            {
                float fps = Raylib.GetFrameTime();

                if (Raylib.IsKeyDown(KeyboardKey.W))
                    leftPaddlePos.Y -= paddleSpeed * fps;
                if (Raylib.IsKeyDown(KeyboardKey.S))
                    leftPaddlePos.Y += paddleSpeed * fps;

                if (Raylib.IsKeyDown(KeyboardKey.Up))
                    rightPaddlePos.Y -= paddleSpeed * fps;
                if (Raylib.IsKeyDown(KeyboardKey.Down))
                    rightPaddlePos.Y += paddleSpeed * fps;

                leftPaddlePos.Y = Math.Clamp(leftPaddlePos.Y, 0, screenHeight - paddleHeight);
                rightPaddlePos.Y = Math.Clamp(rightPaddlePos.Y, 0, screenHeight - paddleHeight);

                ballPos += ballDir * ballSpeed * fps;

                if (ballPos.Y <= 0 || ballPos.Y >= screenHeight)
                    ballDir.Y *= -1;

                Rectangle leftPaddle = new Rectangle(leftPaddlePos.X, leftPaddlePos.Y, paddleWidth, paddleHeight);
                Rectangle rightPaddle = new Rectangle(rightPaddlePos.X, rightPaddlePos.Y, paddleWidth, paddleHeight);

                if (Raylib.CheckCollisionPointRec(ballPos, leftPaddle) ||
                    Raylib.CheckCollisionPointRec(ballPos, rightPaddle))
                {
                    ballDir.X *= -1;
                }

                if (ballPos.X < 0)
                {
                    rightScore++;
                    ballPos = new Vector2(screenWidth / 2, screenHeight / 2);
                }

                if (ballPos.X > screenWidth)
                {
                    leftScore++;
                    ballPos = new Vector2(screenWidth / 2, screenHeight / 2);
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                Raylib.DrawRectangleRec(leftPaddle, Color.Green);
                Raylib.DrawRectangleRec(rightPaddle, Color.Red);
                Raylib.DrawCircleV(ballPos, 10, Color.Blue);

                Raylib.DrawText(leftScore.ToString(), screenWidth / 4, 20, 30, Color.Green);
                Raylib.DrawText(rightScore.ToString(), screenWidth * 3 / 4, 20, 30, Color.Red);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
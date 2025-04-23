using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MazeGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Screen dimensions
    private int screenWidth = 800;
    private int screenHeight = 600;

    // Player variables
    private Vector2 playerPosition = new Vector2(100, 100);
    private float playerAngle = 0f;
    private float fov = MathHelper.PiOver4; // Field of View (45 degrees)
    private float moveSpeed = 2f;
    private float rotationSpeed = 0.05f;

    // Maze grid (1 = wall, 0 = empty space)

    //insert maze generator here instead of... 
    private int[,] maze = new int[,]
    {
        { 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1 },
    };
    //That ^^^
    private int cellSize = 64;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Player rotation
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
            playerAngle -= rotationSpeed;
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
            playerAngle += rotationSpeed;

        // Player movement
        Vector2 direction = new Vector2((float)Math.Cos(playerAngle), (float)Math.Sin(playerAngle));
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
            playerPosition += direction * moveSpeed;
        if (Keyboard.GetState().IsKeyDown(Keys.Down))
            playerPosition -= direction * moveSpeed;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw the 3D walls
        CastRays();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void CastRays()
    {
        int numRays = screenWidth; // One ray per screen column
        float angleStep = fov / numRays;

        for (int i = 0; i < numRays; i++)
        {
            // Calculate the ray angle
            float rayAngle = playerAngle - (fov / 2) + (i * angleStep);

            // Perform raycasting
            float distanceToWall = CastSingleRay(rayAngle);

            // Calculate wall height based on distance
            float wallHeight = screenHeight / (distanceToWall + 0.0001f); // Avoid division by zero
            wallHeight = MathHelper.Clamp(wallHeight, 0, screenHeight);

            // Draw the wall slice
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            _spriteBatch.Draw(
                pixel,
                new Rectangle(i, (int)((screenHeight - wallHeight) / 2), 1, (int)wallHeight),
                Color.Gray
            );
        }
    }

    private float CastSingleRay(float rayAngle)
    {
        // Ray direction vector
        Vector2 rayDirection = new Vector2((float)Math.Cos(rayAngle), (float)Math.Sin(rayAngle));

        // Current ray position
        Vector2 rayPos = playerPosition;

        // Perform Digital Differential Analysis (DDA)
        while (true)
        {
            // Calculate which grid cell the ray is in
            int gridX = (int)(rayPos.X / cellSize);
            int gridY = (int)(rayPos.Y / cellSize);

            // Check if the ray hit a wall
            if (gridX >= 0 && gridX < maze.GetLength(1) && gridY >= 0 && gridY < maze.GetLength(0))
            {
                if (maze[gridY, gridX] == 1)
                {
                    // Return the distance to the wall
                    return Vector2.Distance(playerPosition, rayPos);
                }
            }
            else
            {
                // Ray is out of bounds
                return float.MaxValue;
            }

            // Move the ray forward
            rayPos += rayDirection * 1f; // Step size
        }
    }
}

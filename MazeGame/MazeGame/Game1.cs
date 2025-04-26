using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Win32;
using System;
using System.Collections.Generic;

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
    private int moveSpeed = 1;
    private float rotationSpeed = 0.05f;

    // Maze grid (1 = wall, 0 = empty space)
    //insert maze generator here instead of... 
    //private int[,] maze = new int[,]
    //{
    //    { 1, 1, 1, 1, 1, 1 },
    //    { 1, 0, 0, 0, 0, 1 },
    //    { 1, 0, 1, 1, 0, 1 },
    //    { 1, 0, 0, 0, 0, 1 },
    //    { 1, 1, 1, 1, 1, 1 },
    //};
    //That ^^
    private int cellSize = 35;

    class MazeGenerator
{
    private static int width = 5;
    private static int height = 5;
    public static int[,] maze;
    private static Random rand = new Random();

    public static void Maze()
    {
        width = 21; // Must be odd
        height = 21; // Must be odd
        maze = new int[width, height];

        GenerateMaze();
    }

    private static void GenerateMaze()
    {
        // Initialize the maze with walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;
            }
        }

        // Start the maze generation from the top-left corner
        CarvePassage(1, 1);
    }

    private static void CarvePassage(int cx, int cy)
    {
        // Directions: up, right, down, left
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        // Randomize directions
        List<int> directions = new List<int> { 0, 1, 2, 3 };
        for (int i = 0; i < directions.Count; i++)
        {
            int temp = directions[i];
            int randomIndex = rand.Next(i, directions.Count);
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }

        // Carve passages
        foreach (int direction in directions)
        {
            int nx = cx + dx[direction] * 2;
            int ny = cy + dy[direction] * 2;

            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && maze[nx, ny] == 0)
            {
                maze[cx + dx[direction], cy + dy[direction]] = 1;
                maze[nx, ny] = 1;
                CarvePassage(nx, ny);
            }
        }
    }
}

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
        {
            playerAngle -= rotationSpeed / 2;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            playerAngle += rotationSpeed / 2;
        }    

        // Player movement
        Vector2 direction = new Vector2((float)Math.Cos(playerAngle), (float)Math.Sin(playerAngle));
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            playerPosition += direction * moveSpeed;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            playerPosition -= direction * moveSpeed;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw the 3D walls
        MazeGenerator.Maze();
        CastRays();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void CastRays()
    {
        int numRays = screenWidth; // One ray per screen column
        float angleStep = fov / numRays;
        Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        for (int i = 0; i < numRays; i++)
        {
            // Calculate the ray angle
            float rayAngle = playerAngle - (fov / 2) + (i * angleStep);

            // Perform raycasting
            float distanceToWall = CastSingleRay(rayAngle);

            // Calculate wall height based on distance
            float wallHeight = screenHeight * 5 / (distanceToWall + 0.0001f); // Avoid division by zero
            wallHeight = MathHelper.Clamp(wallHeight, 0, screenHeight);

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
            if (gridX >= 0 && gridX < MazeGenerator.maze.GetLength(1) && gridY >= 0 && gridY < MazeGenerator.maze.GetLength(0))
            {
                if (MazeGenerator.maze[gridY, gridX] == 1)
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


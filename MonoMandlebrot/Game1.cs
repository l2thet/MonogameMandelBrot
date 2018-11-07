using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MonoMandlebrot
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Dictionary<Vector2, int> escapedPoints;
        Dictionary<Vector2, Vector2> pointToComplexMapping;
        Texture2D image;
        
        float xMin = -2f;
        float xMax = 2f;
        float yMin = -2f;
        float yMax = 2f;

        float ComplexX = -2f;
        float ComplexY = 2f;
        float XOffset = 0f;
        float YOffset = 0f;

        const int displayWidth = 600;
        const int displayHeight = 600;


        bool reRunNumbers = true;
        bool runDraw = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = displayHeight;
            graphics.PreferredBackBufferWidth = displayWidth;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            escapedPoints = new Dictionary<Vector2, int>();
            pointToComplexMapping = new Dictionary<Vector2, Vector2>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            image = new Texture2D(GraphicsDevice, 1, 1);
            Color[] pixelColor = new Color[1];
            pixelColor[0] = Color.White;
            image.SetData(pixelColor);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();
            var xZoomFactor = (xMax - xMin) / displayWidth * 10;
            var yZoomFactor = (yMax - yMin) / displayHeight * 10;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Left))
            {
                XOffset -= (xMax - xMin) / displayWidth * 10;
                reRunNumbers = !reRunNumbers;
            }
            if (state.IsKeyDown(Keys.Right))
            {
                XOffset += (xMax - xMin) / displayWidth * 10;
                reRunNumbers = !reRunNumbers;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                YOffset -= (yMax - yMin) / displayHeight * 10;
                reRunNumbers = !reRunNumbers;
            }
            if (state.IsKeyDown(Keys.Down))
            {
                YOffset += (yMax - yMin) / displayHeight * 10;
                reRunNumbers = !reRunNumbers;
            }
            if(state.IsKeyDown(Keys.OemPlus) || state.IsKeyDown(Keys.Add))
            {
                xMin += xZoomFactor;
                xMax -= xZoomFactor;
                yMin += yZoomFactor;
                yMax -= yZoomFactor;
                reRunNumbers = !reRunNumbers;
            }
            if (state.IsKeyDown(Keys.OemMinus) || state.IsKeyDown(Keys.Subtract))
            {
                xMin -= xZoomFactor;
                xMax += xZoomFactor;
                yMin -= yZoomFactor;
                yMax += yZoomFactor;
                reRunNumbers = !reRunNumbers;
            }


            if (ms.LeftButton == ButtonState.Pressed && IsActive)
            {
                runDraw = true;
                float originalXIncrement = (xMax - xMin) / displayWidth;
                float originalYIncrement = (yMax - yMin) / displayHeight;
                XOffset = 0;
                YOffset = 0;
                Vector2 complexClickResult = pointToComplexMapping[new Vector2(ms.Position.X, ms.Position.Y)];
                escapedPoints.Clear();
                pointToComplexMapping.Clear();
                xMin = complexClickResult.X - (originalXIncrement * 300);
                xMax = complexClickResult.X + (originalXIncrement * 300);
                yMin = complexClickResult.Y - (originalYIncrement * 300);
                yMax = complexClickResult.Y + (originalYIncrement * 300);

                xMin += xZoomFactor;
                xMax -= xZoomFactor;
                yMin += yZoomFactor;
                yMax -= yZoomFactor;

                Vector2 startingComplexPoint = new Vector2();
                startingComplexPoint.X = complexClickResult.X - originalXIncrement * 300;
                startingComplexPoint.Y = complexClickResult.Y + originalYIncrement * 300;

                GeneratePoints(startingComplexPoint.X, startingComplexPoint.Y);
            }

            if (reRunNumbers)
            {
                runDraw = true;
                escapedPoints.Clear();
                pointToComplexMapping.Clear();
                GeneratePoints(null,null);
                reRunNumbers = !reRunNumbers;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if(runDraw)
            {
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(1));

                foreach (KeyValuePair<Vector2, int> escapedPoint in escapedPoints)
                {
                    if (escapedPoint.Value > 1)
                    {
                        double t = (double)escapedPoint.Value / (double)256;
                        int r = (int)(9 * (1 - t) * t * t * t * 255);
                        int g = (int)(15 * (1 - t) * (1 - t) * t * t * 255);
                        int b = (int)(8.5 * (1 - t) * (1 - t) * (1 - t) * t * 255);
                        spriteBatch.Draw(image, escapedPoint.Key, new Color(r, g, b, 255));
                    }
                    else
                    {
                        spriteBatch.Draw(image, escapedPoint.Key, new Color(0, 0, 0, 255));
                    }
                }
                spriteBatch.End();

                base.Draw(gameTime);
                runDraw = false;
            }
        }

        private Vector2 squareComplexNumber(Vector2 complexNumber)
        {
            Vector2 result = new Vector2(complexNumber.X, complexNumber.Y);
            result.X = (complexNumber.X * complexNumber.X) - (complexNumber.Y * complexNumber.Y);
            result.Y = 2 * complexNumber.X * complexNumber.Y;
            return result;
        }

        private Vector2 convertPointToPixel(Vector2 point)
        {
            Vector2 pixel = new Vector2(
                (point.X - xMin) / ((xMax - xMin) / displayWidth),
                (point.Y - yMin) / ((yMax - yMin) / displayHeight)
                );
            return pixel;
        }

        private void runSomeNumbers()
        {
            
            for (float x = xMin; x <= xMax; x += ((xMax - xMin) / displayWidth))
            {
                for (float y = yMin; y <= yMax; y += ((yMax - yMin) / displayHeight))
                {

                    Vector2 originalPoint = new Vector2(x, y);
                    Vector2 newPoint = new Vector2(x, y);
                    int whilecounter = 0;
                    while (newPoint.X >= -2F && newPoint.X <= 2F && newPoint.Y >= -2F && newPoint.Y <= 2F && whilecounter <= 120)
                    {
                        newPoint = squareComplexNumber(newPoint);
                        newPoint.X += originalPoint.X;
                        newPoint.Y += originalPoint.Y;
                        whilecounter++;
                    }
                    if ((newPoint.X > 2F || newPoint.X < -2F || newPoint.Y < -2f || newPoint.Y > 2F) && whilecounter <= 120)
                    {
                        //The escaped points are actually the interesting ones.
                        Vector2 escapedPixelFromPoint = convertPointToPixel(originalPoint);
                        escapedPoints.Add(escapedPixelFromPoint, whilecounter);
                    }
                    else
                    {
                        //Not drawing the escaped points as that is boring
                        //Vector2 pixelFromPoint = convertPointToPixel(originalPoint);
                        //trappedPoints.Add(pixelFromPoint);
                    }
                }
            }
        }

        private void GeneratePoints(float? complexX, float? complexY)
        {
            for (int pointy = 0; pointy <= displayHeight; pointy++)
            {
                if (pointy == 0)
                {
                    if(!complexY.HasValue)
                    {
                        ComplexY = 2f - YOffset;
                    }
                    else
                    {
                        ComplexY = complexY.Value;
                    }
                }
                else
                {
                    ComplexY -= (yMax - yMin) / displayHeight;
                }
                for (int pointx = 0; pointx < displayWidth; pointx++)
                {
                    if(pointx == 0)
                    {
                        if(!complexX.HasValue)
                        {
                            ComplexX = -2f + XOffset;
                        }
                        else
                        {
                            ComplexX = complexX.Value;
                        }
                    }
                    else
                    {
                        ComplexX += (xMax - xMin) / displayWidth;
                    }
                    RunEscapeLoop(pointx, pointy);
                }
            }
        }

        private void RunEscapeLoop(float pointx, float pointy)
        {
            Vector2 originalPoint = new Vector2(ComplexX, ComplexY);
            Vector2 newPoint = new Vector2(ComplexX, ComplexY);
            int whilecounter = 0;
            while (newPoint.X >= -2F && newPoint.X <= 2F && newPoint.Y >= -2F && newPoint.Y <= 2F && whilecounter <= 120)
            {
                newPoint = squareComplexNumber(newPoint);
                newPoint.X += originalPoint.X;
                newPoint.Y += originalPoint.Y;
                whilecounter++;
            }
            if ((newPoint.X > 2F || newPoint.X < -2F || newPoint.Y < -2f || newPoint.Y > 2F) && whilecounter <= 120)
            {
                //The escaped points are actually the interesting ones.
                escapedPoints.Add(new Vector2(pointx, pointy), whilecounter);
                pointToComplexMapping.Add(new Vector2(pointx, pointy), new Vector2(originalPoint.X, originalPoint.Y));
            }
            else
            {
                escapedPoints.Add(new Vector2(pointx, pointy), 0);
                pointToComplexMapping.Add(new Vector2(pointx, pointy), new Vector2(originalPoint.X, originalPoint.Y));
            }
        }
    }
}

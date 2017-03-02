using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _3D_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;    // Added the public to make this accessible.
        public Camera camera { get; protected set; }
        ModelManager modelManager;
        public Random rnd { get; protected set; }
        float shotSpeed = 10;
        int shotDelay = 300;
        int shotCountdown = 0;
        Texture2D crosshairTexture;
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        Cue trackCue;
        // Text Annotation
        public SpriteFont font;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            rnd = new Random();
            


            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
#if !DEBUG
            graphics.IsFullScreen = true;
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new Camera(this, new Vector3(0, 0, 50),
                Vector3.Zero, Vector3.Up);
            Components.Add(camera);

            modelManager = new ModelManager(this);
            Components.Add(modelManager);

            Annotation.InitializeAnnotationSystem(GraphicsDevice);


            //new LineAnnotation(new Vector3(0, 0, 0), new Vector3(10, -10, 0));
            new CircleAnnotation(15, Vector3.Zero, Vector3.UnitZ);
            //new ColorAnnotation(Color.Green, Color.BlueViolet, new CircleAnnotation(15, Vector3.Zero, Vector3.UnitZ));
            //new ColorAnnotation(Color.Black, Color.Coral, new CircleAnnotation(15, Vector3.Zero, new Vector3(.3f, .5f, .2f)));
            //new ColorAnnotation(Color.Maroon, Color.Black, new TextAnnotation("This is my test text.", new Vector2(250, 250), Content));
            
            //new ScaleAnnotation(.5f, new CircleAnnotation(20, Vector3.Zero, new Vector3(.6f, .2f, .2f)));
            //new TranslationAnnotation(new Vector3(0, 10, 10), new LineAnnotation(Vector3.Zero, new Vector3(100, 100, 50)));
            //new LineAnnotation(Vector3.Zero, new Vector3(100, 100, 50));

            Vector3 cameraPosition = new Vector3(0, 0, -10);
            viewMatrix = Matrix.CreateTranslation(cameraPosition);
            projectionMatrix = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.Up);

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

            crosshairTexture = Content.Load<Texture2D>(@"textures\crosshair");
            font = Content.Load<SpriteFont>("textAnnotation");
            // Load souns and play initial sounds
            //trackCue.Play();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // See if the player has fired a shot
            FireShots(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            spriteBatch.Draw(crosshairTexture,
                new Vector2((Window.ClientBounds.Width / 2)
                - (crosshairTexture.Width / 2),
                (Window.ClientBounds.Height / 2)
                - (crosshairTexture.Height / 2)),
            Color.White);
            //spriteBatch.DrawString(font, mytext.Words, mytext.Position, mytext.Color);

            spriteBatch.End();

            Annotation.DrawAllAnnotations(camera.view, camera.projection);

            base.Draw(gameTime);
        }

        protected void FireShots(GameTime gameTime)
        {
            if (shotCountdown <= 0)
            {
                //Did player press spacebar or left mouse button?
                if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    // Add shot to the model manager
                    modelManager.AddShot(
                        camera.cameraPosition + new Vector3(0, -5, 0),
                        camera.GetCameraDirection * shotSpeed);


                    // Reset the shot countdown
                    shotCountdown = shotDelay;
                }
            }
            else
                shotCountdown -= gameTime.ElapsedGameTime.Milliseconds;
        }

    }
}

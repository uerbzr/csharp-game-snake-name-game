using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace Simple3DGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Dictionary<char, Model> _letterModels;
        private List<Vector3> _letterPositions;
        private Model _treeModel;
        private Vector3 _treePosition = new Vector3(10, 0, 0); // Position of the tree
        private Matrix _world = Matrix.CreateTranslation(Vector3.Zero);
        private Matrix _view;
        private Matrix _projection;

        private Vector3 _cameraPosition = new Vector3(0, 0, 10);
        private Vector3 _cameraTarget = Vector3.Zero;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                _graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);

            _letterPositions = new List<Vector3>
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(5, 0, 0),
                        new Vector3(-5, 0, 0),
                        new Vector3(0, 0, 5),
                        new Vector3(0, 0, -5)
                    };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _letterModels = new Dictionary<char, Model>();

            foreach (char letter in "YOURNAME")
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Attempting to load model for letter: {letter}");
                    _letterModels[letter] = Content.Load<Model>($"Letters/{letter}");
                    System.Diagnostics.Debug.WriteLine($"Model for letter {letter} loaded successfully.");
                }
                catch (ContentLoadException e)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading model for letter {letter}: {e.Message}");
                }
            }

            // Load the tree model
            try
            {
                _treeModel = Content.Load<Model>("Tree1/Tree1");
                System.Diagnostics.Debug.WriteLine("Tree model loaded successfully.");
            }
            catch (ContentLoadException e)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tree model: {e.Message}");
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.W))
                _cameraPosition.Z -= 0.1f;
            if (keyboardState.IsKeyDown(Keys.S))
                _cameraPosition.Z += 0.1f;
            if (keyboardState.IsKeyDown(Keys.A))
                _cameraPosition.X -= 0.1f;
            if (keyboardState.IsKeyDown(Keys.D))
                _cameraPosition.X += 0.1f;

            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.Up);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var letter in _letterModels.Keys)
            {
                var model = _letterModels[letter];
                var position = _letterPositions[_letterModels.Keys.ToList().IndexOf(letter)];

                if (model != null)
                {
                    foreach (var mesh in model.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.World = Matrix.CreateTranslation(position);
                            effect.View = _view;
                            effect.Projection = _projection;
                        }
                        mesh.Draw();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Model for letter {letter} is null, not drawing.");
                }
            }

            // Draw the tree model
            if (_treeModel != null)
            {
                foreach (var mesh in _treeModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = Matrix.CreateTranslation(_treePosition);
                        effect.View = _view;
                        effect.Projection = _projection;
                    }
                    mesh.Draw();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Tree model is null, not drawing.");
            }

            base.Draw(gameTime);
        }
    }
}

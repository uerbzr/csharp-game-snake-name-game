using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace workshop._2dNameGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private Vector2 _playerVelocity;
        private bool _isGrounded;

        private Texture2D _platformTexture;
        private List<Rectangle> _platforms;
        private List<Color> _platformColors; // List of colors for platforms

        private Dictionary<char, Texture2D> _letterTextures;
        private Dictionary<char, Vector2> _letterPositions;

        private const float Gravity = 0.5f;
        private const float JumpStrength = -10f;
        private const float MoveSpeed = 5f;

        private int _score = 0;
        private HashSet<char> _collectedLetters;
        private List<char> _collectedLettersList;

        // New variables for the moving platform
        private Rectangle _movingPlatform;
        private float _movingPlatformSpeed = 2f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set the game window size
            _graphics.PreferredBackBufferWidth = 1280; // Width of the window
            _graphics.PreferredBackBufferHeight = 720; // Height of the window
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(100, 100);
            _playerVelocity = Vector2.Zero;
            _isGrounded = false;

            _platforms = new List<Rectangle>
                        {
                            new Rectangle(50, 300, 200, 20),
                            new Rectangle(300, 400, 200, 20),
                            new Rectangle(550, 550, 200, 20),
                            new Rectangle(800, 600, 200, 20),
                            new Rectangle(1050, 700, 200, 20)
                        };

            _platformColors = new List<Color>
                        {
                            Color.Gray,
                            Color.Red,
                            Color.Green,
                            Color.Blue,
                            Color.Yellow
                        };

            _letterPositions = new Dictionary<char, Vector2>
                        {
                            { 'N', new Vector2(50 + 100, 300 - 20) },  // On the first platform
                            { 'I', new Vector2(300 + 100, 400 - 20) }, // On the second platform
                            { 'G', new Vector2(550 + 100, 550 - 20) }, // On the third platform
                            { 'E', new Vector2(400, 100) },
                            { 'L', new Vector2(500, 100) },
                            { 'A', new Vector2(600, 100) },
                            { 'B', new Vector2(700, 100) }
                        };

            _collectedLetters = new HashSet<char>();
            _collectedLettersList = new List<char>();

            // Initialize the moving platform
            _movingPlatform = new Rectangle(400, _graphics.PreferredBackBufferHeight, 200, 20);
            _platforms.Add(_movingPlatform);
            _platformColors.Add(Color.Purple);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _platformTexture = new Texture2D(GraphicsDevice, 1, 1);
            _platformTexture.SetData(new[] { Color.White });

            _letterTextures = new Dictionary<char, Texture2D>();
            foreach (char letter in "NIGEL")
            {
                _letterTextures[letter] = CreateTextureFromString(letter.ToString());
            }

            // Initialize player texture with the first letter
            _collectedLettersList.Add('N');
            UpdatePlayerTexture();
        }

        private Texture2D CreateTextureFromString(string text)
        {
            var font = Content.Load<SpriteFont>("DefaultFont");
            var size = font.MeasureString(text);
            var renderTarget = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y);
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, text, Vector2.Zero, Color.Black);
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            return renderTarget;
        }

        private void UpdatePlayerTexture()
        {
            string playerName = string.Join("", _collectedLettersList);
            _playerTexture = CreateTextureFromString(playerName);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            // Horizontal movement
            if (keyboardState.IsKeyDown(Keys.A))
                _playerPosition.X -= MoveSpeed;
            if (keyboardState.IsKeyDown(Keys.D))
                _playerPosition.X += MoveSpeed;

            // Jumping
            if (_isGrounded && keyboardState.IsKeyDown(Keys.Space))
            {
                _playerVelocity.Y = JumpStrength;
                _isGrounded = false;
            }

            // Apply gravity
            _playerVelocity.Y += Gravity;
            _playerPosition += _playerVelocity;

            // Collision detection
            _isGrounded = false;
            foreach (var platform in _platforms)
            {
                if (_playerPosition.Y + _playerTexture.Height >= platform.Top &&
                    _playerPosition.Y + _playerTexture.Height <= platform.Bottom &&
                    _playerPosition.X + _playerTexture.Width > platform.Left &&
                    _playerPosition.X < platform.Right)
                {
                    _playerPosition.Y = platform.Top - _playerTexture.Height;
                    _playerVelocity.Y = 0;
                    _isGrounded = true;
                }
            }

            // Collect letters
            foreach (var letter in _letterPositions.Keys.ToList())
            {
                if (Vector2.Distance(_playerPosition, _letterPositions[letter]) < 30.0f)
                {
                    if ("NIGEL".Contains(letter))
                    {
                        _score += 10;
                        _collectedLetters.Add(letter);
                        _collectedLettersList.Add(letter);
                        UpdatePlayerTexture();
                    }
                    else
                    {
                        _score -= 5;
                    }
                    _letterPositions.Remove(letter);
                }
            }

            // Update moving platform position
            _movingPlatform.Y -= (int)_movingPlatformSpeed;
            if (_movingPlatform.Y < -_movingPlatform.Height)
            {
                _movingPlatform.Y = _graphics.PreferredBackBufferHeight;
            }

            // Update the moving platform in the platforms list
            _platforms[_platforms.Count - 1] = _movingPlatform;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw player
            _spriteBatch.Draw(_playerTexture, _playerPosition, Color.White);

            // Draw platforms
            for (int i = 0; i < _platforms.Count; i++)
            {
                _spriteBatch.Draw(_platformTexture, _platforms[i], _platformColors[i]);
            }

            // Draw letters
            foreach (var letter in _letterTextures.Keys)
            {
                if (_letterPositions.ContainsKey(letter))
                {
                    _spriteBatch.Draw(_letterTextures[letter], _letterPositions[letter], Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

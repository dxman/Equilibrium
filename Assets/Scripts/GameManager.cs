using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Equilibrium
{
    public class GameManager : MonoBehaviour
    {
        public int MaxScore;
        public int MaxRounds;

        private Dictionary<int, GameObject> _players;
        private GameObject _pickUpsObject;
        private GameObject _uiObject;

        private Transform _myTransform;
        private Dictionary<int, Transform> _scoreMarkerTransforms;
        private Text _mainMessageText;
        private Text _subMessageText;
        private ParticleSystem _winParticles;

        private int _currentRound;
        private Dictionary<int, int> _playerScores;
        private Dictionary<int, int> _playerRoundsWon;
        private Dictionary<int, Vector3> _playerStartPositions;
        private Dictionary<int, Vector2> _playerVelocities;
        private GameState _gameState;

        private List<Dictionary<InputAction, string>> _inputMappings;

        public void AddScore(int playerId, int amount)
        {
            _playerScores[playerId] += amount;

            if (_playerScores[playerId] > MaxScore)
            {
                _playerScores[playerId] = MaxScore;
            }

            if (_playerScores[playerId] < -MaxScore)
            {
                _playerScores[playerId] = -MaxScore;
            }

            UpdateScoreBoard();

            if (_playerScores[playerId] == 0)
            {
                RoundComplete(playerId);
            }
        }

        private void GoToTitle()
        {
            _mainMessageText.text = "Equilibrium";
            _subMessageText.text = "Be the first player to reach exactly 0 points. The first player to 3 rounds wins!$$Press [START] to begin, or [BACK] to exit!";
            _subMessageText.text = _subMessageText.text.Replace('$', '\n');

            foreach (var player in _players)
            {
                player.Value.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            _pickUpsObject.SetActive(false);
            _uiObject.SetActive(true);
            _gameState = GameState.Title;
        }

        private void Play()
        {
            foreach (var player in _players)
            {
                var rb = player.Value.GetComponent<Rigidbody2D>();
                rb.velocity = _playerVelocities[player.Value.GetComponent<PlayerComponent>().Id];
            }

            _pickUpsObject.SetActive(true);
            _uiObject.SetActive(false);
            _gameState = GameState.Play;
        }

        private void Pause()
        {
            _mainMessageText.text = "PAUSED";
            _subMessageText.text = "Press [START] to resume or [BACK] to return to title!";

            foreach (var player in _players)
            {
                var rb = player.Value.GetComponent<Rigidbody2D>();
                _playerVelocities[player.Value.GetComponent<PlayerComponent>().Id] =
                    rb.velocity;
                rb.velocity = Vector2.zero;
            }

            _pickUpsObject.SetActive(false);
            _uiObject.SetActive(true);
            _gameState = GameState.Pause;
        }

        private void RoundComplete(int winningPlayerId)
        {
            if (winningPlayerId == 0)
            {
                _winParticles.startColor = new Color(1f, .5f, 0f);
            }
            else
            {
                _winParticles.startColor = new Color(0f, .5f, 1f);
            }
            _winParticles.Play();

            foreach (var player in _players)
            {
                var pc = player.Value.GetComponent<PlayerComponent>();

                player.Value.transform.position = _playerStartPositions[pc.Id];
                if (pc.Id == 0)
                {
                    pc.XInput = 1;
                    pc.YInput = 0;
                }
                else
                {
                    pc.XInput = -1;
                    pc.YInput = 0;
                }

                var rb = player.Value.GetComponent<Rigidbody2D>();
                _playerVelocities[pc.Id] = Vector2.zero;
                rb.velocity = Vector2.zero;
            }

            _pickUpsObject.SetActive(false);
            _uiObject.SetActive(true);
            _currentRound++;
            _playerRoundsWon[winningPlayerId]++;
            if (_playerRoundsWon[winningPlayerId] >= MaxRounds)
            {
                GameOver(winningPlayerId);
                return;
            }

            _gameState = GameState.RoundOver;
            var message = $"Round {_currentRound + 1} winner: Player {winningPlayerId + 1}!$Score: {_playerRoundsWon[0]}-{_playerRoundsWon[1]}";
            message = message.Replace('$', '\n');
            _mainMessageText.text = message;
            _subMessageText.text = "Press [START] to begin the next round!";
        }

        private void GameOver(int winningPlayerId)
        {
            _gameState = GameState.GameOver;
            _currentRound = 0;
            var message = $"Player {winningPlayerId + 1} wins!$Final Score: {_playerRoundsWon[0]}-{_playerRoundsWon[1]}";
            message = message.Replace('$', '\n');
            _mainMessageText.text = message;
            _subMessageText.text = "Press [START] to return to title!";
        }

        private void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public Dictionary<InputAction, string> GetInputMappings(int playerId)
        {
            return _inputMappings[playerId];
        }

        private void UpdateScoreBoard()
        {
            foreach (var marker in _scoreMarkerTransforms)
            {
                var position = marker.Value.position;
                position.x = _playerScores[marker.Key] * 10;
                marker.Value.position = position;
            }
        }

        private void Awake()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            _players = new Dictionary<int, GameObject>();
            _playerVelocities = new Dictionary<int, Vector2>();
            _playerStartPositions = new Dictionary<int, Vector3>();
            _playerRoundsWon = new Dictionary<int, int>();
            foreach (var player in players)
            {
                var pc = player.GetComponent<PlayerComponent>();
                _players[pc.Id] = player;
                _playerVelocities[pc.Id] = Vector2.zero;
                _playerStartPositions[pc.Id] = player.GetComponent<Transform>().position;
                _playerRoundsWon[pc.Id] = 0;
            }

            _pickUpsObject = GameObject.Find("PickUps");
            _uiObject = GameObject.Find("UI");

            _scoreMarkerTransforms = new Dictionary<int, Transform>();
            var markers = FindObjectsOfType(typeof(ScoreMarkerComponent));
            foreach (var marker in markers)
            {
                var m = (ScoreMarkerComponent)marker;
                _scoreMarkerTransforms[m.PlayerId] = m.transform;
            }

            var mainMessage = _uiObject.transform.Find("MainMessage");
            _mainMessageText = mainMessage.GetComponent<Text>();

            var subMessage = _uiObject.transform.Find("SubMessage");
            _subMessageText = subMessage.GetComponent<Text>();

            var winParticles = GameObject.Find("WinParticles");
            _winParticles = winParticles.GetComponent<ParticleSystem>();

            _playerScores = new Dictionary<int, int>
            {
                {0, -MaxScore},
                {1, MaxScore}
            };

            _pickUpsObject.GetComponent<PickUpManager>().SeedPickups();

            _inputMappings = new List<Dictionary<InputAction, string>>
            {
                new Dictionary<InputAction, string>
                {
                    {InputAction.HorizontalAxis, "Horizontal1"},
                    {InputAction.VerticalAxis, "Vertical1"},
                    {InputAction.Charge, "Thrust1"},
                    {InputAction.Brake, "Brake1"}
                },
                new Dictionary<InputAction, string>
                {
                    {InputAction.HorizontalAxis, "Horizontal2"},
                    {InputAction.VerticalAxis, "Vertical2"},
                    {InputAction.Charge, "Thrust2"},
                    {InputAction.Brake, "Brake2"}
                }
            };

            UpdateScoreBoard();
            GoToTitle();
        }

        private void Update()
        {
            if (_gameState == GameState.Play)
            {
                foreach (var p in _players)
                {
                    var pc = p.Value.GetComponent<PlayerComponent>();
                    pc.XInput = Input.GetAxis(_inputMappings[pc.Id][InputAction.HorizontalAxis]);
                    pc.YInput = Input.GetAxis(_inputMappings[pc.Id][InputAction.VerticalAxis]);
                    pc.IsChargeHeld = Input.GetButton(_inputMappings[pc.Id][InputAction.Charge]);

                    if (Input.GetButtonDown(_inputMappings[pc.Id][InputAction.Brake]))
                    {
                        pc.BeginBraking();
                    }
                    else if (Input.GetButtonUp(_inputMappings[pc.Id][InputAction.Brake]))
                    {
                        pc.EndBraking();
                    }
                }
            }

            if (Input.GetButtonDown("Submit"))
            {
                switch (_gameState)
                {
                    case GameState.Title:
                    case GameState.Pause:
                        Play();
                        break;

                    case GameState.RoundOver:
                        _pickUpsObject.GetComponent<PickUpManager>().SeedPickups();
                        _playerScores[0] = -MaxScore;
                        _playerScores[1] = MaxScore;
                        UpdateScoreBoard();
                        Play();
                        break;

                    case GameState.Play:
                        Pause();
                        break;
                    
                    case GameState.GameOver:
                        Restart();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (Input.GetButtonDown("Back"))
            {
                switch (_gameState)
                {
                    case GameState.Title:
                        Application.Quit();
                        break;

                    case GameState.Pause:
                        Restart();
                        break;

                    case GameState.Play:
                    case GameState.RoundOver:
                    case GameState.GameOver:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}

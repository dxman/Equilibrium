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

        private GameObject _playersObject;
        private GameObject _pickUpsObject;
        private GameObject _scoreBoardObject;
        private GameObject _uiObject;

        private Transform _myTransform;
        private Dictionary<int, Transform> _scoreMarkerTransforms;
        private Text _mainMessageText;
        private Text _subMessageText;

        private Dictionary<int, int> _playerScores;
        private GameState _gameState;

        private List<Dictionary<InputAction, KeyCode>> _inputMappings;

        public void AddScore(int playerId, int amount)
        {
            _playerScores[playerId] += amount;

            if (_playerScores[playerId] == 0)
            {
                GameOver(playerId);
                return;
            }

            if (_playerScores[playerId] > MaxScore)
            {
                _playerScores[playerId] = MaxScore;
            }

            if (_playerScores[playerId] < -MaxScore)
            {
                _playerScores[playerId] = -MaxScore;
            }

            UpdateScoreBoard();
        }

        private void GoToTitle()
        {
            _mainMessageText.text = "Equilibrium";
            _subMessageText.text = "Press <SPACE> to begin, or <ESC> to exit!";

            //_playersObject.SetActive(false);
            _scoreBoardObject.SetActive(false);
            _pickUpsObject.SetActive(false);
            _uiObject.SetActive(true);
            Time.timeScale = 0f;
            _gameState = GameState.Title;
        }

        private void Play()
        {
            //_playersObject.SetActive(true);
            _scoreBoardObject.SetActive(true);
            _pickUpsObject.SetActive(true);
            _uiObject.SetActive(false);
            Time.timeScale = 1f;
            _gameState = GameState.Play;
        }

        private void Pause()
        {
            _mainMessageText.text = "PAUSED";
            _subMessageText.text = "Press <SPACE> to resume or <ESC> to return to title!";

            //_playersObject.SetActive(false);
            _scoreBoardObject.SetActive(false);
            _pickUpsObject.SetActive(false);
            _uiObject.SetActive(true);
            Time.timeScale = 0f;
            _gameState = GameState.Pause;
        }

        private void GameOver(int winningPlayerId)
        {
            _mainMessageText.text = $"Player {winningPlayerId + 1} wins!";
            _subMessageText.text = "Press <SPACE> to return to title!";

            //_playersObject.SetActive(false);
            _scoreBoardObject.SetActive(false);
            _pickUpsObject.SetActive(false);
            _uiObject.SetActive(true);
            Time.timeScale = 0f;
            _gameState = GameState.GameOver;
        }

        private void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public Dictionary<InputAction, KeyCode> GetInputMappings(int playerId)
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
            _playersObject = GameObject.Find("Players");
            _scoreBoardObject = GameObject.Find("ScoreBoard");
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

            _playerScores = new Dictionary<int, int>
            {
                {0, -MaxScore},
                {1, MaxScore}
            };

            _inputMappings = new List<Dictionary<InputAction, KeyCode>>
            {
                new Dictionary<InputAction, KeyCode>
                {
                    {InputAction.RotateLeft, KeyCode.A},
                    {InputAction.RotateRight, KeyCode.D},
                    {InputAction.Charge, KeyCode.W},
                    {InputAction.Brake, KeyCode.S}
                },
                new Dictionary<InputAction, KeyCode>
                {
                    {InputAction.RotateLeft, KeyCode.LeftArrow},
                    {InputAction.RotateRight, KeyCode.RightArrow},
                    {InputAction.Charge, KeyCode.UpArrow},
                    {InputAction.Brake, KeyCode.DownArrow}
                }
            };

            UpdateScoreBoard();
            GoToTitle();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switch (_gameState)
                {
                    case GameState.Title:
                    case GameState.Pause:
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
            else if (Input.GetKeyDown(KeyCode.Escape))
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
                    case GameState.GameOver:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}

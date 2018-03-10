using System.Linq;
using Equilibrium.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace Equilibrium.Ui
{
    public class UiManager : MonoBehaviour, IMessageSubscriber<GameStateChangeMessage>
    {
        private GameManager _gameManager;

        private Transform _myTransform;
        private Text _mainMessageText;
        private Text _subMessageText;

        public void Handle(GameStateChangeMessage message)
        {
            WinningGameStateChangeMessage winMessage;
            string mainText;
            switch (message.NewState)
            {
                case GameState.Title:
                    _mainMessageText.text = "Equilibrium";
                    _subMessageText.text = "Be the first player to reach exactly 0 points. The first player to 3 rounds wins!$$Press [START] to begin, or [BACK] to exit!";
                    _subMessageText.text = _subMessageText.text.Replace('$', '\n');
                    SetActive(true);
                    break;

                case GameState.RoundOver:
                    winMessage = message as WinningGameStateChangeMessage;
                    if (winMessage == null) break;

                    mainText =
                        $"Round {_gameManager.CurrentRound + 1} winner: Player {winMessage.WinningPlayerId}!$Score: {GetRoundScore()}";
                    _mainMessageText.text = mainText.Replace('$', '\n');
                    _subMessageText.text = "Press [START] to begin the next round!";
                    SetActive(true);
                    break;

                case GameState.GameOver:
                    winMessage = message as WinningGameStateChangeMessage;
                    if (winMessage == null) break;

                    mainText =
                        $"Player {winMessage.WinningPlayerId} wins!$Final Score: {GetRoundScore()}";
                    _mainMessageText.text = mainText.Replace('$', '\n');
                    _subMessageText.text = "Press [START] to return to title!";
                    SetActive(true);
                    break;

                case GameState.Pause:
                    _mainMessageText.text = "PAUSED";
                    _subMessageText.text = "Press [START] to resume or [BACK] to return to title!";
                    SetActive(true);
                    break;

                case GameState.Play:
                    SetActive(false);
                    break;

            }
        }

        private void SetActive(bool isActive)
        {
            _myTransform.gameObject.SetActive(isActive);
        }

        private string GetRoundScore()
        {
            return string.Join("-", _gameManager.Players.PlayerIds.Select(id => _gameManager.GetPlayerRounds(id)));
        }

        private void Awake()
        {
            _myTransform = GetComponent<Transform>();

            var mainMessage = _myTransform.Find("MainMessage");
            _mainMessageText = mainMessage.GetComponent<Text>();

            var subMessage = _myTransform.Find("SubMessage");
            _subMessageText = subMessage.GetComponent<Text>();
        }

        private void Start()
        {
            var gameManagerObject = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gameManagerObject.GetComponent<GameManager>();

            _gameManager.Subscribe(this);
        }
    }
}

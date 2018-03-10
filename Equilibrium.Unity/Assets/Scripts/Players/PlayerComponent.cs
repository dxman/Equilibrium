using System;
using System.Collections.Generic;
using UnityEngine;

namespace Equilibrium.Players
{
    public class PlayerComponent : MonoBehaviour
    {
        public int Id;
        public int StartPoints;
        public Color Color;
        public float ThrustStrength;
        public float RotateSpeed;
        public float ChargeSpeed;

        private GameManager _gameManager;
        private Transform _myTransform;
        private Rigidbody2D _myRigidbody2D;
        private Transform _mySpriteTransform;
        private Transform _myChargeBarTransform;

        private Dictionary<InputAction, string> _inputMappings;
        
        private float _chargePercent;
        private bool _isCharging;

        public Vector3 Facing { get; private set; }

        public Vector3 Position
        {
            get { return _myTransform.position; }

            set { _myTransform.position = value; }
        }

        public Quaternion Rotation
        {
            get { return _mySpriteTransform.rotation; }

            set
            {
                _mySpriteTransform.rotation = value;
                UpdateFacing();
            }
        }

        public Vector2 Velocity
        {
            get { return _myRigidbody2D.velocity; }

            set { _myRigidbody2D.velocity = value; }
        }

        public void SetActive(bool isActive)
        {
            _myTransform.gameObject.SetActive(isActive);
        }

        public void ResetCharge()
        {
            _isCharging = false;
            UpdateCharging();
        }

        private void BeginCharging()
        {
            _isCharging = true;
        }

        private void Thrust()
        {
            _myRigidbody2D.AddForce(Facing * _chargePercent * ThrustStrength, ForceMode2D.Impulse);
            _gameManager.Sound.PlaySound(SoundType.Thrust);
            ResetCharge();
        }

        private void UpdateCharging()
        {
            if (_isCharging)
            {
                _chargePercent += ChargeSpeed * Time.unscaledDeltaTime;
                if (_chargePercent > 1f)
                {
                    _chargePercent = 1f;
                }
            }
            else
            {
                _chargePercent = 0f;
            }

            var chargeBarLocalScale = _myChargeBarTransform.localScale;
            chargeBarLocalScale.x = 10f * _chargePercent;
            _myChargeBarTransform.localScale = chargeBarLocalScale;
        }

        private void UpdateFacing()
        {
            Facing = _mySpriteTransform.rotation * Vector3.up;
            if (Facing == Vector3.zero)
            {
                Facing = Vector3.up;
            }
        }

        public void BeginBraking()
        {
            _myRigidbody2D.drag = 8f;
        }

        public void EndBraking()
        {
            _myRigidbody2D.drag = .5f;
        }

        private void Awake()
        {
            var gameManagerObject = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gameManagerObject.GetComponent<GameManager>();

            _myTransform = GetComponent<Transform>();
            _myRigidbody2D = GetComponent<Rigidbody2D>();

            _mySpriteTransform = _myTransform.Find("Sprite");
            if (_mySpriteTransform == null)
            {
                Debug.LogError($"Initialization error: Player {Id} does not have a child named Sprite.");
                return;
            }

            _myChargeBarTransform = _mySpriteTransform.Find("ChargeBar");
            if (_myChargeBarTransform == null)
            {
                Debug.LogError($"Initialization error: Player {Id} does not have a child named ChargeBar.");
                return;
            }

            _inputMappings = _gameManager.GetInputMappings(Id);
        }

        private void Update()
        {
            //if (_gameManager.IsInMenu) return;

            var xInput = Input.GetAxis(_inputMappings[InputAction.HorizontalAxis]);
            var yInput = Input.GetAxis(_inputMappings[InputAction.VerticalAxis]);
            var isChargeHeld = Input.GetButton(_inputMappings[InputAction.Charge]);

            if (Input.GetButtonDown(_inputMappings[InputAction.Brake]))
            {
                BeginBraking();
            }
            else if (Input.GetButtonUp(_inputMappings[InputAction.Brake]))
            {
                EndBraking();
            }

            if (Math.Abs(xInput) > 0.1f || Math.Abs(yInput) > 0.1f)
            {
                var heading = Mathf.Atan2(
                    -xInput,
                    -yInput
                );

                var targetRotation = Quaternion.Euler(0f, 0f, heading * Mathf.Rad2Deg);
                Rotation = Quaternion.RotateTowards(Rotation, targetRotation, RotateSpeed * Time.deltaTime);
            }

            if (isChargeHeld)
            {
                if (!_isCharging)
                {
                    BeginCharging();
                }
            }
            else if (_isCharging)
            {
                Thrust();
            }

            UpdateCharging();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _gameManager.Sound.PlaySound(collision.gameObject.CompareTag("Player")
                ? SoundType.CollidePlayer
                : SoundType.CollideWall);
        }
    }
}

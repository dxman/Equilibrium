using System.Collections.Generic;
using UnityEngine;

namespace Equilibrium
{
    public class PlayerComponent : MonoBehaviour
    {
        public int Id;
        public float ThrustStrength;
        public float RotateSpeed;
        public float ChargeSpeed;

        private GameManager _gameManager;
        private SoundManager _soundManager;
        private Transform _myTransform;
        private Rigidbody2D _myRigidbody2D;
        private Transform _mySpriteTransform;
        private Transform _myChargeBarTransform;

        private Dictionary<InputAction, KeyCode> _inputMappings;
        private Vector3 _facing;
        private float _chargePercent;
        private bool _isCharging;
        private bool _isBraking;

        private void RotateLeft()
        {
            _mySpriteTransform.Rotate(Vector3.forward * RotateSpeed * Time.deltaTime);
            UpdateFacing();
        }

        private void RotateRight()
        {
            _mySpriteTransform.Rotate(Vector3.forward * -RotateSpeed * Time.deltaTime);
            UpdateFacing();
        }

        private void Idle()
        {
            _chargePercent = 0f;
            _isCharging = false;
        }

        private void BeginCharging()
        {
            _isCharging = true;
        }

        private void Thrust()
        {
            _myRigidbody2D.AddForce(_facing * _chargePercent * ThrustStrength, ForceMode2D.Impulse);
            _soundManager.PlaySound(SoundType.Thrust);
            Idle();
        }

        private void UpdateCharging()
        {
            if (_isCharging)
            {
                _chargePercent += ChargeSpeed * Time.deltaTime;
                if (_chargePercent > 1f)
                {
                    _chargePercent = 1f;
                }
            }

            var chargeBarLocalScale = _myChargeBarTransform.localScale;
            chargeBarLocalScale.x = 10f * _chargePercent;
            _myChargeBarTransform.localScale = chargeBarLocalScale;
        }

        private void BeginBraking()
        {
            _isBraking = true;
            _myRigidbody2D.drag = 8f;
        }

        private void EndBraking()
        {
            _isBraking = false;
            _myRigidbody2D.drag = .5f;
        }

        private void UpdateFacing()
        {
            _facing = _mySpriteTransform.rotation * Vector3.up;
            if (_facing == Vector3.zero)
            {
                _facing = Vector3.up;
            }
        }

        private void Awake()
        {
            var gm = GameObject.FindGameObjectWithTag("GameManager");
            _gameManager = gm.GetComponent<GameManager>();
            _soundManager = gm.GetComponent<SoundManager>();

            _myTransform = GetComponent<Transform>();
            _myRigidbody2D = GetComponent<Rigidbody2D>();

            _mySpriteTransform = _myTransform.Find("Sprite");
            if (_mySpriteTransform == null)
            {
                Debug.LogError($"Initialization error: Player {Id} does not have a child named Sprite.");
                return;
            }

            _myChargeBarTransform = _myTransform.Find("ChargeBar");
            if (_myChargeBarTransform == null)
            {
                Debug.LogError($"Initialization error: Player {Id} does not have a child named ChargeBar.");
                return;
            }

            _inputMappings = _gameManager.GetInputMappings(Id);

            _isCharging = false;
            _isBraking = false;
            UpdateFacing();
        }

        private void Update()
        {
            if (Input.GetKey(_inputMappings[InputAction.RotateLeft]))
            {
                RotateLeft();
            }

            if (Input.GetKey(_inputMappings[InputAction.RotateRight]))
            {
                RotateRight();
            }

            if (Input.GetKey(_inputMappings[InputAction.Charge]))
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

            if (Input.GetKeyDown(_inputMappings[InputAction.Brake]))
            {
                BeginBraking();
            }
            else if (Input.GetKeyUp(_inputMappings[InputAction.Brake]))
            {
                EndBraking();
            }

            UpdateCharging();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _soundManager.PlaySound(SoundType.CollidePlayer);
            }
            else
            {
                _soundManager.PlaySound(SoundType.CollideWall);
            }
        }
    }
}

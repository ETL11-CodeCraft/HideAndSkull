using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace HideAndSkull.Character
{
    public enum PlayMode
    {
        None,
        AI,
        Player,
    }

    public enum ActFlag
    {
        None,
        Idle,
        Move,
        Die
    }

    public class Skull : MonoBehaviour
    {
        public PlayMode PlayMode { get; set; }
        private float Speed => _isRunning ? RUN_SPEED : WALK_SPEED;  //프레임당 이동거리


        //상수
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int IsWalk = Animator.StringToHash("IsWalk");
        private const float IDLE_DURATION = 3f;
        private const float MOVE_DURATION = 5f;
        private const float STOP_AFTER_MOVE = 1f;
        private const float DURATION_NOISE_RANGE = 1f;
        private const float WALK_SPEED = 3f;
        private const float RUN_SPEED = 5f;
        private const float ROTATE_SPEED = 60f;
        private readonly Vector3 _cameraOffset = new Vector3(0, 2.5f, -3.5f);
        private readonly Quaternion _cameraRotation = new Quaternion(0.075f, 0, 0, 1f);

        //AI, Player 공통
        private bool _isRunning;
        private bool _isDead;
        private ActFlag _currentAct;
        private Animator _animator;
        private Renderer[] _skinnedMeshRenderers = new Renderer[4];

        //AI
        private float _idleElapsed = 0f;
        private float _moveElapsed = 0f;
        private float _durationNoise = 0f;
        private Vector3 _moveDirection;
        private Coroutine _aiActCoroutine;

        //Player
        [SerializeField] private BoxCollider _swordCollider;
        private CapsuleCollider _characterCollider;
        private Transform _cameraAttachTransform;
        private bool _canAction = true;
        private Vector3 _movement;
        //DEBUG
        private PlayerInputActions _inputActions;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _swordCollider.enabled = false;
            _skinnedMeshRenderers = GetComponentsInChildren<Renderer>();
            _characterCollider = GetComponent<CapsuleCollider>();
        }

        private void Update()
        {
            if (PlayMode == PlayMode.AI)
            {
                switch (_currentAct)
                {
                    case ActFlag.None:
                        _currentAct = (ActFlag)Random.Range(1, 3);
                        _durationNoise = Random.Range(-DURATION_NOISE_RANGE, DURATION_NOISE_RANGE);
                        break;
                    case ActFlag.Idle:
                        Idle();
                        break;
                    case ActFlag.Move:
                        Move();
                        break;
                    case ActFlag.Die:
                        //죽었을 때는 아무것도 하지 않기
                        break;
                }
            }
            if(PlayMode == PlayMode.Player)
            {
                if(_canAction)
                {
                    //PC 바인딩 실행
                    if(_movement.y > 0)
                    {
                        UpPerform();
                        _animator.SetBool(IsWalk, true);
                    }
                    else
                    {
                        _animator.SetBool(IsWalk, false);
                    }
                    if(_movement.x > 0)
                    {
                        RightPerform();
                    }
                    if(_movement.x < 0)
                    {
                        LeftPerform();
                    }
                }
            }
        }

        public void Die()
        {
            if(!_isDead)
            {
                _canAction = false;
                _isDead = true;
                _currentAct = ActFlag.Die;

                _animator.SetTrigger(IsDead);
            }
        }

        public void OnEndDieAnimation()
        {
            _canAction = true;

            switch (PlayMode)
            {
                case PlayMode.AI:
                    Destroy(gameObject);
                    break;
                case PlayMode.Player:
                    foreach (Renderer meshRenderer in _skinnedMeshRenderers)
                    {
                        meshRenderer.enabled = false;
                        _characterCollider.enabled = false;
                    }
                    break;
            }
        }

        #region Player
        public void StartPlayerAct()
        {
            SetPlayerCamera();

            //TEST
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            //PC용 BINDING
            _inputActions.Player.Move.performed += PressMoveButton;
            _inputActions.Player.Move.canceled += PressMoveButton;
            _inputActions.Player.Sprint.performed += PressRunButton;
            _inputActions.Player.Attack.performed += PressAttackButton;
        }

        private void SetPlayerCamera()
        {
            if (!Camera.main) return;

            _cameraAttachTransform = transform.Find("CameraAttach");
            if (!_cameraAttachTransform)
            {
                _cameraAttachTransform = new GameObject("CameraAttach").transform;
                _cameraAttachTransform.SetParent(transform);
                _cameraAttachTransform.localPosition = Vector3.zero;
            }
            
            Camera.main.transform.SetParent(_cameraAttachTransform);
            Camera.main.transform.localPosition = _cameraOffset;
            Camera.main.transform.localRotation = _cameraRotation;
        }

        public void RightPerform()
        {
            _cameraAttachTransform.Rotate(Vector3.up * (ROTATE_SPEED * Time.deltaTime));
        }

        public void LeftPerform()
        {
            _cameraAttachTransform.Rotate(Vector3.down * (ROTATE_SPEED * Time.deltaTime));
        }

        public void UpPerform()
        {
            if (_cameraAttachTransform.localRotation != Quaternion.identity)
            {
                transform.forward = _cameraAttachTransform.forward;
                _cameraAttachTransform.localRotation = Quaternion.identity;
            }
            
            transform.position += transform.forward * (Speed * Time.deltaTime);
        }

        public void RunPerform()
        {
            if (!_canAction) return;

            _isRunning = !_isRunning;
        }

        public void AttackPerform()
        {
            if(!_canAction) return;

            _swordCollider.enabled = true;
            _canAction = false;

            _animator.SetTrigger(IsAttacking);
        }

        public void OnEndAttackAnimation()
        {
            _swordCollider.enabled = false;
            _canAction = true;
        }

        public void PressMoveButton(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
        }

        public void PressRunButton(InputAction.CallbackContext context)
        {
            RunPerform();
        }

        public void PressAttackButton(InputAction.CallbackContext context)
        {
            AttackPerform();
        }
        #endregion

        #region AI
        private void Idle()
        {
            if (_idleElapsed > IDLE_DURATION + _durationNoise)
            {
                _currentAct = ActFlag.None;
                _idleElapsed = 0f;
            }
            else
            {
                _idleElapsed += Time.deltaTime;
            }
        }

        private void Move()
        {
            if (_moveElapsed > MOVE_DURATION + _durationNoise)
            {
                _currentAct = ActFlag.None;
                _moveDirection = Vector3.zero;
                _moveElapsed = 0f;
                _animator.SetBool(IsWalk, false);
            }
            else
            {
                if(_moveDirection == Vector3.zero)
                {
                    Vector2 tempDirection = Random.insideUnitCircle.normalized;
                    _moveDirection = new Vector3(tempDirection.x, 0, tempDirection.y);
                    transform.forward = new Vector3(_moveDirection.x, 0, _moveDirection.z);
                }
                
                transform.Translate(Vector3.forward * (Speed * Time.deltaTime));
                _animator.SetBool(IsWalk, true);
                _moveElapsed += Time.deltaTime;
            }
        }
        #endregion
    }
}
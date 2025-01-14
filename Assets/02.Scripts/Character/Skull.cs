using Photon.Pun;
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
    }

    [RequireComponent(typeof(PhotonTransformView))]
    public class Skull : MonoBehaviour
    {
        public PlayMode PlayMode { get; set; }
        private float Speed => _isRunning ? RUN_SPEED : WALK_SPEED;  //프레임당 이동거리


        //상수
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
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
        [SerializeField] private BoxCollider _boxCollider;
        private Transform _cameraAttachTransform;
        private bool _canAction = true;
        private Vector3 _movement;
        private PhotonView _photonView;
        //DEBUG
        PlayerInputActions inputActions;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _boxCollider.enabled = false;
            _skinnedMeshRenderers = GetComponentsInChildren<Renderer>();
            _photonView = GetComponent<PhotonView>();
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
                }
            }
            if(PlayMode == PlayMode.Player && _photonView.IsMine)
            {
                if(_canAction)
                {
                    //PC 바인딩 실행
                    if(_movement.y > 0)
                    {
                        UpPerform();
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
                    foreach (Renderer renderer in _skinnedMeshRenderers)
                    {
                        renderer.enabled = false;
                    }
                    break;
            }
        }

        #region Player
        public void StartPlayerAct()
        {
            if (!_photonView.IsMine) return;

            SetPlayerCamera();

            //TEST
            inputActions = new PlayerInputActions();
            inputActions.Enable();
            //PC용 BINDING
            inputActions.Player.Move.performed += PressMoveButton;
            inputActions.Player.Move.canceled += PressMoveButton;
            inputActions.Player.Sprint.performed += PressRunButton;
            inputActions.Player.Attack.performed += PressAttackButton;
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
            _cameraAttachTransform.Rotate(Vector3.up * ROTATE_SPEED * Time.deltaTime);
        }

        public void LeftPerform()
        {
            _cameraAttachTransform.Rotate(Vector3.down * ROTATE_SPEED * Time.deltaTime);
        }

        public void UpPerform()
        {
            if (_cameraAttachTransform.localRotation != Quaternion.identity)
            {
                transform.forward = _cameraAttachTransform.forward;
                _cameraAttachTransform.localRotation = Quaternion.identity;
            }
            
            transform.position += transform.forward * Speed * Time.deltaTime;
        }

        public void RunPerform()
        {
            if (!_canAction) return;

            _photonView.RPC(nameof(RunPerform_RPC), RpcTarget.AllViaServer);
        }

        [PunRPC]
        public void RunPerform_RPC()
        {
            _isRunning = !_isRunning;
        }

        public void AttackPerform()
        {
            if(!_canAction) return;

            _photonView.RPC(nameof(AttackPerform_RPC), RpcTarget.AllViaServer);
        }

        [PunRPC]
        public void AttackPerform_RPC()
        {
            _boxCollider.enabled = true;
            _canAction = false;

            _animator.SetTrigger(IsAttacking);
        }

        public void OnEndAttackAnimation()
        {
            _boxCollider.enabled = false;
            _canAction = true;
        }

        public void PressMoveButton(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
        }

        public void CancelMoveButton(InputAction.CallbackContext context)
        {
            _movement = Vector3.zero;
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
                _moveElapsed = 0f;
            }
            else
            {
                if(_moveDirection == Vector3.zero)
                {
                    Vector2 tempDirection = Random.insideUnitCircle.normalized * (Speed * Time.deltaTime);
                    _moveDirection = new Vector3(tempDirection.x, 0, tempDirection.y);
                }
                
                transform.Translate(_moveDirection);
                _moveElapsed += Time.deltaTime;
            }
        }
        #endregion
    }
}
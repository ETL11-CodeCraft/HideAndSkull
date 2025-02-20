﻿using ExitGames.Client.Photon;
using HideAndSkull.Lobby.UI;
using HideAndSkull.Settings.Sound;
using HideAndSkull.Settings.UI;
using HideAndSkull.Survivors.UI;
using Photon.Pun;
using Photon.Realtime;
﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    [RequireComponent(typeof(PhotonTransformView))]
    public class Skull : MonoBehaviour, IPunOwnershipCallbacks, IPunObservable, IOnEventCallback
    {
        public PlayMode PlayMode { get; set; }
        private float Speed => _isRunning ? RUN_SPEED : WALK_SPEED;  //프레임당 이동거리
        public PhotonView PhotonView { get; private set; }


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
        private bool _isMoving;
        public bool isDead;
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
        private float _rotateSpeed = ROTATE_SPEED;
        public Hashtable PlayerCustomProperty = new Hashtable { { "IsDead", false }, { "KillCount", 0 } };
        //PlayerInput
        private PlayerInputActions _inputActions;
        private GraphicRaycaster _graphicRaycaster;
        private List<RaycastResult> _results = new List<RaycastResult>(2);
        private PointerEventData _pointerEventData;
        private bool _isTouching;
        private bool _isTouchFlagDirty;
        private Vector2 _touchPosition;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _swordCollider.enabled = false;
            _swordCollider.GetComponent<Sword>().SwordOwner = this;
            _skinnedMeshRenderers = GetComponentsInChildren<Renderer>();
            _characterCollider = GetComponent<CapsuleCollider>();
            _graphicRaycaster = GameObject.Find("Canvas - Buttons").GetComponent<GraphicRaycaster>();
            _pointerEventData = new PointerEventData(null);
            PhotonView = GetComponent<PhotonView>();
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Update()
        {
            if (PlayMode == PlayMode.AI && PhotonNetwork.IsMasterClient)
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
            if (PlayMode == PlayMode.Player && PhotonView.IsMine)
            {
                //카메라 감도 설정
                _rotateSpeed = PlayerPrefs.GetFloat(SettingsParameter.CAMERA_SENSIBILITY);


                if (_canAction)
                {
                    if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        //PC 바인딩 실행
                        if (_movement.y > 0)
                        {
                            UpPerform();
                            _isMoving = true;
                        }
                        else
                        {
                            _isMoving = false;
                        }
                        if (_movement.x > 0)
                        {
                            RightPerform();
                        }
                        if (_movement.x < 0)
                        {
                            LeftPerform();
                        }
                    }
                    else if(Application.platform == RuntimePlatform.Android)
                    {
                        //Mobile 바인딩 실행
                        if (_isTouching)
                        {
                            _pointerEventData.position = _touchPosition;
                            _results.Clear();
                            _graphicRaycaster.Raycast(_pointerEventData, _results);

                            if (_results.Count > 0)
                            {
                                switch (_results[0].gameObject.name)
                                {
                                    case "Right":
                                        RightPerform();
                                        break;
                                    case "Left":
                                        LeftPerform();
                                        break;
                                    case "Up":
                                        UpPerform();
                                        _isMoving = true;
                                        break;
                                    case "Run":
                                        if (!_isTouchFlagDirty)
                                        {
                                            _isTouchFlagDirty = true;
                                            RunPerform();
                                        }
                                        break;
                                    case "Attack":
                                        if (!_isTouchFlagDirty)
                                        {
                                            _isTouchFlagDirty = true;
                                            AttackPerform();
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            _animator.SetBool(IsWalk, _isMoving);
        }

        [PunRPC]
        public void Die()
        {
            if(!isDead)
            {
                _canAction = false;
                isDead = true;
                _currentAct = ActFlag.Die;
                _animator.SetTrigger(IsDead);
                SoundManager.instance.PlaySFX("Die", transform.position);

                this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        public void OnEndDieAnimation()
        {
            _canAction = true;

            switch (PlayMode)
            {
                case PlayMode.AI:
                    if(PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
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
        public void InitPlayer()
        {
            if (!PhotonView.IsMine) return;

            SetPlayerCamera();
            PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerCustomProperty);

            //TEST
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            SceneManager.sceneUnloaded += (currentScene) =>
            {
                _inputActions.Disable();
            };
            //PC용 Binding
            _inputActions.Player.Move.performed += PressMoveButton;
            _inputActions.Player.Move.canceled += PressMoveButton;
            _inputActions.Player.Sprint.performed += PressRunButton;
            _inputActions.Player.Jump.performed += PressAttackButton;   //Test용 공격 space
            //Mobile용 Binding
            _inputActions.UI.Point.performed += OnTouchScreen;
            _inputActions.UI.Click.canceled += OnReleaseScreen;

            //감도 초기화
            PlayerPrefs.SetFloat(SettingsParameter.CAMERA_SENSIBILITY, ROTATE_SPEED);
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

        private void RightPerform()
        {
            _cameraAttachTransform.Rotate(Vector3.up * (_rotateSpeed * Time.deltaTime));
        }

        private void LeftPerform()
        {
            _cameraAttachTransform.Rotate(Vector3.down * (_rotateSpeed * Time.deltaTime));
        }

        private void UpPerform()
        {
            if (_cameraAttachTransform.localRotation != Quaternion.identity)
            {
                transform.forward = _cameraAttachTransform.forward;
                _cameraAttachTransform.localRotation = Quaternion.identity;
            }
            
            transform.position += transform.forward * (Speed * Time.deltaTime);
        }

        private void RunPerform()
        {
            if (!_canAction) return;

            _isRunning = !_isRunning;
        }

        private void AttackPerform()
        {
            if(!_canAction || isDead) return;

            PhotonView.RPC(nameof(AttackPerform_RPC), RpcTarget.AllViaServer);
        }

        [PunRPC]
        public void AttackPerform_RPC()
        {
            _swordCollider.enabled = true;
            _canAction = false;

            _animator.SetTrigger(IsAttacking);
            SoundManager.instance.PlaySFX("Attack", transform.position);
        }

        public void OnEndAttackAnimation()
        {
            _swordCollider.enabled = false;
            _canAction = true;
        }

        private void PressMoveButton(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
        }

        private void PressRunButton(InputAction.CallbackContext context)
        {
            RunPerform();
        }

        private void PressAttackButton(InputAction.CallbackContext context)
        {
            AttackPerform();
        }

        private void OnTouchScreen(InputAction.CallbackContext context)
        {
            
            _touchPosition = context.ReadValue<Vector2>();
            _isTouching = true;
        }

        private void OnReleaseScreen(InputAction.CallbackContext context)
        {
            
            _isTouching = false;
            _isTouchFlagDirty = false;
            _isMoving = false;
        }

        #endregion

        #region AI
        public void InitAI()
        {
            
        }


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
                _isMoving = false;
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
                _isMoving = true;
                _moveElapsed += Time.deltaTime;
            }
        }
        #endregion

        #region Interface
        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
        }

        public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //송신
            if (stream.IsWriting)
            {
                stream.SendNext(_isMoving);
            }
            //수신
            else
            {
                _isMoving = (bool)stream.ReceiveNext();
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if(eventCode == Lobby.Network.PhotonEventCode.SYNC_PLAYMODE)
            {
                object[] data = (object[])photonEvent.CustomData;
                if (PhotonView.ViewID == (int)data[1])
                {
                    PlayMode = (PlayMode)data[0];
                    if (PlayMode == PlayMode.AI)
                    {
                        InitAI();
                    }
                    else if (PlayMode == PlayMode.Player)
                    {
                       InitPlayer(); 
                    }
                }
            }
        }
        #endregion
    }
}
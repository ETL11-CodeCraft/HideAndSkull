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
        private const float WALK_SPEED = 3f;
        private const float RUN_SPEED = 5f;
        private const float ROTATE_SPEED = 5f;
        private readonly Vector3 _cameraOffset = new Vector3(0, 2.5f, -3.5f);
        private readonly Quaternion _cameraRotation = new Quaternion(0.075f, 0, 0, 1f);

        //AI, Player 공통
        private bool _isRunning;
        private bool _isDead;
        private ActFlag _currentAct;
        private Animator _animator;
        private Renderer[] _skinnedMeshRenderers = new Renderer[4];

        //AI
        private readonly WaitForSeconds _idleWait = new WaitForSeconds(IDLE_DURATION);
        private readonly WaitForSeconds _stopAfterMoveWait = new WaitForSeconds(STOP_AFTER_MOVE);
        private Coroutine _aiActCoroutine;

        //Player
        [SerializeField] private BoxCollider _boxCollider;
        private Transform _cameraAttachTransform;
        //DEBUG
        PlayerInputActions inputActions;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _boxCollider.enabled = false;
            _skinnedMeshRenderers = GetComponentsInChildren<Renderer>();
        }

        public void Die()
        {
            if(!_isDead)
            {
                Debug.Log(1);
                //TODO :: 공격 하지 못하도록 막기
                _animator.SetTrigger(IsDead);
                _isDead = true;
            }
        }

        public void OnEndDieAnimation()
        {
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
            SetPlayerCamera();

            //TEST
            inputActions = new PlayerInputActions();
            inputActions.Enable();
            inputActions.Player.Move.performed += PressMoveButton;
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

        public void PressRightButton()
        {
            _cameraAttachTransform.Rotate(Vector3.up * ROTATE_SPEED);
        }

        public void PressLeftButton()
        {
            _cameraAttachTransform.Rotate(Vector3.down * ROTATE_SPEED);
        }

        public void PressUpButton()
        {
            if (_cameraAttachTransform.localRotation != Quaternion.identity)
            {
                transform.forward = _cameraAttachTransform.forward;
                _cameraAttachTransform.localRotation = Quaternion.identity;
            }
            
            transform.position += transform.forward * Speed * Time.deltaTime;
        }

        public void PressRunButton()
        {
            _isRunning = !_isRunning;
        }

        public void PressAttackButton()
        {
            _boxCollider.enabled = true;
            _animator.SetTrigger(IsAttacking);
        }

        public void OnEndAttackAnimation()
        {
            _boxCollider.enabled = false;
        }

        public void PressMoveButton(InputAction.CallbackContext context)
        {
            Vector2 movement = context.ReadValue<Vector2>();
            if(movement.x > 0)
            {
                PressRightButton();
            }
            if(movement.x < 0)
            {
                PressLeftButton();
            }
            if(movement.y > 0)
            {
                PressUpButton();
            }
        }

        public void PressRunButton(InputAction.CallbackContext context)
        {
            PressRunButton();
        }

        public void PressAttackButton(InputAction.CallbackContext context)
        {
            PressAttackButton();
        }
        #endregion

        #region AI
        /// <summary>
        /// AI라면 해당 함수를 한번 실행
        /// </summary>
        public void StartAIAct()
        {
            // 코루틴은 한번만 실행 / 사용자가 플레이하는 캐릭터는 실행할 수 없음
            if (_aiActCoroutine != null || PlayMode == PlayMode.Player) return;

            _aiActCoroutine = StartCoroutine(Act());
        }

        /// <summary>
        /// AI의 행동을 랜덤으로 골라 실행하는 함수
        /// </summary>
        private IEnumerator Act()
        {
            while (true)
            {
                _currentAct = (ActFlag)Random.Range(1, 3);
                switch (_currentAct)
                {
                    case ActFlag.Idle:
                        yield return Idle();
                        break;
                    case ActFlag.Move:
                        yield return Move();
                        yield return _stopAfterMoveWait;
                        break;
                }
            }
        }

        private IEnumerator Idle()
        {
            yield return _idleWait;
        }

        private IEnumerator Move()
        {
            Vector2 tempDirection = Random.insideUnitCircle.normalized * (Speed * Time.deltaTime);
            Vector3 randomDirection = new Vector3(tempDirection.x, 0, tempDirection.y);
            transform.rotation = Quaternion.LookRotation(randomDirection);

            for (float elapsedTIme = 0f; elapsedTIme < MOVE_DURATION; elapsedTIme += Time.deltaTime)
            {
                transform.position += randomDirection;
                yield return null;
            }
        }
        #endregion
    }
}
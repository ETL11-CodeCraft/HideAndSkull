using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
        private const float IDLE_DURATION = 3f;
        private const float MOVE_DURATION = 5f;
        private const float STOP_AFTER_MOVE = 1f;
        private const float WALK_SPEED = 3f;
        private const float RUN_SPEED = 5f;
        private const float ROTATE_SPEED = 5f;
        private readonly Vector3 _cameraOffset = new Vector3(0, 2.5f, -3.5f);
        private readonly Quaternion _cameraRotation = new Quaternion(0.075f, 0, 0, 1f);

        //AI, Player 공통
        private bool _isRunning = false;
        private ActFlag _currentAct;

        //AI
        private readonly WaitForSeconds _idleWait = new WaitForSeconds(IDLE_DURATION);
        private readonly WaitForSeconds _stopAfterMoveWait = new WaitForSeconds(STOP_AFTER_MOVE);
        private Coroutine _aiActCoroutine;

        //Player
        private Transform _cameraAttachTransform;
        

        private void Start()
        {
            SetPlayerCamera();
        }
        
        #region Player
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
            _cameraAttachTransform.Rotate(Vector3.down * ROTATE_SPEED);
        }

        public void PressLeftButton()
        {
            _cameraAttachTransform.Rotate(Vector3.up * ROTATE_SPEED);
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
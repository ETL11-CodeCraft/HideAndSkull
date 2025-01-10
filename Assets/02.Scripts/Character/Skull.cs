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


        private const float IDLE_DURATION = 3f;
        private const float MOVE_DURATION = 5f;
        private const float STOP_AFTER_MOVE = 1f;

        private float _speed = 3f;  //프레임당 이동거리

        private WaitForSeconds _idleWait = new WaitForSeconds(IDLE_DURATION);
        private WaitForSeconds _stopAfterMoveWait = new WaitForSeconds(STOP_AFTER_MOVE);
        private Coroutine _aiActCoroutine;
        private ActFlag _currentAct;


        private void Start()
        {
            //TEST
            StartAIAct();
        }

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
            Vector2 tempDirection = Random.insideUnitCircle.normalized * _speed * Time.deltaTime;
            Vector3 randomDirection = new Vector3(tempDirection.x, 0, tempDirection.y);
            transform.rotation = Quaternion.LookRotation(randomDirection);
            
            for(float elapsedTIme = 0f;  elapsedTIme < MOVE_DURATION; elapsedTIme+=Time.deltaTime)
            {
                transform.position += randomDirection;
                yield return null;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _floorPrefab; 
    private int _floorCount = 7;  //생성할 바닥의 개수
    private float _floorSize = 14f;  //바닥 크기
    private List<Vector3> _floorPositionsList = new List<Vector3>();  //바닥 위치 저장하는 리스트

    [SerializeField] private GameObject[] _fencePrefabs;  //울타리 프리팹 배열
    [SerializeField] private float[] _weights; //가중치 배열 - 울타리 프리팹을 위함 
    [SerializeField] private GameObject[] _objectPrefabs; //오브젝트 프리팹 배열

    void Start()
    {
        GenerateFloors();

        PlaceRandomObject();

    }

    /// <summary>
    /// 바닥 생성 함수
    /// </summary>
    private void GenerateFloors()
    {
        Vector3 currentPos = Vector3.zero;  //첫 번째 바닥 (0,0,0)에 생성 
        _floorPositionsList.Add(currentPos);

        for (int i = 0; i < _floorCount; i++)
        {
            if (i == 0)
            {
                PlaceFloor(currentPos);
            }
            else
            {
                currentPos = GetRandomPosition();
                PlaceFloor(currentPos);
            }
        }

        foreach (Vector3 floor in _floorPositionsList)
        {
            PlaceFence(floor);
        }
    }

    /// <summary>
    /// 바닥 배치 함수
    /// </summary>
    /// <param name="position"></param>
    private void PlaceFloor(Vector3 position)
    {
        if (!_floorPrefab)
        {
            Debug.Log("There's no Prefabs");
            return;
        }

        Instantiate(_floorPrefab, position, Quaternion.identity);

        _floorPositionsList.Add(position);

    }

    /// <summary>
    /// 랜덤 위치 값을 반환해주는 함수
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPosition()
    {
        Vector3 newPos;  //배치할 위치

        if (_floorPositionsList.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0), //오른쪽
            new Vector3(-_floorSize,0,0), //왼쪽
            new Vector3(0,0,_floorSize), //위쪽
            new Vector3(0,0,-_floorSize)  //아래쪽
        };

        do
        {
            // 기존 바닥 중 하나를 랜덤으로 생성
            Vector3 basePos = _floorPositionsList[Random.Range(0, _floorPositionsList.Count)];

            // 상하좌우 중 랜덤 방향 선택
            Vector3 randomDir = directions[Random.Range(0, directions.Length)];

            //새로운 위치 계산
            newPos = basePos + randomDir;
        }
        while (_floorPositionsList.Contains(newPos));

        return newPos;
    }


    /// <summary>
    /// 현재 바닥이 가장자리인지 확인하고, 리스트로 반환하는 함수
    /// </summary>
    /// <param name="Position"></param>
    /// <returns></returns>
    private void PlaceFence(Vector3 Position)
    {
        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0),  //오른쪽
            new Vector3(-_floorSize,0,0), //왼쪽
            new Vector3(0,0,_floorSize),  //위쪽
            new Vector3(0,0,-_floorSize)  //아래쪽
        };

        //각 방향에 바닥이 있는지 확인
        foreach (Vector3 direction in directions)
        {
            Vector3 neighborPos = Position + direction;
            GameObject FencePrefab = GetRandomFencePrefab();

            if (!_floorPositionsList.Contains(neighborPos))
            {
                Vector3 FencePos = Position + direction / 2;
                if (direction.x != 0)
                    Instantiate(FencePrefab, FencePos, Quaternion.Euler(0,90,0));
                else if (direction.z != 0)
                    Instantiate(FencePrefab, FencePos, Quaternion.identity);
            }
        }
    }

    private bool isValidWeights()
    {
        if(_fencePrefabs.Length == _weights.Length)
            return true;
        return false;
    }

    private GameObject GetRandomFencePrefab()
    {
        if (isValidWeights())
        {
            float totalWeight = _weights.Sum();  //가중치 합 계산 

            float randomNum = Random.Range(0, totalWeight);  // 0~totalWeight 사이의 랜덤 값 생성 

            float cumulativeWeight = 0f;
            for (int i = 0; i < _fencePrefabs.Length; i++)
            {
                cumulativeWeight += _weights[i];
                if (randomNum <= cumulativeWeight)
                    return _fencePrefabs[i];
            }
        }
        else
        {
            Debug.Log("가중치 값과 프리팹의 개수가 맞지 않습니다.");
        }

        return _fencePrefabs[0];
    }

    private void PlaceRandomObject()
    {
        //오브젝트들이 하나씩 무조건 배치 되어야 함 

        //배치된 위치는 랜덤이되 , 배치된 곳에 배치되면 안됨 (중복 방지)

        //타일의 중앙에만 위치하지 않도록 랜덤한 변위 추가 
        if (_objectPrefabs.Length == 0)
        {
            Debug.Log("No Object prefabs assigned.");
            return;
        }

        HashSet<Vector3> usedPosition = new HashSet<Vector3>();

        foreach (GameObject obj in _objectPrefabs)
        {
            int attempts = 0; //무한루프 방지용 시도 횟수 제한 
            while (attempts < 100)
            {
                Vector3 basePosition = _floorPositionsList[Random.Range(0, _floorPositionsList.Count)];

                Vector3 randomOffset = new Vector3(
                    Random.Range(-_floorSize, _floorSize),
                    0,
                    Random.Range(-_floorSize, _floorSize)
                    );

                Vector3 finalPosition = basePosition + randomOffset;

                if (usedPosition.Contains(finalPosition))
                {
                    attempts++;
                    continue;
                }

                Instantiate(obj, finalPosition, Quaternion.identity);

                AddPositionsToForbidden(finalPosition,usedPosition);
                break;

                if (attempts >= 100)
                {
                    Debug.LogWarning("시도횟수가 100회 이상이라 실패헀습니다.");
                }
            }
        }
    }

    private void AddPositionsToForbidden(Vector3 position, HashSet<Vector3> forbiddenPositions)
    {
        Vector3[] directions =
        {
        new Vector3(_floorSize / 2f, 0, 0),  // 오른쪽
        new Vector3(-_floorSize / 2f, 0, 0), // 왼쪽
        new Vector3(0, 0, _floorSize / 2f),  // 위쪽
        new Vector3(0, 0, -_floorSize / 2f), // 아래쪽
        new Vector3(_floorSize / 2f, 0, _floorSize / 2f),  // 대각선 오른쪽 위
        new Vector3(-_floorSize / 2f, 0, _floorSize / 2f), // 대각선 왼쪽 위
        new Vector3(_floorSize / 2f, 0, -_floorSize / 2f), // 대각선 오른쪽 아래
        new Vector3(-_floorSize / 2f, 0, -_floorSize / 2f) // 대각선 왼쪽 아래
    };

        // 금지된 위치 추가
        forbiddenPositions.Add(position); // 자신도 금지 영역에 포함
        foreach (Vector3 dir in directions)
        {
            forbiddenPositions.Add(position + dir);
        }
    }
}

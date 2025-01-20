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
    private float[] _weights = { 0.5f, 0.4f, 0.1f}; //가중치 배열 - 울타리 프리팹을 위함 
    [SerializeField] private GameObject[] _objectPrefabs; //오브젝트 프리팹 배열
    private float _minDistance = 3.8f;
    void Start()
    {
        GenerateFloors();

        PlaceObjectRandomly(_floorPositionsList, _objectPrefabs, _minDistance);

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

    /// <summary>
    /// 가중치 배열 크기와 울타리 프리팹의 개수가 같은지 확인하는 함수 
    /// </summary>
    /// <returns></returns>
    private bool isValidWeights()
    {
        if(_fencePrefabs.Length == _weights.Length)
            return true;
        return false;
    }

    /// <summary>
    /// 울타리 프리팹중 랜덤으로 하나 가져오는 함수
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 랜덤 오브젝트 배치 함수
    /// </summary>
    /// <param name="tileCenters">타일의 중심점</param>
    /// <param name="objcetPrefab">오브젝트 프리팹 배열</param>
    /// <param name="minDistance">최소거리</param>
    private void PlaceObjectRandomly(List<Vector3> tileCenters, GameObject[] objcetPrefab, float minDistance)
    {
        int randomPositionPerTile = 3;

        //1. 랜덤 포지션 리스트 생성 
        List<Vector3> randomPositions = GenerateRandomPositionList(tileCenters,  randomPositionPerTile);

        //2. 리스트 무작위 정렬 
        randomPositions.OrderBy(x => Random.value).ToList();

        //3. 배치된 위치 저장 
        HashSet<Vector3> usedPositions = new HashSet<Vector3> ();

        //4. 랜덤 위치에서 오브젝트 생성 
        PlacedObjectsFromRandomPosition(randomPositions, _objectPrefabs, usedPositions, minDistance);
    }


    /// <summary>
    /// 랜덤 포지션 리스트를 생성하여 반환하는 함수
    /// </summary>
    /// <param name="tileCenters"></param>
    /// <param name="countPerTile"></param>
    /// <returns></returns>
    private List<Vector3> GenerateRandomPositionList(List<Vector3> tileCenters, int countPerTile) 
    {
        List<Vector3> allPosition = new List<Vector3>();

        foreach (Vector3 center in tileCenters)
        {
            List<Vector3> randomPositions = GenerateRandomPositions(center, countPerTile);
            allPosition.AddRange(randomPositions);
        }

        return allPosition;
    }

    /// <summary>
    /// 각 타일별로 랜덤 위치를 반환하는 함수 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private List<Vector3> GenerateRandomPositions(Vector3 center, int count)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            //랜덤한 오프셋 생성 
            float offsetX = Random.Range(-_floorSize / 3f, _floorSize / 3f);
            float offsetZ = Random.Range(-_floorSize / 3f, _floorSize / 3f);

            //중심에서 약간 떨어진 위치 계산 
            Vector3 randomPos = center + new Vector3(offsetX, 0, offsetZ);

            //타일 중심과 동일한 위치는 제외 
            if (randomPos != center)
            {
                positions.Add(randomPos);
            }
        }

        return positions;
    }

    /// <summary>
    /// 랜덤한 위치에 오브젝트를 배치하는 함수 
    /// </summary>
    /// <param name="randomPositions"></param>
    /// <param name="objectPrefabs"></param>
    /// <param name="usedPositions"></param>
    /// <param name="minDistance"></param>
    private void PlacedObjectsFromRandomPosition(List<Vector3> randomPositions, GameObject[] objectPrefabs, HashSet<Vector3> usedPositions, float minDistance)
    {
        foreach (GameObject prefab in objectPrefabs)
        {
            foreach (Vector3 position in randomPositions)
            {
                // 최소 거리 조건 확인 
                if (isPositionTooClose(position, usedPositions, minDistance))
                {
                    Debug.Log($"{position}: 이 위치는 너무 가깝습니다.");
                    continue;
                }

                //오브젝트 배치 
                Instantiate(prefab, position, Quaternion.identity);

                //금지된 위치 추가
                usedPositions.Add(position);
                break;
            }
        }
    }

    /// <summary>
    /// 너무 근접한 거리에 있는지 확인하는 함수
    /// </summary>
    /// <param name="position"></param>
    /// <param name="usedPositions"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    private bool isPositionTooClose(Vector3 position, HashSet<Vector3> usedPositions, float minDistance)
    {
        foreach (Vector3 used in usedPositions)
        {
            if (Vector3.Distance(position, used) < minDistance)
            {
                return true;
            }
        }
        return false;
    }
}

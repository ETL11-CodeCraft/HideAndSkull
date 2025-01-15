using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// 랜덤으로 오브젝트를 배치하는 함수
    /// </summary>
    private void PlaceRandomObject()
    {
        if (_objectPrefabs.Length == 0)
        {
            Debug.Log("No Object prefabs assigned.");
            return;
        }

        HashSet<Vector3> usedPositions = new HashSet<Vector3>();
        //_floorPositionsList.OrderBy(x=>Random.value).ToList();
        foreach (Vector3 pos in _floorPositionsList)
        {
            Debug.Log(pos);
        }
        foreach (GameObject obj in _objectPrefabs)
        {
            float prefabRadius = GetPrefabRadius(obj);  //프리팹 반경 계산

            bool placed = false; //배치 여부 확인 

            foreach (Vector3 basePosition in _floorPositionsList )
            {
                // 타일 내부 랜덤 변위
                Vector3 randomOffset = new Vector3(
                    Random.Range(-_floorSize / 3f, _floorSize / 3f),
                    0,
                    Random.Range(-_floorSize / 3f, _floorSize / 3f)
                );
                Vector3 finalPosition = basePosition + randomOffset;

                // 최소 거리 조건 확인
                if (isPositionTooClose(finalPosition, usedPositions, prefabRadius))
                {
                    // 오브젝트 배치
                    Instantiate(obj, finalPosition, Quaternion.identity);

                    // 금지된 위치 추가
                    AddPositionsToForbidden(finalPosition, prefabRadius, usedPositions);

                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"Could not place object {obj.name}. No valid positions available.");
            }
        }
    }

    /// <summary>
    /// 거리가 너무 가까운지 확인하는 함수 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="usedPosition"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private bool isPositionTooClose(Vector3 position, HashSet<Vector3> usedPosition, float radius)
    {
        foreach (Vector3 used in usedPosition)
        {
            if (Vector3.Distance(position, used) < radius)
            {
                return false;
            }
        }

        return true;   //원하는 위치가 사용된 위치들에서 해당 반경에 해당이 안될 때 false반환 
    }

    /// <summary>
    /// 주변 영영을 배치 금지 위치로 리스트에 추가하는 함수
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="forbiddenPositions"></param>
    private void AddPositionsToForbidden(Vector3 position, float radius, HashSet<Vector3> forbiddenPositions)
    {
        forbiddenPositions.Add(position);  //중심 위치를 금지된 위치에 추가 

        // 반경을 기준으로 주변 영역을 금지 위치로 추가
        float step = radius / 2f; // 금지 영역 간격을 조정
        for (float x = -radius; x <= radius; x += step)
        {
            for (float z = -radius; z <= radius; z += step)
            {
                Vector3 offset = new Vector3(x, 0, z);
                if (offset.magnitude <= radius) // 원 형태의 금지 영역
                {
                    forbiddenPositions.Add(position + offset);
                }
            }
        }
    }

    /// <summary>
    /// 프리팹의 반경을 반환하는 함수
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private float GetPrefabRadius(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float maxExtent = 0f;

            foreach (Renderer renderer in renderers)
            {
                maxExtent = Mathf.Max(maxExtent, renderer.bounds.extents.x, renderer.bounds.extents.z);
            }
            Debug.Log($"{prefab.name} maxExtent : {maxExtent}");
            return maxExtent;
        }
        else
        {
            Debug.LogWarning($"Prefab {prefab.name} does not have any Renderer components in its hierarchy.");
            return 10f; // 기본값
        }
    }
}

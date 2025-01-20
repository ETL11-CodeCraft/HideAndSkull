using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _floorPrefab; 
    private int _floorCount = 7;  //������ �ٴ��� ����
    private float _floorSize = 14f;  //�ٴ� ũ��
    private List<Vector3> _floorPositionsList = new List<Vector3>();  //�ٴ� ��ġ �����ϴ� ����Ʈ

    [SerializeField] private GameObject[] _fencePrefabs;  //��Ÿ�� ������ �迭
    private float[] _weights = { 0.5f, 0.4f, 0.1f}; //����ġ �迭 - ��Ÿ�� �������� ���� 
    [SerializeField] private GameObject[] _objectPrefabs; //������Ʈ ������ �迭
    private float _minDistance = 3.8f;
    void Start()
    {
        GenerateFloors();

        PlaceObjectRandomly(_floorPositionsList, _objectPrefabs, _minDistance);

    }

    /// <summary>
    /// �ٴ� ���� �Լ�
    /// </summary>
    private void GenerateFloors()
    {
        Vector3 currentPos = Vector3.zero;  //ù ��° �ٴ� (0,0,0)�� ���� 
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
    /// �ٴ� ��ġ �Լ�
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
    /// ���� ��ġ ���� ��ȯ���ִ� �Լ�
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPosition()
    {
        Vector3 newPos;  //��ġ�� ��ġ

        if (_floorPositionsList.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0), //������
            new Vector3(-_floorSize,0,0), //����
            new Vector3(0,0,_floorSize), //����
            new Vector3(0,0,-_floorSize)  //�Ʒ���
        };

        do
        {
            // ���� �ٴ� �� �ϳ��� �������� ����
            Vector3 basePos = _floorPositionsList[Random.Range(0, _floorPositionsList.Count)];

            // �����¿� �� ���� ���� ����
            Vector3 randomDir = directions[Random.Range(0, directions.Length)];

            //���ο� ��ġ ���
            newPos = basePos + randomDir;
        }
        while (_floorPositionsList.Contains(newPos));

        return newPos;
    }


    /// <summary>
    /// ���� �ٴ��� �����ڸ����� Ȯ���ϰ�, ����Ʈ�� ��ȯ�ϴ� �Լ�
    /// </summary>
    /// <param name="Position"></param>
    /// <returns></returns>
    private void PlaceFence(Vector3 Position)
    {
        Vector3[] directions =
        {
            new Vector3(_floorSize,0,0),  //������
            new Vector3(-_floorSize,0,0), //����
            new Vector3(0,0,_floorSize),  //����
            new Vector3(0,0,-_floorSize)  //�Ʒ���
        };

        //�� ���⿡ �ٴ��� �ִ��� Ȯ��
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
    /// ����ġ �迭 ũ��� ��Ÿ�� �������� ������ ������ Ȯ���ϴ� �Լ� 
    /// </summary>
    /// <returns></returns>
    private bool isValidWeights()
    {
        if(_fencePrefabs.Length == _weights.Length)
            return true;
        return false;
    }

    /// <summary>
    /// ��Ÿ�� �������� �������� �ϳ� �������� �Լ�
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomFencePrefab()
    {
        if (isValidWeights())
        {
            float totalWeight = _weights.Sum();  //����ġ �� ��� 

            float randomNum = Random.Range(0, totalWeight);  // 0~totalWeight ������ ���� �� ���� 

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
            Debug.Log("����ġ ���� �������� ������ ���� �ʽ��ϴ�.");
        }

        return _fencePrefabs[0];
    }

    /// <summary>
    /// ���� ������Ʈ ��ġ �Լ�
    /// </summary>
    /// <param name="tileCenters">Ÿ���� �߽���</param>
    /// <param name="objcetPrefab">������Ʈ ������ �迭</param>
    /// <param name="minDistance">�ּҰŸ�</param>
    private void PlaceObjectRandomly(List<Vector3> tileCenters, GameObject[] objcetPrefab, float minDistance)
    {
        int randomPositionPerTile = 3;

        //1. ���� ������ ����Ʈ ���� 
        List<Vector3> randomPositions = GenerateRandomPositionList(tileCenters,  randomPositionPerTile);

        //2. ����Ʈ ������ ���� 
        randomPositions.OrderBy(x => Random.value).ToList();

        //3. ��ġ�� ��ġ ���� 
        HashSet<Vector3> usedPositions = new HashSet<Vector3> ();

        //4. ���� ��ġ���� ������Ʈ ���� 
        PlacedObjectsFromRandomPosition(randomPositions, _objectPrefabs, usedPositions, minDistance);
    }


    /// <summary>
    /// ���� ������ ����Ʈ�� �����Ͽ� ��ȯ�ϴ� �Լ�
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
    /// �� Ÿ�Ϻ��� ���� ��ġ�� ��ȯ�ϴ� �Լ� 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private List<Vector3> GenerateRandomPositions(Vector3 center, int count)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            //������ ������ ���� 
            float offsetX = Random.Range(-_floorSize / 3f, _floorSize / 3f);
            float offsetZ = Random.Range(-_floorSize / 3f, _floorSize / 3f);

            //�߽ɿ��� �ణ ������ ��ġ ��� 
            Vector3 randomPos = center + new Vector3(offsetX, 0, offsetZ);

            //Ÿ�� �߽ɰ� ������ ��ġ�� ���� 
            if (randomPos != center)
            {
                positions.Add(randomPos);
            }
        }

        return positions;
    }

    /// <summary>
    /// ������ ��ġ�� ������Ʈ�� ��ġ�ϴ� �Լ� 
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
                // �ּ� �Ÿ� ���� Ȯ�� 
                if (isPositionTooClose(position, usedPositions, minDistance))
                {
                    Debug.Log($"{position}: �� ��ġ�� �ʹ� �������ϴ�.");
                    continue;
                }

                //������Ʈ ��ġ 
                Instantiate(prefab, position, Quaternion.identity);

                //������ ��ġ �߰�
                usedPositions.Add(position);
                break;
            }
        }
    }

    /// <summary>
    /// �ʹ� ������ �Ÿ��� �ִ��� Ȯ���ϴ� �Լ�
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

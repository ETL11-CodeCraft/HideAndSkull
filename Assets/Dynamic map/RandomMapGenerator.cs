using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _floorPrefab; 
    private int _floorCount = 7;  //������ �ٴ��� ����
    private float _floorSize = 14f;  //�ٴ� ũ��
    private List<Vector3> _floorPositionsList = new List<Vector3>();  //�ٴ� ��ġ �����ϴ� ����Ʈ

    [SerializeField] private GameObject[] _fencePrefabs;  //��Ÿ�� ������ �迭
    [SerializeField] private float[] _weights; //����ġ �迭 - ��Ÿ�� �������� ���� 
    [SerializeField] private GameObject[] _objectPrefabs; //������Ʈ ������ �迭

    void Start()
    {
        GenerateFloors();

        PlaceRandomObject();

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

    private void PlaceRandomObject()
    {
        //������Ʈ���� �ϳ��� ������ ��ġ �Ǿ�� �� 

        //��ġ�� ��ġ�� �����̵� , ��ġ�� ���� ��ġ�Ǹ� �ȵ� (�ߺ� ����)

        //Ÿ���� �߾ӿ��� ��ġ���� �ʵ��� ������ ���� �߰� 
        if (_objectPrefabs.Length == 0)
        {
            Debug.Log("No Object prefabs assigned.");
            return;
        }

        HashSet<Vector3> usedPosition = new HashSet<Vector3>();

        foreach (GameObject obj in _objectPrefabs)
        {
            int attempts = 0; //���ѷ��� ������ �õ� Ƚ�� ���� 
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
                    Debug.LogWarning("�õ�Ƚ���� 100ȸ �̻��̶� ���������ϴ�.");
                }
            }
        }
    }

    private void AddPositionsToForbidden(Vector3 position, HashSet<Vector3> forbiddenPositions)
    {
        Vector3[] directions =
        {
        new Vector3(_floorSize / 2f, 0, 0),  // ������
        new Vector3(-_floorSize / 2f, 0, 0), // ����
        new Vector3(0, 0, _floorSize / 2f),  // ����
        new Vector3(0, 0, -_floorSize / 2f), // �Ʒ���
        new Vector3(_floorSize / 2f, 0, _floorSize / 2f),  // �밢�� ������ ��
        new Vector3(-_floorSize / 2f, 0, _floorSize / 2f), // �밢�� ���� ��
        new Vector3(_floorSize / 2f, 0, -_floorSize / 2f), // �밢�� ������ �Ʒ�
        new Vector3(-_floorSize / 2f, 0, -_floorSize / 2f) // �밢�� ���� �Ʒ�
    };

        // ������ ��ġ �߰�
        forbiddenPositions.Add(position); // �ڽŵ� ���� ������ ����
        foreach (Vector3 dir in directions)
        {
            forbiddenPositions.Add(position + dir);
        }
    }
}

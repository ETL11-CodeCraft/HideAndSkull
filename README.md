# HideAndSkull
- Unity 3D Mobile Team Project
- PhotonNetwork를 사용한 멀티플레이 게임

## 목차
- 시연영상
- 게임 방법 및 시스템
- 코드 설명
- 발표자료
<br><br><br>
## 시연영상
https://youtu.be/w2tk3_-yusU?si=3MLNqgTK4ONVqmmg
<br>

## 게임 방법 및 시스템
### 로비 및 룸 입장
![image](https://github.com/user-attachments/assets/92e19465-0b40-466f-89c9-d29bc9af56e1)

![image](https://github.com/user-attachments/assets/f308687b-a387-484e-9552-533a0d2f1dee)

### 채팅
![image](https://github.com/user-attachments/assets/40a1e5f7-9e84-408e-a2cb-d155f537f0ff)

### 플레이어 조작
![image](https://github.com/user-attachments/assets/7ed6140d-cb62-461b-b841-061ac81fa507)

### 맵 생성
![image](https://github.com/user-attachments/assets/148470db-9f95-4ed3-a18e-e84911d33d7d)

![image](https://github.com/user-attachments/assets/27dc3b5d-45a0-42f9-bda7-ea8fbbf1bc62)

### AI 생성 및 행동 패턴
![image](https://github.com/user-attachments/assets/202c612c-5ae8-4497-9763-cabfc1224704)

### 생존 인원 및 Kill 알림
![image](https://github.com/user-attachments/assets/2ab3ab35-5166-4a93-b8e2-0fa305191238)

### 설정창
![image](https://github.com/user-attachments/assets/c79aefcd-8024-473f-85f5-4cb81b583ad5)
<br>

## 코드 설명
### UI
UI를 효율적으로 관리하기 위한 클래스들을 만들었습니다.<br>
Attribute를 상속받은 ResolveAttribute 클래스를 만들어 [Resolve] Attribute를 통해 DI 해야할 변수에 접근할 수 있도록 했습니다.<br>
ComponentResolvingBehavior 클래스를 만들어 [Resolve] Attribute가 붙은 변수들에 리플렉션을 이용해 접근하여 DI 했습니다.
```
private void ResolveAll()
{
    Type type = GetType();
    FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
    StringBuilder stringBuilder = new StringBuilder(40);
  
    for (int i = 0; i < fieldInfos.Length; i++)
    {
        ResolveAttribute resolveAttribute = fieldInfos[i].GetCustomAttribute<ResolveAttribute>();
  
        if (resolveAttribute != null)
        {
            stringBuilder.Clear();
            string prefix = ResolvePrefixTable.GetPrefix(fieldInfos[i].FieldType);
            stringBuilder.Append(prefix);
            string fieldName = fieldInfos[i].Name;
            bool isFirstCharacter = true;
  
            //_camelCase -> PascalCase
            for (int j = 0; j < fieldName.Length; j++)
            {
                if (isFirstCharacter)
                {
                    if (fieldName[j].Equals('_'))
                        continue;
  
                    stringBuilder.Append(char.ToUpper(fieldName[j]));
                    isFirstCharacter = false;
                }
                else
                {
                    stringBuilder.Append(fieldName[j]);
                }
            }
  
            Transform child = transform.FindChildReculsively(stringBuilder.ToString());
  
            if(child)
            {
                Component childComponenet = child.GetComponent(fieldInfos[i].FieldType);
                fieldInfos[i].SetValue(this, childComponenet);
            }
            else
            {
                Debug.LogError($"[{name}] : Can not resolve field {fieldInfos[i].Name}");
            }
        }
    }
}
```
해당 코드를 모든 UI에서 사용할 수 있도록 ComponentResolvingBehavior를 상속받은 UI_Base클래스를 만들고 해당 클래스를 상속받은 UI_Screen과 UI_Popup으로 분리하여 관리하였습니다.<br>
Popup은 한 화면에 여러개 띄울수 있고, Screen은 하나만 띄울수 있으며, 해당 UI들의 관리는 UI_Manager라는 싱글톤 클래스에서 합니다.<br>
UI_Manager에서는 UI를 Regist하고 Resolve할 수 있도록 관리합니다.

```
public void Register(UI_Base ui)
{
    if (_uis.ContainsKey(ui.GetType()))
    {
        Debug.LogWarning($"UI {ui.GetType()} is already registered. Replacing the old instance.");

        if (ui is UI_Popup)
        {
            ui.onShow.AddListener(() => Push((UI_Popup)ui));
            ui.onHide.AddListener(() => Pop((UI_Popup)ui));
        }

        _uis[ui.GetType()] = ui; // 기존 UI를 새 UI로 교체
    }
    else if (_uis.TryAdd(ui.GetType(), ui))
    {
        Debug.Log($"Registered UI {ui.GetType()}");

        if (ui is UI_Popup)
        {
            ui.onShow.AddListener(() => Push((UI_Popup)ui));
            ui.onHide.AddListener(() => Pop((UI_Popup)ui));
        }
    }
    else
    {
        throw new Exception($"Failed to register ui {ui.GetType()}. already exist");
    }
}
```
```
public T Resolve<T>()
            where T : UI_Base
{
    if (_uis.TryGetValue(typeof(T), out UI_Base result))
    {
        if(result != null)
            return (T)result;
    }

    string path = $"UI/Canvas - {typeof(T).Name.Substring(3)}";
    UI_Base prefab = Resources.Load<UI_Base>(path);

    if (prefab == null)
        throw new Exception($"Failed to resolve ui {typeof(T)}. Not exist");

    return (T)GameObject.Instantiate(prefab);
}
```

### 맵 생성
동적으로 맵을 생성할 때 PhotonNetwork.Instantiate를 사용하던 방식에서 동일한 시드값을 사용한 랜덤생성 방식으로 변경하였습니다. <br>
해당 방식을 통해 네트워크 트래픽이 감소됨을 측정할 수 있었습니다.
  - (Incoming) 103,396 byte -> 34,944 byte
  - (Outgoing) 18,033 byte -> 2,189 byte
  - 맵생성갯수600개, 실행후5초후기준
```
void Start()
{
    if (SceneManager.GetActiveScene().buildIndex.Equals(1) && PhotonNetwork.IsMasterClient)
    {
        int mapSeed = Random.Range(0, int.MaxValue);
        
        _photonView.RPC(nameof(GenerateMap), RpcTarget.AllBuffered, mapSeed);
        
        _workflow.CachedCharacterPosition(GenerateRandomPositionList(_floorPositionsList, 10), usedPositions);
    }
}

[PunRPC]
private void GenerateMap(int seed)
{
    Random.InitState(seed);
    GenerateFloors();
    PlaceObjectRandomly(_objectPrefabs, MIN_DISTANCE);
}
```

### 캐릭터 동기화
캐릭터를 생성하고 해당 스폰위치로 이동시킨후 각 플레이어에게 권한을 넘기는 방식을 사용합니다.<br>
TransferOwnership을 통해 해당 권한을 넘기고 RaiseEvent를 통해 자신이 플레이어인지 AI인지 넘긴후 권한을 가지고 있는 플레이어가 해당 캐릭터를 조종하는 방식입니다.
```
_characters[cnt].transform.position = spawnPoint + Vector3.up;
_usedPositions.Add(spawnPoint);

PhotonView photonView = _characters[cnt].GetComponent<PhotonView>();

if(cnt >= 0 && cnt < _playerList.Count)
{
    photonView.TransferOwnership(_playerList[cnt].ActorNumber);
    
    PhotonNetwork.RaiseEvent(Lobby.Network.PhotonEventCode.SYNC_PLAYMODE,
        new object[] { PlayMode.Player, photonView.ViewID },
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        SendOptions.SendReliable);
}
else
{
    PhotonNetwork.RaiseEvent(Lobby.Network.PhotonEventCode.SYNC_PLAYMODE,
        new object[] { PlayMode.AI, photonView.ViewID },
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        SendOptions.SendReliable);
}
```

캐릭터 스크립트인 Skull에는 인터페이스 분리원칙을 지키기 위해 PunBehaviour를 상속받지 않고, 사용하는 인터페이스만을 상속받아 구현하였습니다. <br>
IPunObservable을 상속받아 구현한 OnPhotonSerializeView에서 플레이어가 이동중인지 전달받아 해당 애니메이션을 동기화 하였습니다.
```
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
```

캐릭터의 공격은 RPC를 통해 호출하였습니다.
```
[PunRPC]
public void AttackPerform_RPC()
{
    _swordCollider.enabled = true;
    _canAction = false;

    _animator.SetTrigger(IsAttacking);
    SoundManager.instance.PlaySFX("Attack", transform.position);
}
```

캐릭터의 킬수, 자신이 죽었는지등에 대한 정보는 CustomProperty에 저장합니다.
```
//맞았는지 판단은 맞은 Skull에서 함
if(other.TryGetComponent(out PhotonView photonView) && 
   photonView.IsMine)
{
    if (other.TryGetComponent(out Skull attackedSkull))
    {
        if (attackedSkull.isDead)
            return;

        //모든 플레이어에게 해당 character가 죽었다고 호출함
        attackedSkull.PhotonView.RPC(nameof(attackedSkull.Die), RpcTarget.All);

        if (attackedSkull.PlayMode == PlayMode.Player)
        {
            int killcount = (int)SwordOwner.PhotonView.Owner.CustomProperties["KillCount"];
            SwordOwner.PlayerCustomProperty["KillCount"] = killcount + 1;
            SwordOwner.PhotonView.Owner.SetCustomProperties(SwordOwner.PlayerCustomProperty);

            attackedSkull.PlayerCustomProperty["IsDead"] = true;
            attackedSkull.PhotonView.Owner.SetCustomProperties(attackedSkull.PlayerCustomProperty);
            UI_ToastPanel uI_ToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
            uI_ToastPanel.ShowToast($"{photonView.Owner.NickName}님이 사망하였습니다.");
        }
    }
}
```
<br>

## 발표자료
https://docs.google.com/presentation/d/1YNm52cihYWhvS1Ze_MygPpIqLfC8t3gFad-gpCiHV30/edit#slide=id.g32816f25803_0_97

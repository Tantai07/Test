using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class DungeonMaker : MonoBehaviour
{
    public Vector2Int roomSize;
    public Room[,] rooms;
    public float roomDistance;
    public GameObject[] RoomsPrefab;
    public GameObject bridge;
    public int roomCount;
    public int RandomDeleteAmount;
    public int Delay;

    private delegate void CustomAction(Room _room);
    public List<Room> DungeonRoom;
    public List<GameObject> bridgeCreated = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rooms = new Room[roomSize.x + 1,roomSize.y + 1];
        CreateGrid();
        CreateFirstRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CreateFirstRoom()
    {
        if(roomCount <= 0)
            return;
        Vector2Int randomPos = new Vector2Int(Random.Range(1,roomSize.x), Random.Range(1, roomSize.y));
        rooms[randomPos.x, randomPos.y].AssignGameObject(RoomsPrefab[Random.Range(1, RoomsPrefab.Length)]);
        Room startPos = rooms[randomPos.x, randomPos.y];
        DungeonRoom.Add(rooms[randomPos.x, randomPos.y]);
        roomCount--;
        CreateRooms(startPos);
    }
    public void CreateGrid()
    {
        for (int x = 1; x <= roomSize.x; x++)
        {
            for (int y = 1; y <= roomSize.y; y++)
            {
                GameObject grid = Instantiate(RoomsPrefab[0], RoomPosition(x,y),Quaternion.identity);
                grid.name = $"Room {x},{y}";
                Room roomComponent = grid.AddComponent<Room>();
                rooms[x, y] = roomComponent;
                rooms[x, y].pos = new Vector2Int(x, y);
            }
        }
    }
    public void CreateRooms(Room startPos)
    {
        //Debug.Log(startPos.pos);
        if(roomCount > 0 && roomCount < rooms.Length)
            CheckRoom(startPos);
        if (roomCount == 0)
            RoomConnectBridge();
    }
    public void CheckRoom(Room _room)
    {
        List<CustomAction> actions = new List<CustomAction>();
        // Perform boundary checks for each direction
        if (_room.pos.y + 1 < rooms.GetLength(1) && rooms[_room.pos.x, _room.pos.y + 1].roomPrefab == null)
        { // Above
            actions.Add(CreateAbove);
        }
        if (_room.pos.x - 1 > 0 && rooms[_room.pos.x - 1, _room.pos.y].roomPrefab == null)
        { // Left
            actions.Add(CreateLeft);
        }
        if (_room.pos.x + 1 < rooms.GetLength(0) && rooms[_room.pos.x + 1, _room.pos.y].roomPrefab == null)
        { // Right
            actions.Add(CreateRight);
        }
        if (_room.pos.y - 1 > 0 && rooms[_room.pos.x, _room.pos.y - 1].roomPrefab == null)
        { // Below
            actions.Add(CreateBelow);
        }

        //Debug.Log($"Above Function : {_room.pos.y + 1 < rooms.GetLength(1) && rooms[_room.pos.x, _room.pos.y + 1].roomPrefab == null}");
        //Debug.Log($"Left Function : {_room.pos.x - 1 > 0 && rooms[_room.pos.x - 1, _room.pos.y].roomPrefab == null}");
        //Debug.Log($"Right Function : {_room.pos.x + 1 < rooms.GetLength(0) && rooms[_room.pos.x + 1, _room.pos.y].roomPrefab == null}");
        //Debug.Log($"Below Function : {_room.pos.y - 1 > 0 && rooms[_room.pos.x, _room.pos.y - 1].roomPrefab == null}");

        if (actions.Count > 0)
        {
            int randomIndex = Random.Range(0, actions.Count);
            //Debug.Log("randomIndex = " + actions[randomIndex].Method.Name);
            actions[randomIndex]?.Invoke(_room);
        }
        else
        {
            Vector2Int ranRoompos = DungeonRoom[Random.Range(0, DungeonRoom.Count)].pos;
            CreateRooms(rooms[ranRoompos.x,ranRoompos.y]);
        }

    }
    public async void CreateAbove(Room startpos)
    {
        await Task.Delay(Delay);
        Vector2Int pos = new Vector2Int(startpos.pos.x, startpos.pos.y + 1);
        rooms[pos.x,pos.y].AssignGameObject(RoomsPrefab[Random.Range(1, RoomsPrefab.Length)]);
        DungeonRoom.Add(rooms[pos.x,pos.y]);
        roomCount--;
        CreateRooms(rooms[pos.x,pos.y]);
        CreateBridge(startpos, pos);
    }
    public async void CreateLeft(Room startpos)
    {
        await Task.Delay(Delay);
        Vector2Int pos = new Vector2Int(startpos.pos.x - 1, startpos.pos.y);
        rooms[pos.x, pos.y].AssignGameObject(RoomsPrefab[Random.Range(1, RoomsPrefab.Length)]);
        DungeonRoom.Add(rooms[pos.x, pos.y]);
        roomCount--;
        CreateRooms(rooms[pos.x, pos.y]);
        CreateBridge(startpos, pos);
    }
    public async void CreateRight(Room startpos)
    {
        await Task.Delay(Delay);
        Vector2Int pos = new Vector2Int(startpos.pos.x + 1, startpos.pos.y);
        rooms[pos.x, pos.y].AssignGameObject(RoomsPrefab[Random.Range(1,RoomsPrefab.Length)]);
        DungeonRoom.Add(rooms[pos.x, pos.y]);
        roomCount--;
        CreateRooms(rooms[pos.x, pos.y]);
        CreateBridge(startpos, pos);
    }
    public async void CreateBelow(Room startpos)
    {
        await Task.Delay(Delay);
        Vector2Int pos = new Vector2Int(startpos.pos.x, startpos.pos.y - 1);
        rooms[pos.x, pos.y].AssignGameObject(RoomsPrefab[Random.Range(1, RoomsPrefab.Length)]);
        DungeonRoom.Add(rooms[pos.x, pos.y]);
        roomCount--;
        CreateRooms(rooms[pos.x, pos.y]);
        CreateBridge(startpos, pos);
    }
    public GameObject CreateBridge(Room oldPos,Vector2Int curPos)
    {

        Vector3 pos = new Vector3(oldPos.pos.x + curPos.x, oldPos.pos.y + curPos.y, 0) / 2f * roomDistance;
        GameObject Bridge = null;

        if (oldPos.bridge.Exists(b => b.name == $"{oldPos.pos},{curPos}") ||
            rooms[curPos.x, curPos.y].bridge.Exists(b => b.name == $"{curPos},{oldPos.pos}"))
        {
            return null;
        }

        if (oldPos.pos.x > curPos.x || oldPos.pos.x < curPos.x)
        {
           Bridge = Instantiate(bridge, pos, Quaternion.identity);
        }
        if (oldPos.pos.y > curPos.y || oldPos.pos.y < curPos.y)
        {
            Bridge = Instantiate(bridge, pos, Quaternion.Euler(0,0,90));
        }
        Bridge.name = $"{oldPos.pos},{curPos}";
        Bridge.transform.parent = GameObject.Find("Bridge").transform;
        oldPos.bridge.Add(Bridge);
        rooms[curPos.x,curPos.y].bridge.Add(Bridge);
        return Bridge;
    }
    public async void RoomConnectBridge()
    {
        await Task.Delay(100);
        List<(Room, Vector2Int)> bridgesToCreate = new List<(Room, Vector2Int)>();

        foreach (Room _room in DungeonRoom)
        {
            foreach (GameObject _bridge in _room.bridge)
            {
                // Above
                if (_room.pos.y + 1 < rooms.GetLength(1) && rooms[_room.pos.x, _room.pos.y + 1].roomPrefab != null)
                {
                    if (_bridge.name != $"{_room.pos},{rooms[_room.pos.x, _room.pos.y + 1].pos}")
                    {
                        bridgesToCreate.Add((_room, rooms[_room.pos.x, _room.pos.y + 1].pos));
                    }
                }
                // Left
                if (_room.pos.x - 1 > 0 && rooms[_room.pos.x - 1, _room.pos.y].roomPrefab != null)
                {
                    if (_bridge.name != $"{_room.pos},{rooms[_room.pos.x - 1, _room.pos.y].pos}")
                    {
                        bridgesToCreate.Add((_room, rooms[_room.pos.x - 1, _room.pos.y].pos));
                    }
                }
                // Right
                if (_room.pos.x + 1 < rooms.GetLength(0) && rooms[_room.pos.x + 1, _room.pos.y].roomPrefab != null)
                {
                    if (_bridge.name != $"{_room.pos},{rooms[_room.pos.x + 1, _room.pos.y].pos}")
                    {
                        bridgesToCreate.Add((_room, rooms[_room.pos.x + 1, _room.pos.y].pos));
                    }
                }
                // Below
                if (_room.pos.y - 1 > 0 && rooms[_room.pos.x, _room.pos.y - 1].roomPrefab != null)
                {
                    if (_bridge.name != $"{_room.pos},{rooms[_room.pos.x, _room.pos.y - 1].pos}")
                    {
                        bridgesToCreate.Add((_room, rooms[_room.pos.x, _room.pos.y - 1].pos));
                    }
                }
            }
        }
        // Now create the bridges after iterating
        foreach (var bridgeInfo in bridgesToCreate)
        {
            if (bridgeInfo.Item1 != null && bridgeInfo.Item2 != null)
            {
                if(isBridgeValid(bridgeInfo.Item1, bridgeInfo.Item2))
                    bridgeCreated.Add(CreateBridge(bridgeInfo.Item1, bridgeInfo.Item2));
            }
        }
        while (RandomDeleteAmount > 0 && bridgeCreated.Count > 0)
        {
            int ranIndex = Random.Range(0,bridgeCreated.Count);
            Debug.Log($"{ranIndex}");
            Debug.Log($"{bridgeCreated[ranIndex].name}");
            Destroy(bridgeCreated[ranIndex]);
            bridgeCreated.RemoveAt(ranIndex);
            RandomDeleteAmount--;
        }

    }
    public bool isBridgeValid(Room oldPos, Vector2Int curPos)
    {
        if (oldPos.bridge.Exists(b => b.name == $"{oldPos.pos},{curPos}") ||
            rooms[curPos.x, curPos.y].bridge.Exists(b => b.name == $"{curPos},{oldPos.pos}"))
        {
            return false;
        }
        return true;
    }
    public Room ConvertPosToXY(Vector2Int pos)
    {
        return rooms[pos.x,pos.y];
    }
    public Vector2 RoomPosition(int x,int y)
    {
        return new Vector2(x * roomDistance,y * roomDistance);
    }
}
public class Room : MonoBehaviour
{
    public GameObject roomPrefab;
    public List<GameObject> bridge = new List<GameObject>();
    public Vector2Int pos;

    public void AssignGameObject(GameObject go)
    {
        roomPrefab = go;
        GameObject room = Instantiate(roomPrefab, transform);
        room.transform.position = transform.position;
    }
}
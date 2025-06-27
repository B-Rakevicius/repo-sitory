using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomGeneration : MonoBehaviour
{
    [Header("Generation Settings")]
    public int roomCount = 10;
    public GameObject startingRoomPrefab;

    [System.Serializable]
    public class Rule
    {
        public GameObject room; // room prefab
        public int roomWeight = 1; // higher = more likely to spawn
        public int minimumSpawns = 1;
        public int maximumSpawns = -1; // -1 = unlimited
    }
    public List<Rule> roomRules = new List<Rule>();
    private Dictionary<GameObject, int> roomSpawnCounts = new Dictionary<GameObject, int>();

   // public List<GameObject> roomPrefabs = new List<GameObject>();
    public LayerMask roomCollisionLayer;

    private List<RoomScript> spawnedRooms = new List<RoomScript>();
    private List<DoorScript> availableDoors = new List<DoorScript>();
    private List<DoorScript> connectedDoors = new List<DoorScript>();

    [Header("-1 for random seed")]
    public int seed = -1;
    private System.Random rng;
    void Start()
    {
        GenerateDungeon2();
    }
    public void GenerateDungeon2()
    {
        ClearDungeon();
        rng = (seed == -1) ? new System.Random() : new System.Random(seed);
        if (startingRoomPrefab == null || roomRules.Count == 0)
        {
            Debug.LogError("Missing room prefabs in RoomGeneration script");
            return;     
        }
        GameObject startingRoomObj = Instantiate(startingRoomPrefab, Vector3.zero, Quaternion.identity, transform);
        RoomScript startingRoom = startingRoomObj.GetComponent<RoomScript>();
        if (startingRoom == null)
        {
            Debug.LogError("Starting room prefab doesn't have RoomScript attached");
            Destroy(startingRoomObj);
            return;
        }
        spawnedRooms.Add(startingRoom);
        availableDoors.AddRange(startingRoom.doorPoints.Where(d => !d.isExit));
        while (spawnedRooms.Count < roomCount && availableDoors.Count > 0)
        {
            if (!TrySpawnRoom3())
            {
                break;
            }
        }
        Debug.Log($"Dungeon generated with seed: {(seed == -1 ? "Random"  : seed.ToString())}");
        Debug.Log($"Dungeon generation complete. Spawned {spawnedRooms.Count} rooms.");
    }
    private bool TrySpawnRoom3()
    {
        foreach (DoorScript targetDoor in availableDoors.OrderBy(x => rng.NextDouble()).ToList())
        {
            var validRules = roomRules
            .Where(r =>
                r.room != null &&
                (r.maximumSpawns == -1 || !roomSpawnCounts.ContainsKey(r.room) 
                                       || roomSpawnCounts[r.room] < r.maximumSpawns)
                )
                .ToList();
            List<GameObject> weightedRooms = new List<GameObject>();
            foreach (var rule in validRules)
            {
                for (int i = 0; i < rule.roomWeight; i++)
                    weightedRooms.Add(rule.room);
            }
            foreach (GameObject roomPrefab in weightedRooms.OrderBy(x => rng.NextDouble()))
            {
                RoomScript prefabScript = roomPrefab.GetComponent<RoomScript>();
                if (prefabScript == null || prefabScript.doorPoints.Count == 0) continue;

                foreach (DoorScript candidateDoor in prefabScript.doorPoints)
                {
                    foreach (int angle in new int[] { 0, 90, 180, 270 })
                    {
                        Quaternion rotation = Quaternion.Euler(0, angle, 0);
                        Vector3 rotatedForward = rotation * candidateDoor.transform.forward;
                        if (Vector3.Dot(targetDoor.transform.forward, rotatedForward) > -0.99f)
                            continue;

                        Vector3 rotatedLocalPos = rotation * candidateDoor.transform.localPosition;
                        Vector3 spawnPosition = targetDoor.transform.position - rotatedLocalPos;

                        if (!CheckRoomOverlapSimulated(roomPrefab, spawnPosition, rotation))
                        {
                            GameObject newRoomObj = Instantiate(roomPrefab, spawnPosition, rotation, transform);
                            RoomScript newRoom = newRoomObj.GetComponent<RoomScript>();
                            spawnedRooms.Add(newRoom);

                            if (!roomSpawnCounts.ContainsKey(roomPrefab))
                                roomSpawnCounts[roomPrefab] = 0;
                            roomSpawnCounts[roomPrefab]++;

                            ConnectDoors(targetDoor, FindMatchingDoor(newRoom, candidateDoor.name));
                            availableDoors.Remove(targetDoor);

                            foreach (DoorScript newDoor in newRoom.doorPoints)
                            {
                                if (!connectedDoors.Contains(newDoor) && !newDoor.isExit)
                                    availableDoors.Add(newDoor);
                            }

                            return true;
                        }
                    }
                }
            }

            /*
            foreach (GameObject roomPrefab in roomPrefabs.OrderBy(x => rng.NextDouble()))
            {

                
                RoomScript prefabScript = roomPrefab.GetComponent<RoomScript>();
                if (prefabScript == null || prefabScript.doorPoints.Count == 0) continue;
                foreach (DoorScript candidateDoor in prefabScript.doorPoints)
                {
                    foreach (int angle in new int[] { 0, 90, 180, 270 })
                    {
                        Quaternion rotation = Quaternion.Euler(0, angle, 0);
                        Vector3 rotatedForward = rotation * candidateDoor.transform.forward;
                        if (Vector3.Dot(targetDoor.transform.forward, rotatedForward) > -0.99f)
                            continue;
                        Vector3 rotatedLocalPos = rotation * candidateDoor.transform.localPosition;
                        Vector3 spawnPosition = targetDoor.transform.position - rotatedLocalPos;
                        if (!CheckRoomOverlapSimulated(roomPrefab, spawnPosition, rotation))
                        {
                            // valid spot found
                            GameObject newRoomObj = Instantiate(roomPrefab, spawnPosition, rotation, transform);
                            RoomScript newRoom = newRoomObj.GetComponent<RoomScript>();
                            spawnedRooms.Add(newRoom);
                            ConnectDoors(targetDoor, FindMatchingDoor(newRoom, candidateDoor.name)); // find correctly rotated doors
                            availableDoors.Remove(targetDoor);
                            foreach (DoorScript newDoor in newRoom.doorPoints)
                            {
                                if (!connectedDoors.Contains(newDoor) && !newDoor.isExit)
                                {
                                    availableDoors.Add(newDoor);
                                }
                            }
                            return true;
                        }
                    }
                }
                
            }
            */
            availableDoors.Remove(targetDoor);
        }
        return false;
    }
    private bool CheckRoomOverlapSimulated(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        RoomScript prefabScript = roomPrefab.GetComponent<RoomScript>();
        if (prefabScript == null || prefabScript.boundObjects.Count == 0)
            return false;
        foreach (GameObject boundObject in prefabScript.boundObjects)
        {
            if (boundObject == null) continue;

            Vector3 worldPosition = position + rotation * boundObject.transform.localPosition;
            Vector3 halfExtents = Vector3.Scale(boundObject.transform.lossyScale, boundObject.transform.localScale) / 2;

            Collider[] colliders = Physics.OverlapBox(
                worldPosition,
                halfExtents,
                rotation
            );
            colliders = Physics.OverlapBox(
                worldPosition,
                boundObject.transform.lossyScale / 2,
                rotation
            );
            foreach (Collider col in colliders)
            {
                if (col.transform.root == transform)
                    return true; // overlaps existing room = bad
            }
        }

        return false; 
    }
    private DoorScript FindMatchingDoor(RoomScript room, string originalDoorName)
    {
        return room.doorPoints.FirstOrDefault(d => d.name == originalDoorName);
    }
    private void ConnectDoors(DoorScript doorA, DoorScript doorB)
    {
        Debug.Log($"Connected door {doorA.name} to {doorB.name} in room {doorB.transform.parent.parent.name}");
        connectedDoors.Add(doorA);
        connectedDoors.Add(doorB);
    }
    private void ConnectDoors(DoorScript doorA, DoorScript doorB, RoomScript roomB)
    {
        Debug.Log($"Connected door {doorA.name} to {doorB.name} in room {roomB.name}");
        connectedDoors.Add(doorA);
        connectedDoors.Add(doorB);
    }
    /*
    private void CheckForAdditionalConnections(RoomScript newRoom)
    {
        foreach (DoorScript newRoomDoor in newRoom.doorPoints)
        {
            if (connectedDoors.Contains(newRoomDoor)) continue;

            foreach (RoomScript existingRoom in spawnedRooms)
            {
                if (existingRoom == newRoom) continue;

                foreach (DoorScript existingDoor in existingRoom.doorPoints)
                {
                    if (connectedDoors.Contains(existingDoor)) continue;

                    // Check if doors are close enough and facing opposite directions
                    float distance = Vector3.Distance(newRoomDoor.transform.position, existingDoor.transform.position);
                    float angle = Vector3.Angle(newRoomDoor.transform.forward, -existingDoor.transform.forward);

                    if (distance < 0.5f && angle < 30f)
                    {
                        ConnectDoors(existingDoor, newRoomDoor, newRoom);
                        availableDoors.Remove(existingDoor);
                        availableDoors.Remove(newRoomDoor);
                    }
                }
            }
        }
    }
    */
    public void ClearDungeon()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        spawnedRooms.Clear();
        availableDoors.Clear();
        connectedDoors.Clear();
        roomSpawnCounts.Clear();
    }
}
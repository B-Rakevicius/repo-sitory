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
    public LayerMask roomCollisionLayer;

    private List<RoomScript> spawnedRooms = new List<RoomScript>();
    private List<DoorScript> availableDoors = new List<DoorScript>();
    private List<DoorScript> connectedDoors = new List<DoorScript>();

    public GameObject doorPrefab, wallPrefab;
    private List<GameObject> spawnedDoors = new List<GameObject>();

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
            if (spawnedRooms.Count > roomCount / 2)
            {
                if (!TrySpawnRoom3())
                {
                    break;
                }
            }
            else
            {
                if (!TrySpawnRoom4())
                {
                    break;
                }
            }
        }
        SealUnusedDoors();
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
                                Debug.Log("buhblunt " + newDoor.name);
                            }

                            return true;
                        }
                    }
                }
            }
            //availableDoors.Remove(targetDoor);
        }
        return false;
    }
    private bool TrySpawnRoom4()
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
                RoomScript roomScript = rule.room.GetComponent<RoomScript>();
                if (roomScript == null || roomScript.doorPoints.Count <= 1) continue; // != 1door

                int doorCount = roomScript.doorPoints.Count;
                for (int i = 0; i < rule.roomWeight * doorCount; i++)
                    weightedRooms.Add(rule.room);
            }
            if (weightedRooms.Count == 0)
                return false;
            foreach (GameObject roomPrefab in weightedRooms.OrderBy(x => rng.NextDouble()))
            {
                RoomScript prefabScript = roomPrefab.GetComponent<RoomScript>();
                if (prefabScript == null || prefabScript.doorPoints.Count <= 1) continue;

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

                                Debug.Log("buhblunt " + newDoor.name);
                            }
                            return true;
                        }
                    }
                }
            }
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
        float doorWidth = doorA.transform.localScale.x;
        Vector3 spawnPosition = doorA.transform.position + (doorA.transform.right * (doorWidth * -0.5f) + (new Vector3(0,1.5f,0)));
        GameObject newDoor = Instantiate(
            doorPrefab,
            spawnPosition,
            doorA.transform.rotation,
            doorA.transform.parent
        );
        //newDoor.transform.SetParent(doorA.transform.parent);    
        doorA.gameObject.SetActive(false);
        doorB.gameObject.SetActive(false);
        spawnedDoors.Add(newDoor);
    }
    private void ConnectDoors(DoorScript doorA, DoorScript doorB, RoomScript roomB)
    {
        Debug.Log($"Connected door {doorA.name} to {doorB.name} in room {roomB.name}");
        connectedDoors.Add(doorA);
        connectedDoors.Add(doorB);
    }
    private void SealUnusedDoors()
    {
        foreach (DoorScript door in availableDoors)
        {
            if (wallPrefab != null && door != null)
            {

                GameObject wall = Instantiate(wallPrefab, door.transform.position, door.transform.rotation);
                door.doorway.SetActive(false);
                door.gameObject.SetActive(false);
            }
        }   
        availableDoors.Clear();
    }
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
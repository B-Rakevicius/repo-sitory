using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DungeonGenerator1 : MonoBehaviour
{
    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }
    public enum RoomSpawnType
    {
        Filler,
        MustSpawnOnce,
        MustSpawnAtLeastN
    }
    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public RoomSpawnType spawnType;
        public int minimumSpawns = 1;
        public int maximumSpawns = -1; // -1 = no limit
    }
    public GameObject startingRoomPrefab;
    public GameObject emptyRoomPrefab;
    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public Vector2 offset;
    public int seed = -1;
    public bool useRandomSeed = true;
    [Range(0f, 1f)]
    public float emptyRoomChance = 0.5f;
    List<Cell> board;
    private Dictionary<Rule, int> spawnCounts;
    void Start()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);
        Debug.Log("Using seed: " + seed);
        spawnCounts = new Dictionary<Rule, int>();
        foreach (var rule in rooms)
            spawnCounts[rule] = 0;
        SparseMazeGenerator(gapChance: emptyRoomChance, maxRooms: (int)(size.x * size.y));
        LootManager.Instance.SpawnLoot(1000);
    }
    void GenerateDungeon()
    {
        List<Vector2Int> visitedCells = new();
        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                visitedCells.Add(new Vector2Int(i, j));
        for (int i = 0; i < visitedCells.Count; i++)
        {
            var temp = visitedCells[i];
            int rand = Random.Range(i, visitedCells.Count);
            visitedCells[i] = visitedCells[rand];
            visitedCells[rand] = temp;
        }
        foreach (var pos in visitedCells)
        {
            int i = pos.x;
            int j = pos.y;
            int index = i + j * size.x;
            if (!board[index].visited)
            {
                var emptyRoom = Instantiate(emptyRoomPrefab, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour1>();
                emptyRoom.UpdateRoom(new bool[4]); // No doors
                emptyRoom.name = i + "-" + j + " EmptyRoom " ;
                continue;
            }
            if (index == startPos)
            {
                var startRoom = Instantiate(startingRoomPrefab, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour1>();
                startRoom.UpdateRoom(board[index].status);
                startRoom.name = i + "-" + j + " StartRoom ";
                continue;
            }
            Rule selectedRule = SelectRoomRule();
            spawnCounts[selectedRule]++;
            var newRoom = Instantiate(selectedRule.room, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour1>();
            newRoom.UpdateRoom(board[index].status);
            newRoom.name = i + "-" + j + " " + newRoom.name;
        }
    }
    Rule SelectRoomRule()
    {
        foreach (var rule in rooms)
        {
            int count = spawnCounts[rule];
            if (rule.spawnType == RoomSpawnType.MustSpawnOnce)
            {
                if (count == 0)
                    return rule;
                else
                    continue;
            }
            if (rule.spawnType == RoomSpawnType.MustSpawnAtLeastN)
            {
                if (count < rule.minimumSpawns)
                {
                    if (rule.maximumSpawns == -1 || count < rule.maximumSpawns)
                        return rule;
                    else
                        continue;
                }
            }
        }
        List<Rule> validOptions = new();
        foreach (var rule in rooms)
        {
            int count = spawnCounts[rule];
            bool underMax = (rule.maximumSpawns == -1 || count < rule.maximumSpawns);

            if (underMax)
                validOptions.Add(rule);
        }
        if (validOptions.Count == 0)
        {
            Debug.LogWarning("No valid rooms left to spawn.");
            return null;
        }

        return validOptions[Random.Range(0, validOptions.Count)];
    }
    void MazeGenerator()
    {
        board = new List<Cell>();
        for (int i = 0; i < size.x * size.y; i++)
        {
            board.Add(new Cell());
        }
        int currentCell = startPos;
        Stack<int> path = new Stack<int>();
        int k = 0;
        while (k < 1000)
        {
            k++;
            board[currentCell].visited = true;
            if (currentCell == board.Count - 1)
                break;
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0) break;
                currentCell = path.Pop();
            }
            else
            {
                path.Push(currentCell);
                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell)
                {
                    if (newCell - 1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    if (newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }
            }
        }

        GenerateDungeon();
    }
    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        if (cell - size.x >= 0 && !board[cell - size.x].visited)
            neighbors.Add(cell - size.x); 
        if (cell + size.x < board.Count && !board[cell + size.x].visited)
            neighbors.Add(cell + size.x);
        if ((cell + 1) % size.x != 0 && !board[cell + 1].visited)
            neighbors.Add(cell + 1);
        if (cell % size.x != 0 && !board[cell - 1].visited)
            neighbors.Add(cell - 1);
        return neighbors;
    }
    void SparseMazeGenerator(float gapChance = 0.4f, int maxRooms = 20)
    {
        board = new List<Cell>();
        for (int i = 0; i < size.x * size.y; i++)
        {
            board.Add(new Cell());
        }
        int visitedCount = 1;
        int currentCell = startPos;
        board[currentCell].visited = true;
        Stack<int> path = new Stack<int>();
        path.Push(currentCell);
        int iterations = 0;
        int maxIterations = 500;
        while (path.Count > 0 && visitedCount < maxRooms && iterations < maxIterations)
        {
            iterations++;
            currentCell = path.Peek();
            List<int> neighbors = CheckNeighbors(currentCell);
            bool createGap = visitedCount >= (size.x * size.y / 2) && Random.value < gapChance;
            if (neighbors.Count == 0 || createGap)
            {
                path.Pop();
                continue;
            }

            int newCell = neighbors[Random.Range(0, neighbors.Count)];
            ConnectCells(currentCell, newCell);

            board[newCell].visited = true;
            path.Push(newCell);
            visitedCount++;
        }
        EnsureConnectivity();
        GenerateDungeon();
    }
    void ConnectCells(int currentCell, int newCell)
    {
        if (newCell > currentCell)
        {
            if (newCell - 1 == currentCell)
            {
                board[currentCell].status[2] = true;
                board[newCell].status[3] = true;
            }
            else
            {
                board[currentCell].status[1] = true;
                board[newCell].status[0] = true;
            }
        }
        else
        {
            if (newCell + 1 == currentCell)
            {
                board[currentCell].status[3] = true;
                board[newCell].status[2] = true;
            }
            else
            {
                board[currentCell].status[0] = true;
                board[newCell].status[1] = true;
            }
        }
    }
    void EnsureConnectivity()
    {
        var parent = new int[board.Count];
        for (int i = 0; i < parent.Length; i++) parent[i] = i;
        int Find(int x)
        {
            if (parent[x] != x) parent[x] = Find(parent[x]);
            return parent[x];
        }
        void Union(int a, int b)
        {
            int rootA = Find(a);
            int rootB = Find(b);
            if (rootA != rootB) parent[rootB] = rootA;
        }
        for (int i = 0; i < board.Count; i++)
        {
            if (!board[i].visited) continue;
            for (int dir = 0; dir < 4; dir++)
            {
                if (!board[i].status[dir]) continue;

                int nx = i % size.x;
                int ny = i / size.x;
                int neighborIndex = -1;
                switch (dir)
                {
                    case 0: neighborIndex = (nx - 1) + ny * size.x; break; // left
                    case 1: neighborIndex = (nx + 1) + ny * size.x; break; // right
                    case 2: neighborIndex = nx + (ny - 1) * size.x; break; // up
                    case 3: neighborIndex = nx + (ny + 1) * size.x; break; // down
                }
                if (neighborIndex >= 0 && neighborIndex < board.Count && board[neighborIndex].visited)
                {
                    Union(i, neighborIndex);
                }
            }
        }
        var components = new Dictionary<int, List<int>>();
        for (int i = 0; i < board.Count; i++)
        {
            if (!board[i].visited) continue;
            int root = Find(i);
            if (!components.ContainsKey(root))
                components[root] = new List<int>();
            components[root].Add(i);
        }
        if (components.Count <= 1)
            return;
        var componentRoots = new List<int>(components.Keys);
        for (int i = 1; i < componentRoots.Count; i++)
        {
            int rootA = componentRoots[0];
            int rootB = componentRoots[i];
            int bestA = -1, bestB = -1;
            float bestDist = float.MaxValue;
            foreach (var cellA in components[rootA])
            {
                int ax = cellA % size.x;
                int ay = cellA / size.x;
                foreach (var cellB in components[rootB])
                {
                    int bx = cellB % size.x;
                    int by = cellB / size.x;
                    float dist = Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestA = cellA;
                        bestB = cellB;
                    }
                }
            }
            if (bestA != -1 && bestB != -1)
            {
                ConnectCells(bestA, bestB);
                Union(bestA, bestB);
                components[rootA].AddRange(components[rootB]);
                components.Remove(rootB);
                componentRoots.RemoveAt(i);
                i--;
            }
        }
        List<int> deadEnds = GetDeadEnds();
        foreach (int index in deadEnds)
        {
            Debug.Log("Dead end at: " + (index % size.x) + ", " + (index / size.x));
        }
    }
    List<int> GetDeadEnds()
    {
        List<int> deadEnds = new List<int>();
        for (int i = 0; i < board.Count; i++)
        {
            if (!board[i].visited) continue;

            int connections = 0;
            foreach (bool b in board[i].status)
                if (b) connections++;

            if (connections == 1)
                deadEnds.Add(i);
        }
        return deadEnds;
    }
}

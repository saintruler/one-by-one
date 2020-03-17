﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabirintBuilder : MonoBehaviour
{
    Labirint labirint;
    public int numberOfRooms = 10;
    public int correctPathLength = 3;
    public GameObject[] combatRoomPrefabs;
    public GameObject[] peacefulRoomPrefabs;
    public string exitSceneName = "";
    public GameObject[] containersPrefabs;

    Dictionary<int, Vector2Int> allRoomsPositions;
    List<Vector2Int> correctPathRoomsPositions;
    private int[,] map;
    Vector2Int startPosition;
    Vector2Int endPosition;
    Vector2Int currentPosition;
    int lastRoomID;    

    private void Init()
    {
        map = new int[2*numberOfRooms, 2*numberOfRooms];
        for (int i = 0; i < 2 * numberOfRooms; i++)
            for (int j = 0; j < 2 * numberOfRooms; j++) {
                map[i, j] = -1;
            }
    }

    public void BuildLabirint(Labirint labirintScript) {
        labirint = labirintScript;
        Init();
        labirint.blueprints = new RoomBlueprint[numberOfRooms+1];
        labirint.InitBlueprintsFromBuilder();
        startPosition = new Vector2Int(numberOfRooms, numberOfRooms); // середина
        currentPosition = startPosition;
        allRoomsPositions = new Dictionary<int, Vector2Int>();
        allRoomsPositions.Add(0,startPosition);
        correctPathRoomsPositions = new List<Vector2Int>();
        correctPathRoomsPositions.Add(startPosition);
        map[startPosition.x, startPosition.y] = 0;
        lastRoomID = 0;

        MakeCorrectPath();
        MakeDeadEnds();
        //DrawMap(); 
        FillRoomPrefabs();
        FillContainers();
    }

    void MakeCorrectPath() {
        Vector2Int newPosition;
        Vector2Int positionToMove;
        List<Direction.Side> availableSides;
        while (lastRoomID < correctPathLength)
        {
            availableSides = new List<Direction.Side>();
            foreach (Direction.Side side in Direction.sides)
            {
                positionToMove = currentPosition + Direction.SideToVector2Int(side);
                if (map[positionToMove.x, positionToMove.y] == -1)
                    availableSides.Add(side);
            }
            if (availableSides.Count == 0) // if dead end
            {
                correctPathRoomsPositions.Remove(currentPosition);
                StepBack();
            }
            else
            {
                Direction.Side stepDirrection = availableSides.ToArray()[Random.Range(0, availableSides.Count)]; // random available side
                newPosition = currentPosition + Direction.SideToVector2Int(stepDirrection);
                lastRoomID++;
                map[newPosition.x, newPosition.y] = lastRoomID;
                ConnectRoomBlueprints(map[currentPosition.x, currentPosition.y], map[newPosition.x, newPosition.y], stepDirrection);
                allRoomsPositions.Add(lastRoomID, newPosition);
                correctPathRoomsPositions.Add(newPosition);
                currentPosition = newPosition;
            }
        }
        endPosition = currentPosition;
    }

    void StepBack()
    {
        Vector2Int positionToMove = currentPosition;
        int indexMax = -1;
        foreach (Direction.Side side in Direction.sides)
        {
            Vector2Int newPosition = currentPosition + Direction.SideToVector2Int(side);
            if ((map[newPosition.x, newPosition.y] > indexMax) && correctPathRoomsPositions.Contains(newPosition))
            {
                indexMax = map[newPosition.x, newPosition.y];
                positionToMove = newPosition;
            }
        }
        currentPosition = positionToMove;
    }

    void MakeDeadEnds() {
        int randomRoomID;
        int backupExit = 0; // это костыль. Есть не нулевая вероятность что лабиринт будет продолжать генериться неограниченно долго.
                            // В таком случае лучше недоделанный лабиринт чем зависший цикл
        List<Direction.Side> availableSides;
        Vector2Int newPosition;
        Vector2Int positionToMove;
        while (lastRoomID < numberOfRooms && backupExit < 1000)
        {
            backupExit++;
            randomRoomID = Random.Range(0, allRoomsPositions.Count);
            if (allRoomsPositions[randomRoomID] != endPosition) // end room must not be a crossroad, only correct way enter and exit to another scene
            {
                availableSides = new List<Direction.Side>();
                foreach (Direction.Side side in Direction.sides)
                {
                    positionToMove = allRoomsPositions[randomRoomID] + Direction.SideToVector2Int(side);
                    if (map[positionToMove.x, positionToMove.y] == -1)
                        availableSides.Add(side);
                }

                if (availableSides.Count != 0)
                {
                    Direction.Side randomSide = availableSides.ToArray()[Random.Range(0, availableSides.Count)];
                    newPosition = allRoomsPositions[randomRoomID] + Direction.SideToVector2Int(randomSide);
                    if (map[newPosition.x, newPosition.y] == -1)
                    {
                        lastRoomID++;
                        map[newPosition.x, newPosition.y] = lastRoomID;
                        ConnectRoomBlueprints(randomRoomID, lastRoomID, randomSide);
                        allRoomsPositions.Add(lastRoomID, newPosition);
                    }
                }
            }
        }
    }
    
    void DrawMap() {// for debug only
        Color lineColor;
        for (int i = 0; i < allRoomsPositions.Count; i++) {
            if (correctPathRoomsPositions.Contains(allRoomsPositions[i]))
                lineColor = Color.green;
            else
                lineColor = Color.red;
            foreach (Direction.Side side in Direction.sides)
            {
                if (Labirint.instance.blueprints[i].rooms.ContainsKey(side))
                    if (Labirint.instance.blueprints[i].rooms[side] != -1)
                    {
                        Debug.DrawRay(new Vector3(allRoomsPositions[i].x, allRoomsPositions[i].y, 0), Direction.SideToVector3(side), lineColor,9999f);
                    }
            }
        }
    }

    void ConnectRoomBlueprints(int firstID, int secondID, Direction.Side direction1to2) {
        Labirint.instance.blueprints[firstID].rooms[direction1to2] = secondID;
        Labirint.instance.blueprints[secondID].rooms[Direction.InvertSide(direction1to2)] = firstID;
    }

    void FillRoomPrefabs() {
        const bool norepeat = true; // hardcode flag for testing of big labirint, while we dont have many room prefabs
        List<GameObject> emptyRoomsList = new List<GameObject>(peacefulRoomPrefabs);
        List<GameObject> combatRoomsList = new List<GameObject>(combatRoomPrefabs);
        labirint.blueprints[0].prefab = RandomGameObjectFromList(emptyRoomsList); // 0 index is for starting room, always empty
        if (norepeat) emptyRoomsList.Remove(labirint.blueprints[0].prefab);
        for (int i = 1; i <= numberOfRooms; i++)
        {
            if (allRoomsPositions[i] != endPosition)
            {
                labirint.blueprints[i].prefab = RandomGameObjectFromList(combatRoomsList);
                if (norepeat) combatRoomsList.Remove(labirint.blueprints[i].prefab);
            }
            else
            {
                labirint.blueprints[i].prefab = RandomGameObjectFromList(emptyRoomsList);
                if (norepeat) emptyRoomsList.Remove(labirint.blueprints[i].prefab);
                labirint.blueprints[i].exitSceneName = exitSceneName;
            }
        }
        labirint.blueprints[map[endPosition.x, endPosition.y]].exitSceneName = exitSceneName;
    }

    void FillContainers() {
        List<int> containerAvailableRooms = new List<int>(allRoomsPositions.Keys);
        containerAvailableRooms.Remove(0);                                  // no containers in first room
        containerAvailableRooms.Remove(map[endPosition.x, endPosition.y]);  // and last room

        if (containerAvailableRooms.Count < containersPrefabs.Length)
            Debug.LogError("not enough rooms for containtes");
        else
            foreach (GameObject containerPrefab in containersPrefabs)
            {
                int roomForContainerID = containerAvailableRooms.ToArray()[Random.Range(0, containerAvailableRooms.ToArray().Length - 1)];
                labirint.blueprints[roomForContainerID].contanerPrefab = containerPrefab;
                containerAvailableRooms.Remove(roomForContainerID);
            }
    }

    GameObject RandomGameObjectFromList(List<GameObject> prefabList) {
        if (prefabList.Count == 0)
        {
            Debug.LogError("Not enough room prefabs to fill rooms to labirintBuilder");
            return null;
        }
        GameObject[] array = prefabList.ToArray();
        return array[Random.Range(0, array.Length)]; 
    }
}

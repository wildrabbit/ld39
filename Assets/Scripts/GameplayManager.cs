using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [Header("Dependencies")]
    public TimeManager timeManager;
    public GeneratorManager generatorManager;
    public CharacterManager characterManager;
    public NodeManager nodeManager;
    public RoomManager roomManager;
}

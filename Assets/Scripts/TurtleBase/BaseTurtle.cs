using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseTurtle", menuName = "Turtle/BaseTurtle")]
public class BaseTurtle : ScriptableObject {
    public int health = 3;                          // 3-5
    public int moveSpeed = 5;                       // 5-12
    public int rotateSpeed = 100;
    public int jumpForce = 200;                     // 200-400
    public Weapon weapon = null;                    // Lazor / Rocket
    public AltWeapon altWeapon = null;              // Bomb / Mine
    public GameObject prefab;
}

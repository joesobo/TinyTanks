using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Turtle/Weapon")]
public class Weapon : ScriptableObject {
    public float reloadTime = 0.5f;
    public float timeBetweenShots = 0.1f;
    public int directions = 1;                              // 1-8
    public Ammo ammo = null;
    public BulletType type;
    public Sprite icon;

    public enum BulletType {
        Lazor,
        Rocket
    }

    public void UseAmmo(int amount) {
        ammo.currentClip -= amount;
        if(ammo.currentClip < 0) {
            ammo.currentClip = 0;
        }
    }

    public void Reload() {
        ammo.currentClip = ammo.clipSize;
    }

    public void Shoot(Vector3 position, Quaternion rotation, Transform parent) {
        ammo.StartUpBullet(Instantiate(ammo.prefab, position, rotation, parent), type);
        UseAmmo(1);
    }
}
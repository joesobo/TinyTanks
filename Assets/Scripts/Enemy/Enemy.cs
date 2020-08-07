﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public BaseTurtle BaseTurtle;

    private EnemyHealth enemyHealth;
    private SmartEnemy smartEnemy;

    void Start() {
        GameObject turtle = Instantiate(BaseTurtle.prefab, this.transform.position, Quaternion.identity);

        enemyHealth = turtle.GetComponent<EnemyHealth>();
        enemyHealth.MAXHEALTH = BaseTurtle.health;

        smartEnemy = turtle.GetComponent<SmartEnemy>();
        smartEnemy.speed = BaseTurtle.moveSpeed;
        smartEnemy.rotateSpeed = BaseTurtle.rotateSpeed;
        smartEnemy.jumpForce = BaseTurtle.jumpForce;
        smartEnemy.weapon = BaseTurtle.weapon;

        BaseTurtle.weapon.reload();
    }
}

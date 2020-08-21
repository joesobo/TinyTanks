using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health {
    protected override void OnDeath() {
        if (settings.useParticle) {
            Instantiate(deathParticles, transform.position, transform.rotation);
        }
        FindObjectOfType<LevelRunner>().DecreaseNumEnemy();
        Destroy(gameObject);
    }
}

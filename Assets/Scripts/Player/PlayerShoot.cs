﻿using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour {
    [HideInInspector]
    public Weapon weapon;
    [HideInInspector]
    public Weapon newWeapon = null;
    [HideInInspector]
    private Weapon tempWeapon = null;
    [HideInInspector]
    public bool useNewWeapon = false;
    private int waitForSecondsWeapon = 5;

    [HideInInspector]
    public AltWeapon altWeapon;
    [HideInInspector]
    public AltWeapon newAltWeapon = null;
    [HideInInspector]
    private AltWeapon tempAltWeapon = null;
    [HideInInspector]
    public bool useNewAlt = false;
    private int waitForSecondsAlt = 5;

    public Transform shootPoint;
    public Transform dropPoint;
    public Transform parent;
    public Transform altParent;
    private bool mainDelayOn = false;
    private bool altDelayOn = false;

    private GameSettings settings;
    private CameraShake screenShake;

    private PlayerSoundManager soundManager;

    private void Start() {
        settings = FindObjectOfType<GameSettings>();
        screenShake = FindObjectOfType<CameraShake>();

        soundManager = GetComponent<PlayerSoundManager>();

        if (altWeapon) {
            altWeapon.inPlay = 0;
        }
    }

    private void Update() {
        CheckSwitchWeapons();

        if (!settings.isPaused) {
            MainShoot();
            AltShoot();
        }
    }

    private void AltShoot() {
        if (altWeapon && Input.GetButtonDown("Fire2") && !altDelayOn && altWeapon.inPlay < altWeapon.maxInPlay) {
            //start delay for next shot
            altDelayOn = true;
            StartCoroutine("AltDelayCo");
            //create bullet
            altWeapon.Shoot(dropPoint.position, this.transform.rotation, altParent);
            altWeapon.inPlay++;
            //play sound
            soundManager.Play(PlayerSoundManager.Clip.shootAlt);
            //apply screen shake
            StartCoroutine(screenShake.Shake());
        }
    }

    private void MainShoot() {
        if (weapon && Input.GetButtonDown("Fire1") && !mainDelayOn) {
            //start delay for next shot
            mainDelayOn = true;
            StartCoroutine("MainDelayCo");
            //create bullet
            weapon.Shoot(shootPoint.position, this.transform.rotation, parent);
            //play sound
            soundManager.Play(PlayerSoundManager.Clip.shootMain);
            //apply screen shake
            StartCoroutine(screenShake.Shake());
        }
    }

    private void CheckSwitchWeapons() {
        if (useNewWeapon) {
            useNewWeapon = false;
            StartCoroutine("UseNewWeapon");
        }

        if (useNewAlt) {
            useNewAlt = false;
            StartCoroutine("UseNewAltWeapon");
        }
    }

    //Reload and delay shooting
    IEnumerator MainDelayCo() {
        if (weapon.ammo.currentClip - 1 > 0) {
            yield return new WaitForSeconds(weapon.timeBetweenShots);
        }
        else {
            yield return new WaitForSeconds(weapon.reloadTime);
            weapon.Reload();
        }
        mainDelayOn = false;
    }

    //Delay shooting
    IEnumerator AltDelayCo() {
        yield return new WaitForSeconds(altWeapon.timeBetweenUses);
        altDelayOn = false;
    }

    IEnumerator UseNewWeapon() {
        tempWeapon = weapon;
        weapon = newWeapon;
        yield return new WaitForSeconds(waitForSecondsWeapon);
        weapon = tempWeapon;
        newWeapon = null;
        tempWeapon = null;
    }

    IEnumerator UseNewAltWeapon() {
        tempAltWeapon = altWeapon;
        altWeapon = newAltWeapon;
        yield return new WaitForSeconds(waitForSecondsAlt);
        altWeapon = tempAltWeapon;
        newAltWeapon = null;
        tempAltWeapon = null;
    }
}

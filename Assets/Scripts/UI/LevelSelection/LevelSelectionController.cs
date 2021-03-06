﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionController : MonoBehaviour {
    public List<GameObject> selectors = new List<GameObject>();
    public Text prevText;
    public Text nextText;

    private LevelSingleton ls;
    private GameObject activeSelectorRef;
    private LevelLoader levelLoader;
    private GameSettings settings;

    private Color deactivatedColor = new Color(0.5f, 0.5f, 0.5f, 1);
    private Color white = new Color(1, 1, 1, 1);

    private void OnEnable() {
        ls = FindObjectOfType<LevelSingleton>();
        settings = FindObjectOfType<GameSettings>();
        levelLoader = GetComponent<LevelLoader>();
        activeSelectorRef = selectors[ls.activeLevel - 1];
        activeSelectorRef.SetActive(true);

        UpdateSelectors();
        UpdateButtons();
    }

    public void UnlockNextLevel() {
        ls.UnlockLevel();
        UpdateButtons();
    }

    public void Next() {
        if (ls.activeLevel < ls.unlockedLevels) {
            ls.activeLevel++;

            UpdateSelectors();
            UpdateButtons();
        }
    }

    public void Previous() {
        if (ls.activeLevel > 1) {
            ls.activeLevel--;

            UpdateSelectors();
            UpdateButtons();
        }
    }

    public void Play() {
        settings.isPaused = false;
        levelLoader.LoadLevel(ls.activeLevel);
    }

    private void UpdateSelectors() {
        activeSelectorRef.SetActive(false);
        activeSelectorRef = selectors[ls.activeLevel - 1];
        activeSelectorRef.SetActive(true);
    }

    private void UpdateButtons() {
        nextText.color = ls.activeLevel < ls.unlockedLevels ? white : deactivatedColor;
        prevText.color = ls.activeLevel > 1 ? white : deactivatedColor;
    }

    public void ResetLevels() {
        ls.ResetLevels();
        UpdateSelectors();
        UpdateButtons();
    }
}

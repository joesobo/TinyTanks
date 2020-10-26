﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitLoader : MonoBehaviour {
    public void LoadQuit() {
        this.gameObject.SetActive(true);
    }

    public void UnloadQuit() {
        this.gameObject.SetActive(false);
    }

    public void Quit() {
        Application.Quit();
    }
}

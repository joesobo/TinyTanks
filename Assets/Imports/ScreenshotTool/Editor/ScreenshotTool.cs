﻿#define CINEMACHINE
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Staggart Creations http://staggart.xyz
// Copyright protected under Unity asset store EULA

public sealed class ScreenshotTool : EditorWindow
{
    private static bool captureScene = false;
    private static bool captureGame = true;

    private Camera sourceCamera;
    private List<Camera> cameras = new List<Camera>();
    private string[] cameraNames;
    private int cameraID;

    private int ssWidth = 1920;
    private int ssHeight = 1080;

    private static string[] reslist = new string[] { "720p", "1080p", "1140p", "4K", "8K", "Custom..." };
    private const float FLOAT_WIDE_MULTIPLIER = 1.3215f;
    public static int Resolution
    {
        get { return EditorPrefs.GetInt(PlayerSettings.productName + "_SRCSHOT_RESOLUTION", 1); }
        set { EditorPrefs.SetInt(PlayerSettings.productName + "_SRCSHOT_RESOLUTION", value); }
    }

    public bool wideScreen = false;
    public static bool OpenAfterCapture
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SRCSHOT_OpenAfterCapture", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SRCSHOT_OpenAfterCapture", value); }
    }

    public static string SavePath
    {
        get { return EditorPrefs.GetString(PlayerSettings.productName + "_SRCSHOT_DIR", Application.dataPath.Replace("Assets", "Screenshots/")); }
        set { EditorPrefs.SetString(PlayerSettings.productName + "_SRCSHOT_DIR", value); }
    }

    public static string FileNameFormat
    {
        get { return EditorPrefs.GetString(PlayerSettings.productName + "_SRCSHOT_FileNameFormat", "{S}_{R}_{D}_{T}"); }
        set { EditorPrefs.SetString(PlayerSettings.productName + "_SRCSHOT_FileNameFormat", value); }
    }

    public static string[] DateFormats = new string[] { "MM-dd-yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };
    public static int DateFormat
    {
        get { return EditorPrefs.GetInt(PlayerSettings.productName + "_SRCSHOT_DateFormat", 1); }
        set { EditorPrefs.SetInt(PlayerSettings.productName + "_SRCSHOT_DateFormat", value); }
    }

    public static bool UseCompression
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SRCSHOT_CMPRS", false); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SRCSHOT_CMPRS", value); }
    }

    GUIStyle pathField;

    // Check if folder exists, otherwise create it
    public static string CheckFolderValidity(string targetFolder)
    {
        //Create folder if it doesn't exist
        if (!Directory.Exists(targetFolder))
        {
            Debug.Log("Directory <i>\"" + targetFolder + "\"</i> didn't exist and was created...");
            Directory.CreateDirectory(targetFolder);

            AssetDatabase.Refresh();
        }

        return targetFolder;
    }

    private static string GetSceneName()
    {
        string name;
        //Screenshot name prefix
        if (SceneManager.sceneCount > 0)
            name = SceneManager.GetActiveScene().name;
        else
            //If there are no scenes in the build, or scene is unsaved
            name = PlayerSettings.productName;

        if (name == string.Empty) name = "Screenshot";

        return name;
    }

    public static string FormatFileName(int resIndex, string dateFormat)
    {
        string fileName = FileNameFormat.Replace("{S}", GetSceneName());
        fileName = fileName.Replace("{P}", PlayerSettings.productName);
        fileName = fileName.Replace("{R}", reslist[resIndex]);
        fileName = fileName.Replace("{D}", System.DateTime.Now.ToString(dateFormat));
        fileName = fileName.Replace("{T}", System.DateTime.Now.ToString("HH-mm-ss"));
        fileName = fileName.Replace("{U}", System.DateTime.Now.Ticks.ToString());

        return fileName;
    }

    [MenuItem("Tools/Screenshot")]
    public static void Init()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow ssWindow = EditorWindow.GetWindow(typeof(ScreenshotTool), false);

        //Options
        ssWindow.autoRepaintOnSceneChange = true;
        ssWindow.maxSize = new Vector2(250f, 275f);
        ssWindow.minSize = ssWindow.maxSize;
        ssWindow.titleContent.image = EditorGUIUtility.IconContent("camera gizmo.png").image;
        ssWindow.titleContent.text = "Screenshot";

        //Show
        ssWindow.Show();

    }

    private void OnFocus()
    {
        RefreshCameras();
    }

    private void OnEnable()
    {
        RefreshCameras();

        if (focusedWindow != null && focusedWindow.GetType() == typeof(SceneView))
        {
            captureScene = true;
            captureGame = false;
        }
        else
        {
            captureGame = true;
            captureScene = false;
        }
    }

    private void RefreshCameras()
    {
        Camera[] sceneCameras = GameObject.FindObjectsOfType<Camera>();
        cameras.Clear();

        foreach (Camera cam in sceneCameras)
        {
            //Try to exclude any off-screen camera's
            if (cam.activeTexture != null && cam.hideFlags != HideFlags.None && !cam.enabled) continue;

            cameras.Add(cam);
        }

        //Compose list of names
        cameraNames = new string[cameras.Count];
        for (int i = 0; i < cameraNames.Length; i++)
        {
            cameraNames[i] = cameras[i].name;
        }
    }

    void SetResolution()
    {
        switch (Resolution)
        {
            case 0:
                ssWidth = 1280;
                ssHeight = 720;
                break;
            case 1:
                ssWidth = 1920;
                ssHeight = 1080;
                break;
            case 2:
                ssWidth = 2560;
                ssHeight = 1440;
                break;
            case 3:
                ssWidth = 3840;
                ssHeight = 2160;
                break;
            case 4:
                ssWidth = 7680;
                ssHeight = 4320;
                break;
        }

        if (wideScreen && Resolution != reslist.Length - 1) ssWidth = Mathf.RoundToInt(ssWidth * FLOAT_WIDE_MULTIPLIER);
    }

    void OnGUI()
    {
        var e = Event.current;

        if (cameras.Count == 0)
        {
            EditorGUILayout.HelpBox("No active camera's in the scene", MessageType.Warning);
            return;
        }
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Toggle(captureScene, new GUIContent("Scene", EditorGUIUtility.IconContent("d_unityeditor.sceneview.png").image), EditorStyles.miniButtonLeft))
                {
                    captureScene = true;
                    captureGame = false;
                }
                if (GUILayout.Toggle(captureGame, new GUIContent("Game", EditorGUIUtility.IconContent("unityeditor.gameview.png").image), EditorStyles.miniButtonRight))
                {
                    captureGame = true;
                    captureScene = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (cameras.Count > 1)
            {
                EditorGUILayout.BeginHorizontal();
                cameraID = EditorGUILayout.Popup(cameraID, cameraNames, GUILayout.MaxWidth(100f));
                sourceCamera = cameras[cameraID];

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                sourceCamera = cameras[0];
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Resolution (" + ssWidth + " x " + ssHeight + ")", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            Resolution = EditorGUILayout.Popup(Resolution, reslist, GUILayout.MinWidth(75f), GUILayout.MaxWidth(75f));
            if (Resolution != reslist.Length - 1)
            {
                wideScreen = EditorGUILayout.ToggleLeft("Widescreen (21:9)", wideScreen);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                ssWidth = EditorGUILayout.IntField(ssWidth, GUILayout.MaxWidth(45f));
                EditorGUILayout.LabelField("x", GUILayout.MaxWidth(15f));
                ssHeight = EditorGUILayout.IntField(ssHeight, GUILayout.MaxWidth(45f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();

            //Update resolution
            SetResolution();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            GUILayout.Label("Output folder", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(SavePath, Styles.PathField);

            if (SavePath == string.Empty) SavePath = Application.dataPath.Replace("Assets", "Screenshots/");

            if (GUILayout.Button("...", GUILayout.ExpandWidth(true)))
            {
                SavePath = EditorUtility.SaveFolderPanel("Screenshot destination folder", SavePath, Application.dataPath);
            }
            if (GUILayout.Button("Open", GUILayout.ExpandWidth(true)))
            {
                CheckFolderValidity(SavePath);
                Application.OpenURL("file://" + SavePath);
            }
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(SavePath == string.Empty);
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Capture", GUILayout.MinHeight(25)) || (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.S)))
                    {
                        RenderScreenshot();
                    }
                }
            }

            EditorGUIUtility.labelWidth = 0.1f;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.textArea))
            {
                UseCompression = EditorGUILayout.ToggleLeft("PNG", UseCompression);
                UseCompression = !EditorGUILayout.ToggleLeft("JPG", !UseCompression);
                OpenAfterCapture = EditorGUILayout.ToggleLeft("Auto open", OpenAfterCapture);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.LabelField(" - Staggart Creations - ", EditorStyles.centeredGreyMiniLabel);
    }

    public void RenderScreenshot()
    {
        if (!sourceCamera) return;

        Vector3 originalPos = sourceCamera.transform.position;
        Quaternion originalRot = sourceCamera.transform.rotation;
        float originalFOV = sourceCamera.fieldOfView;
        bool originalOrtho = sourceCamera.orthographic;
        float originalOthoSize = sourceCamera.orthographicSize;

#if CINEMACHINE
        Cinemachine.CinemachineBrain cBrain = sourceCamera.GetComponent<Cinemachine.CinemachineBrain>();
        bool cBrainEnable = false;
        if (cBrain) cBrainEnable = cBrain.enabled;
#endif

        if (captureScene)
        {
            //Set focus to scene view
#if !UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Scene");
#else
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
#endif

            if (SceneView.lastActiveSceneView)
            {
#if CINEMACHINE
                if (cBrain && cBrainEnable) cBrain.enabled = false;
#endif

                sourceCamera.fieldOfView = SceneView.lastActiveSceneView.camera.fieldOfView;
                sourceCamera.orthographic = SceneView.lastActiveSceneView.camera.orthographic;
                sourceCamera.orthographicSize = SceneView.lastActiveSceneView.camera.orthographicSize;
                sourceCamera.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                sourceCamera.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
            }
        }

        RenderTexture rt = new RenderTexture(ssWidth, ssHeight, 24);
        rt.format = RenderTextureFormat.ARGB32;
        rt.useDynamicScale = true;

        RenderTexture.active = rt;
        sourceCamera.targetTexture = rt;
        sourceCamera.Render();

        Texture2D screenShot = new Texture2D(ssWidth, ssHeight, TextureFormat.RGB24, false, true);

        EditorUtility.DisplayProgressBar("Screenshot", "Reading pixels " + 1 + "/" + 3, 1f / 3f);
        screenShot.ReadPixels(new Rect(0, 0, ssWidth, ssHeight), 0, 0);
        sourceCamera.targetTexture = null;
        RenderTexture.active = null;

        byte[] bytes;
        EditorUtility.DisplayProgressBar("Screenshot", "Encoding " + 2 + "/" + 3, 2f / 3f);
        bytes = (UseCompression) ? screenShot.EncodeToPNG() : screenShot.EncodeToJPG();

        string filename = FormatFileName(Resolution, DateFormats[DateFormat]) + (UseCompression == true ? ".png" : ".jpg");

        CheckFolderValidity(SavePath);

        EditorUtility.DisplayProgressBar("Screenshot", "Saving file " + 3 + "/" + 3, 3f / 3f);
        System.IO.File.WriteAllBytes(filename, bytes);

        if (OpenAfterCapture) Application.OpenURL(filename);

        //Restore
#if CINEMACHINE
        if (cBrain && cBrainEnable) cBrain.enabled = true;
#endif
        sourceCamera.orthographic = originalOrtho;
        sourceCamera.orthographicSize = originalOthoSize;
        sourceCamera.fieldOfView = originalFOV;
        sourceCamera.transform.position = originalPos;
        sourceCamera.transform.rotation = originalRot;

        EditorUtility.ClearProgressBar();
    }

    private class Styles
    {
        private static GUIStyle _PathField;
        public static GUIStyle PathField
        {
            get
            {
                if (_PathField == null)
                {
                    _PathField = new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleRight,
                        stretchWidth = true
                    };
                }

                return _PathField;
            }
        }
    }

#if UNITY_2019_1_OR_NEWER
    [SettingsProvider]
    public static SettingsProvider ScreenshotSettings()
    {
        var provider = new SettingsProvider("Editor/Screenshot", SettingsScope.User)
        {
            label = "Screenshot",
            guiHandler = (searchContent) =>
            {
                FileNameFormat = EditorGUILayout.TextField("File name format", FileNameFormat);
                EditorGUILayout.HelpBox("{P} = Project name\n{S} = Scene name\n{R} = Resolution\n{D} = Date\n{T} = Time\n{U} = Unique number", MessageType.None);

                EditorGUILayout.LabelField("Example: " + ScreenshotTool.FormatFileName(1, DateFormats[DateFormat]));

                ScreenshotTool.DateFormat = EditorGUILayout.Popup("Date format", DateFormat, DateFormats);
            },

            keywords = new HashSet<string>(new[] { "Screenshot" })
        };

        return provider;
    }
#else
    [PreferenceItem("Screenshot")]
    public static void ScreenshotGUI()
    {
        FileNameFormat = EditorGUILayout.TextField("File name format", FileNameFormat);
        EditorGUILayout.HelpBox("{P} = Project name\n{S} = Scene name\n{R} = Resolution\n{D} = Date\n{T} = Time\n{U} = Unique number", MessageType.None);

        EditorGUILayout.LabelField("Example: " + ScreenshotTool.FormatFileName(1, DateFormats[DateFormat]), EditorStyles.miniLabel);

        EditorGUILayout.Space();
        ScreenshotTool.DateFormat = EditorGUILayout.Popup("Date format", DateFormat, DateFormats);
    }
#endif
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.SceneManagement;
using YG;
using System.Threading.Tasks;
using Unity.VisualScripting;
//using UnityStandardAssets.ImageEffects;
/// <summary>
///  Copyright (c) 2016 Eric Zhu 
/// </summary>
namespace GreatArcStudios
{
    /// <summary>
    /// The pause menu manager. You can extend this to make your own. Everything is pretty modular, so creating you own based off of this should be easy. Thanks for downloading and good luck! 
    /// </summary>
    public class StartMenuManager : MonoBehaviour
    {
        public GameObject mainPanel;
        public GameObject levelsPanel;
        public GameObject vidPanel;
        public GameObject audioPanel;
        public GameObject TitleTexts;
        public GameObject mask;
        public Text pauseMenu;    
        public Camera mainCam;
        internal static Camera mainCamShared;
        public GameObject mainCamObj;
        public float detailDensity; 
        public float timeScale = 1f;
        public Terrain terrain;
        public Terrain simpleTerrain;
        internal static float shadowDistINI;
        internal static float renderDistINI;
        internal static float aaQualINI;
        internal static float densityINI;
        internal static float treeMeshAmtINI;
        internal static float fovINI;
        internal static int msaaINI;

        internal static int vsyncINI;
        public Dropdown aaCombo;
        public Dropdown afCombo;

        public Slider fovSlider;
        public Slider modelQualSlider;
        public Slider terrainQualSlider;
        public Slider highQualTreeSlider;
        public Slider renderDistSlider;
        public Slider terrainDensitySlider;
        public Slider shadowDistSlider;
        public Slider audioMasterSlider;
        public Slider audioMusicSlider;
        public Slider audioEffectsSlider;
        public Slider masterTexSlider;
        public Slider shadowCascadesSlider;
        public Toggle vSyncToggle;
        public Toggle dofToggle;
        public Toggle fullscreenToggle;

        public Text presetLabel;
        public Text resolutionLabel;
        public float[] LODBias;
        public float[] shadowDist;
        public AudioSource[] music;
        public AudioSource[] effects;
        public GameObject[] otherUIElements;
        public Boolean hardCodeSomeVideoSettings;
        public Boolean useSimpleTerrain;
        public static Boolean readUseSimpleTerrain;
        public EventSystem uiEventSystem;
        public GameObject defualtSelectedVideo;
        public GameObject defualtSelectedAudio;
        public GameObject defualtSelectedMain;
        //last music multiplier; this should be a value between 0-1
        internal static float lastMusicMult;
        //last audio multiplier; this should be a value between 0-1
        internal static float lastAudioMult;
        //Initial master volume
        internal static float beforeMaster;
        //last texture limit 
        internal static int lastTexLimit;
        //int for amount of effects
        private int _audioEffectAmt = 0;
        //Inital audio effect volumes
        private float[] _beforeEffectVol;

        //Initial music volume
        private float _beforeMusic;
        //Preset level
        private int _currentLevel;
        //Resoutions
        private Resolution[] allRes;
        //Camera dof script
        private MonoBehaviour tempScript;
        //Presets 
        private String[] presets = { "Very Low", "Minimal", "Low", "Normal", "Very High", "Ultra", "Extreme" };
        private String[] ruPresets = { "Очень низкое", "Минимальное", "Низкое", "Нормальное", "Очень высокое", "Ультра", "Экстрим" };
        //Fullscreen Boolean
        private Boolean isFullscreen;
        //current resoultion
        internal static Resolution currentRes;
        //Last resoultion 
        private Resolution beforeRes;

        //last shadow cascade value
        internal static int lastShadowCascade;

        public static Boolean aoBool;
        public static Boolean dofBool;
        private Boolean lastAOBool;
        private Boolean lastDOFBool;
        public static Terrain readTerrain;
        public static Terrain readSimpleTerrain;
        public void Start()
        {


            mainCamShared = mainCam;
            //Set the lastmusicmult and last audiomult
            lastMusicMult = audioMusicSlider.value;
            lastAudioMult = audioEffectsSlider.value;
            //Set the first selected item
            uiEventSystem.firstSelectedGameObject = defualtSelectedMain;
            //Get the presets from the quality settings 
            presets = QualitySettings.names;
            presetLabel.text = presets[QualitySettings.GetQualityLevel()].ToString();
            _currentLevel = QualitySettings.GetQualityLevel();
            //Get the current resoultion, if the game is in fullscreen, and set the label to the original resolution
            allRes = Screen.resolutions;
            currentRes = Screen.currentResolution;
            //Debug.Log("ini res" + currentRes);
            isFullscreen = Screen.fullScreen;
            //get initial screen effect bools
            //get all specified audio source volumes
            _beforeEffectVol = new float[_audioEffectAmt];
            beforeMaster = AudioListener.volume;
            //get all ini values
            aaQualINI = QualitySettings.antiAliasing;
            renderDistINI = mainCam.farClipPlane;
            shadowDistINI = QualitySettings.shadowDistance;
            fovINI = mainCam.fieldOfView;
            msaaINI = QualitySettings.antiAliasing;
            vsyncINI = QualitySettings.vSyncCount;
            //enable titles
            TitleTexts.SetActive(true);
            //Find terrain
            terrain = Terrain.activeTerrain;
            //set last texture limit
            lastTexLimit = QualitySettings.globalTextureMipmapLimit;
            //set last shadow cascade 
            lastShadowCascade = QualitySettings.shadowCascades;
        }

        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape) && !mainPanel.activeInHierarchy && !YandexGame.nowAdsShow)
            {

                uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
                mainPanel.SetActive(true);
                vidPanel.SetActive(false);
                audioPanel.SetActive(false);
                TitleTexts.SetActive(true);
                mask.SetActive(true);
                levelsPanel.SetActive(false);
            }
        }

        public void Play()
        {
            levelsPanel.SetActive(true);
            mainPanel.SetActive(false);
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            audioIn();
        }

        public void loadScene(int scene = 0)
        {
            if (scene != 0)
            {
                SceneManager.LoadScene(scene);
            } 
        }

        public void Audio()
        {
            mainPanel.SetActive(false);
            vidPanel.SetActive(false);
            audioPanel.SetActive(true);
            audioIn();
        }
        public void audioIn()
        {
            uiEventSystem.SetSelectedGameObject(defualtSelectedAudio);
            audioMasterSlider.value = AudioListener.volume;
            //Perform modulo to find factor f to allow for non uniform music volumes
            float a; float b; float f;
            try
            {
                a = music[0].volume;
                b = music[1].volume;
                f = a % b;
                audioMusicSlider.value = f;
            }
            catch
            {
                Debug.Log("You do not have multiple audio sources");
                audioMusicSlider.value = lastMusicMult;
            }
            //Do this with the effects
            try
            {
                a = effects[0].volume;
                b = effects[1].volume;
                f = a % b;
                audioEffectsSlider.value = f;
            }
            catch
            {
                Debug.Log("You do not have multiple audio sources");
                audioEffectsSlider.value = lastAudioMult;
            }

        }
        /// <summary>
        /// Audio Option Methods
        /// </summary>
        /// <param name="f"></param>
        public void updateMasterVol(float f)
        {

            //Controls volume of all audio listeners 
            AudioListener.volume = f;
        }
        /// <summary>
        /// Update music effects volume
        /// </summary>
        /// <param name="f"></param>
        public void updateMusicVol(float f)
        {
            try
            {
                for (int _musicAmt = 0; _musicAmt < music.Length; _musicAmt++)
                {
                    music[_musicAmt].volume *= f;
                }
            }
            catch
            {
                Debug.Log("Please assign music sources in the manager");
            }
            //_beforeMusic = music.volume;
        }
        /// <summary>
        /// Update the audio effects volume
        /// </summary>
        /// <param name="f"></param>
        public void updateEffectsVol(float f)
        {
            try
            {
                for (_audioEffectAmt = 0; _audioEffectAmt < effects.Length; _audioEffectAmt++)
                {
                    //get the values for all effects before the change
                    _beforeEffectVol[_audioEffectAmt] = effects[_audioEffectAmt].volume;

                    //lower it by a factor of f because we don't want every effect to be set to a uniform volume
                    effects[_audioEffectAmt].volume *= f;
                }
            }
            catch
            {
                Debug.Log("Please assign audio effects sources in the manager.");
            }

        }
        /// <summary> 
        /// The method for changing the applying new audio settings
        /// </summary>
        public void applyAudio()
        {
            applyAudioMain();
            uiEventSystem.SetSelectedGameObject(defualtSelectedMain);

        }
        /// <summary>
        /// Use an IEnumerator to first play the animation and then change the audio settings
        /// </summary>
        /// <returns></returns>
        public void applyAudioMain()
        {
            mainPanel.SetActive(true);
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            beforeMaster = AudioListener.volume;
            lastMusicMult = audioMusicSlider.value;
            lastAudioMult = audioEffectsSlider.value;
        }
        /// <summary>
        /// Cancel the audio setting changes
        /// </summary>
        public void cancelAudio()
        {
            uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
            cancelAudioMain();
        }
        /// <summary>
        /// Use an IEnumerator to first play the animation and then change the audio settings
        /// </summary>
        /// <returns></returns>
        public void cancelAudioMain()
        {
            
            mainPanel.SetActive(true);
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            AudioListener.volume = beforeMaster;
            try
            {


                for (_audioEffectAmt = 0; _audioEffectAmt < effects.Length; _audioEffectAmt++)
                {
                    //get the values for all effects before the change
                    effects[_audioEffectAmt].volume = _beforeEffectVol[_audioEffectAmt];
                }
                for (int _musicAmt = 0; _musicAmt < music.Length; _musicAmt++)
                {
                    music[_musicAmt].volume = _beforeMusic;
                }
            }
            catch
            {
                Debug.Log("please assign the audio sources in the manager");
            }
        }
        /////Video Options
        /// <summary>
        /// Show video
        /// </summary>
        public void Video()
        {
            levelsPanel.SetActive(false);
            mainPanel.SetActive(false);
            vidPanel.SetActive(true);
            audioPanel.SetActive(false);
            videoIn();

        }
        /// <summary>
        /// Play the "video panel in" animation
        /// </summary>
        public void videoIn()
        {
            uiEventSystem.SetSelectedGameObject(defualtSelectedVideo);

            if (QualitySettings.antiAliasing == 0)
            {
                aaCombo.value = 0;
            }
            else if (QualitySettings.antiAliasing == 2)
            {
                aaCombo.value = 1;
            }
            else if (QualitySettings.antiAliasing == 4)
            {
                aaCombo.value = 2;
            }
            else if (QualitySettings.antiAliasing == 8)
            {
                aaCombo.value = 3;
            }
            if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable)
            {
                afCombo.value = 1;
            }
            else if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Disable)
            {
                afCombo.value = 0;
            }
            else if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable)
            {
                afCombo.value = 2;
            }
            presetLabel.text = presets[QualitySettings.GetQualityLevel()].ToString();
            fovSlider.value = mainCam.fieldOfView;
            modelQualSlider.value = QualitySettings.lodBias;
            renderDistSlider.value = mainCam.farClipPlane;
            shadowDistSlider.value = QualitySettings.shadowDistance;
            masterTexSlider.value = QualitySettings.globalTextureMipmapLimit;
            shadowCascadesSlider.value = QualitySettings.shadowCascades;
            fullscreenToggle.isOn = Screen.fullScreen;
            dofToggle.isOn = dofBool;
            if (QualitySettings.vSyncCount == 0)
            {
                vSyncToggle.isOn = false;
            }
            else if (QualitySettings.vSyncCount == 1)
            {
                vSyncToggle.isOn = true;
            }
            try
            {
                if (useSimpleTerrain == true)
                {
                    highQualTreeSlider.value = simpleTerrain.treeMaximumFullLODCount;
                    terrainDensitySlider.value = simpleTerrain.detailObjectDensity;
                    terrainQualSlider.value = terrain.heightmapMaximumLOD;
                }
                else
                {
                    highQualTreeSlider.value = terrain.treeMaximumFullLODCount;
                    terrainDensitySlider.value = terrain.detailObjectDensity;
                    terrainQualSlider.value = terrain.heightmapMaximumLOD;
                }
            }
            catch
            {
                return;
            }

        }

        /// <summary>
        /// Cancel the video setting changes 
        /// </summary>
        public void cancelVideo()
        {
            uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
            cancelVideoMain();
        }
        /// <summary>
        /// Use an IEnumerator to first play the animation and then changethe video settings
        /// </summary>
        /// <returns></returns>
        public void cancelVideoMain()
        {
            try
            {
                mainCam.farClipPlane = renderDistINI;
                Terrain.activeTerrain.detailObjectDensity = densityINI;
                mainCam.fieldOfView = fovINI;
                mainPanel.SetActive(true);
                vidPanel.SetActive(false);
                audioPanel.SetActive(false);
                aoBool = lastAOBool;
                dofBool = lastDOFBool;
                Screen.SetResolution(beforeRes.width, beforeRes.height, Screen.fullScreen);
                QualitySettings.shadowDistance = shadowDistINI;
                QualitySettings.antiAliasing = (int)aaQualINI;
                QualitySettings.antiAliasing = msaaINI;
                QualitySettings.vSyncCount = vsyncINI;
                QualitySettings.globalTextureMipmapLimit = lastTexLimit;
                QualitySettings.shadowCascades = lastShadowCascade;
                Screen.fullScreen = isFullscreen;
            }
            catch
            {

                Debug.Log("A problem occured (chances are the terrain was not assigned )");
                mainCam.farClipPlane = renderDistINI;
                mainCam.fieldOfView = fovINI;
                mainPanel.SetActive(true);
                vidPanel.SetActive(false);
                audioPanel.SetActive(false);
                aoBool = lastAOBool;
                dofBool = lastDOFBool;
                QualitySettings.shadowDistance = shadowDistINI;
                Screen.SetResolution(beforeRes.width, beforeRes.height, Screen.fullScreen);
                QualitySettings.antiAliasing = (int)aaQualINI;
                QualitySettings.antiAliasing = msaaINI;
                QualitySettings.vSyncCount = vsyncINI;
                QualitySettings.globalTextureMipmapLimit = lastTexLimit;
                QualitySettings.shadowCascades = lastShadowCascade;
                //Screen.fullScreen = isFullscreen;

            }

        }
        //Apply the video prefs
        /// <summary>
        /// Apply the video settings
        /// </summary>
        public void apply()
        {
            applyVideo();
            uiEventSystem.SetSelectedGameObject(defualtSelectedMain);

        }
        /// <summary>
        /// Use an IEnumerator to first play the animation and then change the video settings.
        /// </summary>
        /// <returns></returns>
        public void applyVideo()
        {
            mainPanel.SetActive(true);
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            renderDistINI = mainCam.farClipPlane;
            shadowDistINI = QualitySettings.shadowDistance;
            Debug.Log("Shadow dist ini" + shadowDistINI);
            fovINI = mainCam.fieldOfView;
            dofBool = dofToggle.isOn;
            lastAOBool = aoBool;
            lastDOFBool = dofBool;
            beforeRes = currentRes;
            lastTexLimit = QualitySettings.globalTextureMipmapLimit;
            lastShadowCascade = QualitySettings.shadowCascades;
            vsyncINI = QualitySettings.vSyncCount;
            isFullscreen = Screen.fullScreen;
            try
            {
                if (useSimpleTerrain == true)
                {
                    densityINI = simpleTerrain.detailObjectDensity;
                    treeMeshAmtINI = simpleTerrain.treeMaximumFullLODCount;
                }
                else
                {
                    densityINI = terrain.detailObjectDensity;
                    treeMeshAmtINI = simpleTerrain.treeMaximumFullLODCount;
                }
            }
            catch { Debug.Log("Please assign a terrain"); }

        }
        /// <summary>
        /// Video Options
        /// </summary>
        /// <param name="B"></param>
        public void toggleVSync(Boolean B)
        {
            vsyncINI = QualitySettings.vSyncCount;
            if (B == true)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }

        }
        /// <summary>
        /// Update full high quality tree mesh amount.
        /// </summary>
        /// <param name="f"></param>
        public void updateTreeMeshAmt(int f)
        {

            if (useSimpleTerrain == true)
            {
                simpleTerrain.treeMaximumFullLODCount = (int)f;
            }
            else
            {
                terrain.treeMaximumFullLODCount = (int)f;
            }

        }
        /// <summary>
        /// Change the lod bias using
        /// <c>
        /// QualitySettings.lodBias = LoDBias / 2.15f;
        /// </c> 
        /// LoDBias is only divided by 2.15 because the max is set to 10 on the slider, and dividing by 2.15 results in 4.65, our desired max. However, deleting or changing 2.15 is compeletly fine.
        /// </summary>
        /// <param name="LoDBias"></param>
        public void lodBias(float LoDBias)
        {
            QualitySettings.lodBias = LoDBias / 2.15f;
        }
        /// <summary>
        /// Update the render distance using 
        /// <c>
        /// mainCam.farClipPlane = f;
        /// </c>
        /// </summary>
        /// <param name="f"></param>
        public void updateRenderDist(float f)
        {
            try
            {
                mainCam.farClipPlane = f;

            }
            catch
            {
                Debug.Log(" Finding main camera now...it is still suggested that you manually assign this");
                mainCam = Camera.main;
                mainCam.farClipPlane = f;

            }

        }
        /// <summary>
        /// Update the texture quality using  
        /// <c>QualitySettings.masterTextureLimit </c>
        /// </summary>
        /// <param name="qual"></param>
        public void updateTex(float qual)
        {

            int f = Mathf.RoundToInt(qual);
            QualitySettings.globalTextureMipmapLimit = f;
        }
        /// <summary>
        /// Update the shadow distance using 
        /// <c>
        /// QualitySettings.shadowDistance = dist;
        /// </c>
        /// </summary>
        /// <param name="dist"></param>
        public void updateShadowDistance(float dist)
        {
            QualitySettings.shadowDistance = dist;

        }
        /// <summary>
        /// Change the max amount of high quality trees using 
        /// <c>
        /// terrain.treeMaximumFullLODCount = (int)qual;
        /// </c>
        /// </summary>
        /// <param name="qual"></param>
        public void treeMaxLod(float qual)
        {
            if (useSimpleTerrain == true)
            {
                simpleTerrain.treeMaximumFullLODCount = (int)qual;
            }
            else
            {
                terrain.treeMaximumFullLODCount = (int)qual;
            }

        }
        /// <summary>
        /// Change the height map max LOD using 
        /// <c>
        /// terrain.heightmapMaximumLOD = (int)qual;
        /// </c>
        /// </summary>
        /// <param name="qual"></param>
        public void updateTerrainLod(float qual)
        {
            try { if (useSimpleTerrain == true) { simpleTerrain.heightmapMaximumLOD = (int)qual; } else { terrain.heightmapMaximumLOD = (int)qual; } }
            catch { Debug.Log("Terrain not assigned"); return; }

        }
        /// <summary>
        /// Change the fov using a float. The defualt should be 60.
        /// </summary>
        /// <param name="fov"></param>
        public void updateFOV(float fov)
        {
            mainCam.fieldOfView = fov;
        }
        /// <summary>
        /// Toggle on or off Depth of Field. This is meant to be used with a checkbox.
        /// </summary>
        /// <param name="b"></param>
        public void toggleDOF(Boolean b)
        {
            try
            {

                if (b == true)
                {
                    tempScript.enabled = true;
                    dofBool = true;
                }
                else
                {
                    tempScript.enabled = false;
                    dofBool = false;
                }
            }
            catch
            {
                Debug.Log("No AO post processing found");
                return;
            }



        }
        /// <summary>
        /// Toggle on or off Ambient Occulusion. This is meant to be used with a checkbox.
        /// </summary>
        /// <param name="b"></param>
        public void toggleAO(Boolean b)
        {
            try
            {

                if (b == true)
                {
                    tempScript.enabled = true;
                    aoBool = true;
                }
                else
                {
                    tempScript.enabled = false;
                    aoBool = false;
                }
            }
            catch
            {
                Debug.Log("No AO post processing found");
                return;
            }
        }
        /// <summary>
        /// Set the game to windowed or full screen. This is meant to be used with a checkbox
        /// </summary>
        /// <param name="b"></param>
        public void setFullScreen(Boolean b)
        {


            if (b == true)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
            else
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
            }
        }
        /// <summary>
        /// Method for moving to the next resoution in the allRes array. WARNING: This is not finished/buggy.  
        /// </summary>
        //Method for moving to the next resoution in the allRes array. WARNING: This is not finished/buggy.  
        public void nextRes()
        {
            beforeRes = currentRes;
            //Iterate through all of the resoultions. 
            for (int i = 0; i < allRes.Length; i++)
            {
                //If the resoultion matches the current resoution height and width then go through the statement.
                if (allRes[i].height == currentRes.height && allRes[i].width == currentRes.width)
                {
                    //Debug.Log("found " + i);
                    //If the user is playing fullscreen. Then set the resoution to one element higher in the array, set the full screen boolean to true, reset the current resolution, and then update the resolution label.
                    if (isFullscreen == true) { Screen.SetResolution(allRes[i + 1].width, allRes[i + 1].height, true); isFullscreen = true; currentRes = Screen.resolutions[i + 1]; resolutionLabel.text = currentRes.width.ToString() + " x " + currentRes.height.ToString(); }
                    //If the user is playing in a window. Then set the resoution to one element higher in the array, set the full screen boolean to false, reset the current resolution, and then update the resolution label.
                    if (isFullscreen == false) { Screen.SetResolution(allRes[i + 1].width, allRes[i + 1].height, false); isFullscreen = false; currentRes = Screen.resolutions[i + 1]; resolutionLabel.text = currentRes.width.ToString() + " x " + currentRes.height.ToString(); }

                    //Debug.Log("Res after: " + currentRes);
                }
            }

        }
        /// <summary>
        /// Method for moving to the last resoution in the allRes array. WARNING: This is not finished/buggy.  
        /// </summary>
        //Method for moving to the last resoution in the allRes array. WARNING: This is not finished/buggy.  
        public void lastRes()
        {
            beforeRes = currentRes;
            //Iterate through all of the resoultions. 
            for (int i = 0; i < allRes.Length; i++)
            {
                if (allRes[i].height == currentRes.height && allRes[i].width == currentRes.width)
                {

                    //Debug.Log("found " + i);
                    //If the user is playing fullscreen. Then set the resoution to one element lower in the array, set the full screen boolean to true, reset the current resolution, and then update the resolution label.
                    if (isFullscreen == true) { Screen.SetResolution(allRes[i - 1].width, allRes[i - 1].height, true); isFullscreen = true; currentRes = Screen.resolutions[i - 1]; resolutionLabel.text = currentRes.width.ToString() + " x " + currentRes.height.ToString(); }
                    //If the user is playing in a window. Then set the resoution to one element lower in the array, set the full screen boolean to false, reset the current resolution, and then update the resolution label.
                    if (isFullscreen == false) { Screen.SetResolution(allRes[i - 1].width, allRes[i - 1].height, false); isFullscreen = false; currentRes = Screen.resolutions[i - 1]; resolutionLabel.text = currentRes.width.ToString() + " x " + currentRes.height.ToString(); }

                    //Debug.Log("Res after: " + currentRes);
                }
            }

        }
        public void enableSimpleTerrain(Boolean b)
        {
            useSimpleTerrain = b;
        }
        /// <summary>
        /// Force aniso on using quality settings
        /// </summary>
        //Force the aniso on.
        public void forceOnANISO()
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }
        /// <summary>
        /// Per texture aniso using quality settings
        /// </summary>
        //Use per texture aniso settings.
        public void perTexANISO()
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
        /// <summary>
        /// Disable aniso using quality setttings
        /// </summary>
        //Disable aniso all together.
        public void disableANISO()
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        /// <summary>
        /// The method for changing aniso settings
        /// </summary>
        /// <param name="anisoSetting"></param>
        public void updateANISO(int anisoSetting)
        {
            if (anisoSetting == 0)
            {
                disableANISO();
            }
            else if (anisoSetting == 1)
            {
                forceOnANISO();
            }
            else if (anisoSetting == 2)
            {
                perTexANISO();
            }
        }

        /// <summary>
        /// The method for setting the amount of shadow cascades
        /// </summary>
        /// <param name="cascades"></param>
        public void updateCascades(float cascades)
        {

            int c = Mathf.RoundToInt(cascades);
            if (c == 1)
            {
                c = 2;
            }
            else if (c == 3)
            {
                c = 2;
            }
            QualitySettings.shadowCascades = c;
        }
        /// <summary>
        /// Update terrain density
        /// </summary>
        /// <param name="density"></param>
        public void updateDensity(float density)
        {
            detailDensity = density;
            try
            {
                terrain.detailObjectDensity = detailDensity;
            }
            catch
            {
                Debug.Log("Please assign a terrain");
            }

        }
        /// <summary>
        /// Update MSAA quality using quality settings
        /// </summary>
        /// <param name="msaaAmount"></param>
        public void updateMSAA(int msaaAmount)
        {
            if (msaaAmount == 0)
            {
                disableMSAA();
            }
            else if (msaaAmount == 1)
            {
                twoMSAA();
            }
            else if (msaaAmount == 2)
            {
                fourMSAA();
            }
            else if (msaaAmount == 3)
            {
                eightMSAA();
            }

        }
        /// <summary>
        /// Set MSAA to 0x (disabling it) using quality settings
        /// </summary>
        public void disableMSAA()
        {

            QualitySettings.antiAliasing = 0;
            // aaOption.text = "MSAA: " + QualitySettings.antiAliasing.ToString();
        }
        /// <summary>
        /// Set MSAA to 2x using quality settings
        /// </summary>
        public void twoMSAA()
        {

            QualitySettings.antiAliasing = 2;
            // aaOption.text = "MSAA: " + QualitySettings.antiAliasing.ToString();
        }
        /// <summary>
        /// Set MSAA to 4x using quality settings
        /// </summary>
        public void fourMSAA()
        {

            QualitySettings.antiAliasing = 4;

            // aaOption.text = "MSAA: " + QualitySettings.antiAliasing.ToString();
        }
        /// <summary>
        /// Set MSAA to 8x using quality settings
        /// </summary>
        public void eightMSAA()
        {

            QualitySettings.antiAliasing = 8;
            // aaOption.text = "MSAA: " + QualitySettings.antiAliasing.ToString();
        }
        /// <summary>
        /// Set the quality level one level higher. This is done by getting the current quality level, then using 
        /// <c> 
        /// QualitySettings.IncreaseLevel();
        /// </c>
        /// to increase the level. The current level variable is set to the new quality setting, and the label is updated.
        /// </summary>
        public void nextPreset()
        {
            _currentLevel = QualitySettings.GetQualityLevel();
            QualitySettings.IncreaseLevel();
            _currentLevel = QualitySettings.GetQualityLevel();
            presetLabel.text = presets[_currentLevel].ToString();
            if (hardCodeSomeVideoSettings == true)
            {
                QualitySettings.shadowDistance = shadowDist[_currentLevel];
                QualitySettings.lodBias = LODBias[_currentLevel];
            }
        }
        /// <summary>
        /// Set the quality level one level lower. This is done by getting the current quality level, then using 
        /// <c> 
        /// QualitySettings.DecreaseLevel();
        /// </c>
        /// to decrease the level. The current level variable is set to the new quality setting, and the label is updated.
        /// </summary>
        public void lastPreset()
        {
            _currentLevel = QualitySettings.GetQualityLevel();
            QualitySettings.DecreaseLevel();
            _currentLevel = QualitySettings.GetQualityLevel();
            presetLabel.text = presets[_currentLevel].ToString();
            if (hardCodeSomeVideoSettings == true)
            {
                QualitySettings.shadowDistance = shadowDist[_currentLevel];
                QualitySettings.lodBias = LODBias[_currentLevel];
            }

        }
        /// <summary>
        /// Hard code the minimal settings
        /// </summary>
        public void setMinimal()
        {
            QualitySettings.SetQualityLevel(0);
            //QualitySettings.shadowDistance = 12.6f;
            QualitySettings.shadowDistance = shadowDist[0];
            //QualitySettings.lodBias = 0.3f;
            QualitySettings.lodBias = LODBias[0];
        }
        /// <summary>
        /// Hard code the very low settings
        /// </summary>
        public void setVeryLow()
        {
            QualitySettings.SetQualityLevel(1);
            //QualitySettings.shadowDistance = 17.4f;
            QualitySettings.shadowDistance = shadowDist[1];
            //QualitySettings.lodBias = 0.55f;
            QualitySettings.lodBias = LODBias[1];
        }
        /// <summary>
        /// Hard code the low settings
        /// </summary>
        public void setLow()
        {
            QualitySettings.SetQualityLevel(2);
            //QualitySettings.shadowDistance = 29.7f;
            //QualitySettings.lodBias = 0.68f;
            QualitySettings.lodBias = LODBias[2];
            QualitySettings.shadowDistance = shadowDist[2];
        }
        /// <summary>
        /// Hard code the normal settings
        /// </summary>
        public void setNormal()
        {
            QualitySettings.SetQualityLevel(3);
            //QualitySettings.shadowDistance = 82f;
            //QualitySettings.lodBias = 1.09f;
            QualitySettings.shadowDistance = shadowDist[3];
            QualitySettings.lodBias = LODBias[3];
        }
        /// <summary>
        /// Hard code the very high settings
        /// </summary>
        public void setVeryHigh()
        {
            QualitySettings.SetQualityLevel(4);
            //QualitySettings.shadowDistance = 110f;
            //QualitySettings.lodBias = 1.22f;
            QualitySettings.shadowDistance = shadowDist[4];
            QualitySettings.lodBias = LODBias[4];
        }
        /// <summary>
        /// Hard code the ultra settings
        /// </summary>
        public void setUltra()
        {
            QualitySettings.SetQualityLevel(5);
            //QualitySettings.shadowDistance = 338f;
            //QualitySettings.lodBias = 1.59f;
            QualitySettings.shadowDistance = shadowDist[5];
            QualitySettings.lodBias = LODBias[5];
        }
        /// <summary>
        /// Hard code the extreme settings
        /// </summary>
        public void setExtreme()
        {
            QualitySettings.SetQualityLevel(6);
            //QualitySettings.shadowDistance = 800f;
            //QualitySettings.lodBias = 4.37f;
            QualitySettings.shadowDistance = shadowDist[6];
            QualitySettings.lodBias = LODBias[6];
        }

    }
}

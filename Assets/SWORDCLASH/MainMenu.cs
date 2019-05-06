using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    // editor fields
    // public Sprite []backgrounds;
    // GetComponent<SpriteRenderer>().sprite = backgrounds[index];

    public Sprite UnPokedEyeSprite; //TODO: make this actually animated one day... one glorious day
    // what sprite changes to when poked I guess for now.
   public Sprite PokedEyeSprite;
    // hoping I'll be able to disable settings menu EZ with this:
    /*
     * for (int i = 0; i < numberOfGameObjects; i++)
                {
                    // Note that some of the GameObjects
                    // might have true already.
                    gameObjects[i].SetActive(true);
                } 
     */
    public GameObject SettingsCanvasGO;
    private Canvas SettingsCanvasRef;

    private Sprite ActiveEyeSprite;

    private SpriteRenderer EyeSpriteRenderer;

    private bool SettingsOpen;

    // Use this for initialization
    void Start()
    {
        // Set sprite renderer reference so tentacle can change color
        EyeSpriteRenderer = GetComponent<SpriteRenderer>();
        UnPokedEyeSprite = EyeSpriteRenderer.sprite;
        ActiveEyeSprite = UnPokedEyeSprite;

        // will this work if there are multiple canvas, yes just use containing Game Object, like in TC
        SettingsCanvasRef = SettingsCanvasGO.GetComponent<Canvas>();
        if (SettingsCanvasRef != null)
        {
            // disable canvas u find;
            SettingsOpen = false;
            SettingsCanvasRef.gameObject.SetActive(false);
        }
    }

    private void ToggleSettingsMenu()
    {
        // flip boolean
        SettingsOpen = !SettingsOpen; 
        SettingsCanvasRef.gameObject.SetActive(SettingsOpen);
    }

    // Not sure if this is back button; also using it for outside of settings window clicks
    public void BackButton_OnClicked()
    {
        if (SettingsOpen)
        {
            // close the settings bubs
            ToggleSettingsMenu();
        }

        //TODO: seperate methods, and probably should pause if during gameplay
    }

    public void PlayButton_OnClicked()
    {
        if (SettingsOpen)
        {
            // close the settings bubs
            ToggleSettingsMenu();
        }

        // LoadSceneByIndex();
        StartCoroutine(DelaySceneLoad());

        if (EyeSpriteRenderer != null)
        {
            OnEyePoked();
        }
    }

    // pop open settings overlay
    public void SettingsButton_OnClicked()
    {
        // make settings canvas enabled/active/appear or go away if it is there;
        ToggleSettingsMenu();
    }

    // menu is assumed to be index 0 in build order, so 1 is game...
    private void LoadSceneByIndex(int index = 1)
    {
        // load topmost scene index
        SceneManager.LoadScene(index);
    }



 
// coroutines are always type IEnumerator
IEnumerator DelaySceneLoad()
{
    // wait for float seconds b4 switching to new scene
    yield return new WaitForSeconds(1.5f);
        LoadSceneByIndex();
}

private void OnEyePoked() {
        // just switch sprite for now.
        ActiveEyeSprite = PokedEyeSprite;

    }

    // Update is called once per frame, sprite update() hack to avoid overkill event system
    void Update()
    {
        EyeSpriteRenderer.sprite = ActiveEyeSprite;
    }
}

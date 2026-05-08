using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameScManger : MonoBehaviour
{

    public static GameScManger instance;

    public GeneralGameSettings generalGameSettings;

    //ui pannels
    public GameObject pausePanel, winPanel, levelListPanel, notEnoughGames, addCoinsAfterBottleFilled;
    public AudioClip winSound, Click_sound;
    public AudioSource game_AudioSOurce;
    public AudioListener mainLisitner_audio;


    // ui buttons
    public Button sound_BTN, add_bottle_btn;
    public Sprite mutedImage, non_muted_img;



    // ui prices to skip and add more bottles 
    public Text skipLevelPrice_txt, undoPriceTxt, CoinsUIText, coinsTextWinPanel;


    // singilton design pattern
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else
        {

            Destroy(gameObject);

        }
    }


    // Start is called before the first frame update
    void Start()
    {
        updateMain_CoinsVlue();
        // check the sound status
        checkSound_status();
        setupUiButtonsPrices();


        Click_sound = generalGameSettings.click_sound;

        Screen.orientation = ScreenOrientation.Portrait;

        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        notEnoughGames.SetActive(false);
        addCoinsAfterBottleFilled.SetActive(false);



       // StartCoroutine(showBannerAdd());


    }
   /* IEnumerator showBannerAdd()
    {
        while (GameAds.instance.isBannerShowing() == false)
        {
            yield return new WaitForSeconds(2.0f);
            GameAds.instance.showbannerAD();
        }
    }*/


    public void givePlayerBottleCoins()
    {
        int coins = PlayerPrefs.GetInt("game_coins_number");
        coins += generalGameSettings.coinsNumberRewarded_bottleFillsUp;

        addCoinsAfterBottleFilled.SetActive(true);


        // show the  animation 
        PlayerPrefs.SetInt("game_coins_number", coins);

        updateMain_CoinsVlue();

    }

    public void undoRewardPlayer()
    {
        int coins = PlayerPrefs.GetInt("game_coins_number");
        coins -= generalGameSettings.coinsNumberRewarded_bottleFillsUp;

        // show the  animation 
        PlayerPrefs.SetInt("game_coins_number", coins);

        updateMain_CoinsVlue();
    }

    void updateMain_CoinsVlue()
    {
        // update the coins ui 
        CoinsUIText.text = PlayerPrefs.GetInt("game_coins_number").ToString();

    }




    void setupUiButtonsPrices()
    {
        skipLevelPrice_txt.text = generalGameSettings.coinsNumber_to_skip.ToString();
        undoPriceTxt.text = generalGameSettings.coinsNumber_to_UndoMoves.ToString();
    }




    private void checkSound_status()
    {

        // check sound status 
        int i = PlayerPrefs.GetInt("sound_status", 1);
        if (i == 0)
        {
            mute_audio();
        }
        else
        {
            inmute_audio();
        }

    }

    public void mute_audio()
    {
        PlayerPrefs.SetInt("sound_status", 0); //no sound (muted) 
        sound_BTN.GetComponent<Image>().sprite = mutedImage;
        game_AudioSOurce.mute = true;
        AudioListener.pause = true;


    }
    public void inmute_audio()
    {
        PlayerPrefs.SetInt("sound_status", 1);  // sound not muted   
        sound_BTN.GetComponent<Image>().sprite = non_muted_img;
        game_AudioSOurce.mute = false;
        AudioListener.pause = false;

    }








    #region  ui manager in game


    public void openLevelList()
    {
        // open levle ist
        SceneManager.LoadScene(1);
    }


    public void openMainMenu()
    {
        // load main scene
        SceneManager.LoadScene(0);
    }

    // principal menu butotns 
    public void restartBtnCLikced()
    {
        SceneManager.LoadScene(3);
    }


    public void addOneTubeBtnClicked()
    {
        GameAds.instance.showreward_Ad();
    }

    int v_btl = 0;
    public void reward_player_addBottle()
    {


        // check if it's possible to ad
        if (v_btl >= 2)
        {
            return;
        }

        v_btl++;
        if (v_btl >= 2)
        {
            add_bottle_btn.enabled = false;
            add_bottle_btn.transform.GetChild(0).gameObject.SetActive(true);
        }
        LevleGeneartor.instance.addAnotherBottle();
    }

    
    public void reward_player_skip_level()
    {
            LevleGeneartor.instance.Skip_Level();
    }

    public void UndoMoveCLicked()
    {




        int coins = getcurrentCoinsNumber();
        if (coins >= generalGameSettings.coinsNumber_to_UndoMoves)
        {
            if (GameController.instance.doesUndoAvailibal() == true)
            {
                coins -= generalGameSettings.coinsNumber_to_UndoMoves;
                PlayerPrefs.SetInt("game_coins_number", coins);

                // add a new bottle to the game
                updateMain_CoinsVlue();



                GameController.instance.makeUndoMovement();

            }

        }
        else
        {
            notEnoughGames.SetActive(true);
        }


        // do undo moves with the needed coins number
    }

    public void clickOnSkipbutoon()
    {
        GameAds.instance.showreward_Ad_Skip();
        // int coins = getcurrentCoinsNumber();
        // if (coins >= generalGameSettings.coinsNumber_to_skip)
        // {
        //     coins -= generalGameSettings.coinsNumber_to_skip;
        //     PlayerPrefs.SetInt("game_coins_number", coins);

        //     // skip the game 
        //     updateMain_CoinsVlue();

        //     LevleGeneartor.instance.Skip_Level();

        // }
        // else
        // {
        //     notEnoughGames.SetActive(true);
        //     PlayerPrefs.SetInt("reward_stats", 1);
        //     GameAds.instance.showreward_Ad();
        // }
    }


    int getcurrentCoinsNumber()
    {
        return PlayerPrefs.GetInt("game_coins_number", 0);
    }

    #endregion



    #region  win panel butotns
    public void win_level()
    {
        // change the coins number on win panel
        coinsTextWinPanel.text = PlayerPrefs.GetInt("game_coins_number").ToString();



        winPanel.SetActive(true);

        LevleGeneartor.instance.openTheWinsLevel();

        game_AudioSOurce.clip = winSound;
        game_AudioSOurce.Play();

    }



    public void bottleFullAddCoins()
    {

    }



    public void palyClickSound()
    {
        game_AudioSOurce.clip = Click_sound;
        game_AudioSOurce.Play();
    }
    #endregion

    #region sound manager 

    public void CLickOnSOundBtn()
    {

        int a = PlayerPrefs.GetInt("sound_status", 1);
        if (a == 0)
        {
            inmute_audio();
        }
        else
        {
            mute_audio();
        }




    }


    public void openHomeScene()
    {
        SceneManager.LoadScene(0);
    }


    #endregion


    #region undo event






    #endregion

}

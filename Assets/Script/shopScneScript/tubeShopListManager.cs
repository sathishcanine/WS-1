using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tubeShopListManager : MonoBehaviour
{
    public Button tubeButtonPrefab;

    public GameObject shop_panel, notEnoughCoins_Panel;

    // parents objects
    public Transform tubesParent;

    private Button[] bottles_list_Buttons;

    List<Button> TubesButtonObjects;

    public GeneralGameSettings generalGameSettings;



    // sprites status for the image
    public Sprite lockedShopItem_sprite, opensShopItem_sprite, selectedShopItem_sprite;


    // Start is called before the first frame update
    void Start()
    {
        // set the old selected sprite for the background

        saveItemStatus(0, 1);
        saveItemStatus(1, 1);


        generateBackButtons();
        notEnoughCoins_Panel.SetActive(false);
    }


    // back butotns
    void generateBackButtons()
    {
        TubesButtonObjects = new List<Button>();

        TubeBack[] TubeShopList = generalGameSettings.shopTubes;
        for (int i = 0; i < TubeShopList.Length; i++)
        {
            int copy = i + 1;
            // instantiate buttons 
            Button currentButton = Instantiate(tubeButtonPrefab, tubeButtonPrefab.transform.position, Quaternion.identity);
            currentButton.transform.SetParent(tubesParent.transform, false);

            // give those new buttons listener ... when the buton clicked 
            currentButton.onClick.AddListener(() => selectBack(copy));
            TubesButtonObjects.Add(currentButton);

        }


        for (int i = 0; i < TubeShopList.Length; i++)
        {

            // give the shop item a priview image of the background
            TubesButtonObjects[i].transform.GetChild(0).GetComponentInChildren<Image>().sprite = TubeShopList[i].prefabSprite; // button sprite
                                                                                                                               // Debug.Log("the number is" + i);



            // is the background locked or unlocked
            if (LoadItemStatus(i) == false)      // that's means it's locked
            {
                // its locked assign the locked panel
                TubesButtonObjects[i].transform.GetComponentInChildren<Image>().sprite = lockedShopItem_sprite;


                // active the locked panel
                TubesButtonObjects[i].transform.GetChild(2).GetComponentInChildren<Image>().gameObject.SetActive(true);

                // because it's locked update the price to open text
                TubesButtonObjects[i].transform.GetChild(1).GetComponentInChildren<Text>().text = TubeShopList[i].coinsNumber_To_Unlock.ToString();  // button level to open

            }
            else
            {

                // its locked assign the locked panel
                TubesButtonObjects[i].transform.GetComponentInChildren<Image>().sprite = opensShopItem_sprite;


                TubesButtonObjects[i].transform.GetChild(2).gameObject.SetActive(false);

                TubesButtonObjects[i].transform.GetChild(1).GetComponentInChildren<Text>().text = "";  // button level to open

            }

            int selected_item = PlayerPrefs.GetInt("Selected_Tube", 0);
            if (i == selected_item)
            {
                TubesButtonObjects[i].transform.GetComponentInChildren<Image>().sprite = selectedShopItem_sprite;
            }
        }

    }


    void selectBack(int index)
    {

        TubeBack selectedtube = generalGameSettings.shopTubes[index - 1];

        if (LoadItemStatus(index - 1) == false)
        {
            // then try to open it 

            int playerGameCoins = PlayerPrefs.GetInt("game_coins_number", 0);
            if (playerGameCoins >= selectedtube.coinsNumber_To_Unlock)
            {

                playerGameCoins -= selectedtube.coinsNumber_To_Unlock;

                saveItemStatus(index - 1, 1);


                //selectedtube.isOpened = true;
                PlayerPrefs.SetInt("game_coins_number", playerGameCoins);

                ShopSystem_Manager.instance.UpdateCoinsSHopPanel();


                // unlock the ui   
                // its locked assign the locked panel
                TubesButtonObjects[index - 1].transform.GetComponentInChildren<Image>().sprite = opensShopItem_sprite;   

                TubesButtonObjects[index - 1].transform.GetChild(2).gameObject.SetActive(false);

                TubesButtonObjects[index - 1].transform.GetChild(1).GetComponentInChildren<Text>().text = "";  // button level to open


            }
            else
            {
                notEnoughCoins_Panel.SetActive(true);
            }

        }
        else
        {

            // the item is opened so select that item
            int oldselected = PlayerPrefs.GetInt("Selected_Tube", 0);
            TubesButtonObjects[oldselected].transform.GetComponentInChildren<Image>().sprite = opensShopItem_sprite;


            // give the new selected item data needed
            PlayerPrefs.SetInt("Selected_Tube", index - 1);

            TubesButtonObjects[index - 1].transform.GetComponentInChildren<Image>().sprite = selectedShopItem_sprite;


        }


    }




    #region   save/load data items



    void saveItemStatus(int ListIndex, int status)
    {

        // 0 : the item is locked
        // 1 : the item is unlocked

        PlayerPrefs.SetInt("TubeStatusSH" + ListIndex, status);
    }



    bool LoadItemStatus(int ListIndex)
    {

        // 0 : the item is locked
        // 1 : the item is unlocked


        int status = PlayerPrefs.GetInt("TubeStatusSH" + ListIndex, 0);


        if (status == 0)
        {
            // locked
            return false;

        }
        else
        {
            //unlocked
            return true;
        }
    }



    #endregion


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class WiiRemoteMenuUI : MonoBehaviour
{
    [SerializeField] public List<Sprite> START_SPRITE_STATES;
    [SerializeField] public List<Sprite> OPTION_SPRITE_STATES;
    [SerializeField] public List<Sprite> QUIT_SPRITE_STATES;

    private Wiimote wiimote;
    private Button[] buttons;
    private int selected_idx = 0;

    private List<List<Sprite>> ALL_SPRITE_STATES;
    private int[] STATE_IDXS;

    private bool is_dUp_down = false;
    private bool is_dDown_down = false;
    private bool is_dA_down = false;

    // Start is called before the first frame update
    void Start()
    {
        WiimoteManager.FindWiimotes();
        if (WiimoteManager.HasWiimote() == false)
            return;
        this.wiimote = WiimoteManager.Wiimotes[0];

        buttons = GetComponentsInChildren<Button>();
        buttons[selected_idx].OnSelect(null);

        ALL_SPRITE_STATES = new List<List<Sprite>>();
        ALL_SPRITE_STATES.Add(START_SPRITE_STATES);
        ALL_SPRITE_STATES.Add(OPTION_SPRITE_STATES);
        ALL_SPRITE_STATES.Add(QUIT_SPRITE_STATES);

        // Initialize Sprite idx {start, option, exit}
        STATE_IDXS = new int[] { 1, 0, 0 };
    }

    // Update is called once per frame
    void Update()
    {
        // Update All Sprites For Selection Buttons
        UpdateAllSprites();

        // -- Wiiremote Checking and Data Reading -- //
        if (WiimoteManager.HasWiimote() == false)
            return;
        this.wiimote = WiimoteManager.Wiimotes[0];
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();
        } while (ret > 0);


        // Button Click Checks
        if (wiimote.Button.d_up)
        {
            is_dUp_down = true;
        }
        else
        {
            // Release
            if (is_dUp_down)
            {
                Debug.Log("Up Button Pressed.");
                if (selected_idx > 0)
                {
                    // Deselect Previous Selection
                    STATE_IDXS[selected_idx] -= 1;
                    selected_idx -= 1;
                    // Select Current Selection
                    STATE_IDXS[selected_idx] += 1;
                }
            }
            is_dUp_down = false;
        }

        if (wiimote.Button.d_down)
        {
            is_dDown_down = true;
        }
        else
        {   
            // Release
            if (is_dDown_down)
            {
                Debug.Log("Down Button Pressed.");
                if (selected_idx < buttons.Length - 1)
                {
                    // Deselect Previous Selection
                    STATE_IDXS[selected_idx] -= 1;
                    selected_idx += 1;
                    // Select Current Selection
                    STATE_IDXS[selected_idx] += 1;
                }
            }
            is_dDown_down = false;
        }

        if (wiimote.Button.a)
        {
            is_dA_down = true;
        }
        else
        {
            if (is_dA_down)
            {
                Debug.Log("A Button Pressed.");
                // Move to onClick Sprite
                STATE_IDXS[selected_idx] += 1;
                buttons[selected_idx].onClick.Invoke();
            }
            is_dA_down = false;
        }
    }

    void UpdateAllSprites()
    {
        for(int idx = 0; idx < 3; idx++)
            buttons[idx].GetComponent<Image>().sprite = ALL_SPRITE_STATES[idx][STATE_IDXS[idx]];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class WiiRemoteMenuUI : MonoBehaviour
{
    private Wiimote wiimote;
    private Button[] buttons;
    private int selected_idx = 0;

    // Start is called before the first frame update
    void Start()
    {
        WiimoteManager.FindWiimotes();
        if (WiimoteManager.HasWiimote() == false)
            return;
        this.wiimote = WiimoteManager.Wiimotes[0];
        buttons = GetComponentsInChildren<Button>();
        buttons[selected_idx].OnSelect(null);
    }

    // Update is called once per frame
    void Update()
    {
        buttons[selected_idx].OnSelect(null);

        if (WiimoteManager.HasWiimote() == false)
            return;

        this.wiimote = WiimoteManager.Wiimotes[0];

        if (wiimote.Button.d_up)
        {
            Debug.Log("Up Button Pressed.");
            if(selected_idx > 0)
                selected_idx -= 1;
        }

        if (wiimote.Button.d_down)
        {
            Debug.Log("Down Button Pressed.");
            if (selected_idx < buttons.Length - 1)
                selected_idx += 1;
        }

        if (wiimote.Button.a)
        {
            buttons[selected_idx].onClick.Invoke();
        }
    }
}

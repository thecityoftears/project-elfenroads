using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

using UnityEngine.EventSystems;


public class Login : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public GameObject errorPopup;
    public Text errorText;
    private string URL = "http://127.0.0.1:4242/"; //127.0.0.1 = localhost means running on your pc locally, 192.168.100.1 = running through the vpn tunnel on Ron's pc

    public GameObject LoginScreen;
    public GameObject SessionListScreen;

    // since this is first scene we will initialize the Lobby Service
    public void Start()
    {
        LobbyService.Initialize(URL);
        usernameInput.Select();
    }

    // Login Request from User
    public async void login()
    {
        if (usernameInput.text.Length == 0)
        {
            errorText.text = "Error: Missing Username";
            errorPopup.SetActive(true);
        }
        else if (passwordInput.text.Length == 0)
        {
            errorText.text = "Error: Missing Password";
            errorPopup.SetActive(true);
        }
        else
        {
            errorPopup.SetActive(false);

            LoginResponse response = await LobbyService.Login(usernameInput.text, passwordInput.text);

            if (response == null)
            {
                errorText.text = "Error: Login Failed";
                errorPopup.SetActive(true);
            }
            else
            {
                Client.AccessToken = response.AccessToken.Replace("+", "%2b"); // need to replace since + encodes blankspace in http messages
                Client.RefreshToken = response.RefreshToken;
                Client.Username = usernameInput.text;

                LoginScreen.SetActive(false);
                SessionListScreen.SetActive(true);
                SessionListScreen.GetComponent<SessionListGetter>().RefreshSessionList();
            }
        }
    }

    // to allow tabbing through
    // taken from https://forum.unity.com/threads/tab-between-input-fields.263779/
    public void OnTab()
    {
        Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

        if (next != null)
        {

            InputField inputfield = next.GetComponent<InputField>();
            if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(EventSystem.current));  //if it's an input field, also set the text caret

            EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
        }
        //else Debug.Log("next nagivation element not found");
    }
}


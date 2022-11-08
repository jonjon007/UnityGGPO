using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.Party;
using PlayFab.ClientModels;
//using TMPro;
using System.Text;
using UnityEngine.InputSystem;

#if MICROSOFT_GAME_CORE
using XGamingRuntime;
using XBL = XGamingRuntime.SDK.XBL;
#endif



public class PartyStarter : MonoBehaviour {
    const string QUEUE_NAME = "CasualMatch";

    [Header("Set in Hierarchy")]
    //public TMP_Text NetworkDescriptorText;
    public InputField NetworkDescriptorText;

    PlayFab.MultiplayerModels.EntityKey EntityKeyP1;
    PlayFab.MultiplayerModels.EntityKey EntityKeyP2;
    private LoginResult loginResult = null;


    // Start is called before the first frame update
    void Start() {
        StartCoroutine(SignInWhenReady());
    }

    private void Update() {
        if (Keyboard.current != null) {
            long input = 0;
            if (Keyboard.current.wKey.wasPressedThisFrame) input = 8;
            if (Keyboard.current.aKey.wasPressedThisFrame) input = 4;
            if (Keyboard.current.sKey.wasPressedThisFrame) input = 2;
            if (Keyboard.current.dKey.wasPressedThisFrame) input = 6;
            if (input > 0) {
                byte[] requestAsBytes = Encoding.UTF8.GetBytes($"Pressed direction {input}");
                PlayFabMultiplayerManager.Get().SendDataMessageToAllPlayers(requestAsBytes);
            }
        }
    }

    private IEnumerator SignInWhenReady() {
        // Sign into PlayFab
        // Log into playfab. The SDK will use the logged in user when connecting to the network.
#if MICROSOFT_GAME_CORE
        // Wait until we're signed into Xbox
        while (!XboxManager.Instance.SignedIn)
        {
            yield return new WaitForEndOfFrame();
        }
        var request = new LoginWithXboxRequest { CreateAccount = true, TitleId = PlayFabSettings.TitleId, XboxToken = XboxManager.Instance.XToken };
        PlayFabClientAPI.LoginWithXbox(request, OnLoginSuccess, OnLoginFailure);
#else
        var request = new LoginWithCustomIDRequest { CustomId = UnityEngine.Random.value.ToString(), CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
#endif
        // Return null to end the CoRoutine
        yield return null;
    }

    private void OnLoginSuccess(LoginResult result) {
        Debug.Log("Logged into PlayFab.");
        loginResult = result;

        PlayFabMultiplayerManager.Get().OnDataMessageReceived += OnDataMessageReceived;
    }

    private void OnDataMessageReceived(object sender, PlayFabPlayer from, byte[] buffer) {
        Debug.Log(Encoding.Default.GetString(buffer));
    }

    private void OnLoginFailure(PlayFabError error) {
        Debug.Log("Error logging into PlayFab: " + error.ErrorMessage);
        //output.text += "\r\n Error logging into PlayFab: " + error.ErrorMessage;
    }

    public void CreateNetwork() {
        PlayFabMultiplayerManager.Get().CreateAndJoinNetwork();

        PlayFabMultiplayerManager.Get().OnNetworkJoined += OnNetworkJoined;
        PlayFabMultiplayerManager.Get().OnRemotePlayerJoined += OnRemotePlayerJoined;
        PlayFabMultiplayerManager.Get().OnRemotePlayerLeft += OnRemotePlayerLeft;
    }

    public void JoinNetwork() {
        // Can only execute if logged in
        if (loginResult is null) return;

        string networkId = NetworkDescriptorText.text;
        PlayFabMultiplayerManager.Get().JoinNetwork(networkId);

        PlayFabMultiplayerManager.Get().OnNetworkJoined += OnNetworkJoined;
        PlayFabMultiplayerManager.Get().OnRemotePlayerJoined += OnRemotePlayerJoined;
        PlayFabMultiplayerManager.Get().OnRemotePlayerLeft += OnRemotePlayerLeft;

        Debug.Log("Join network request made successfully.");
    }

    private void OnNetworkJoined(object sender, string networkId) {
        // Print the Network ID so you can give it to the other client.
        Debug.Log(networkId);
        NetworkDescriptorText.text = networkId;
    }

    private void OnRemotePlayerLeft(object sender, PlayFabPlayer player) {
        Debug.Log($"Player: {player.EntityKey.Id} left!");
    }

    private void OnRemotePlayerJoined(object sender, PlayFabPlayer player) {
        var localPlayer = PlayFabMultiplayerManager.Get().LocalPlayer; //Local player
        Debug.Log($"Player: {player.EntityKey.Id} joined!");
    }
}

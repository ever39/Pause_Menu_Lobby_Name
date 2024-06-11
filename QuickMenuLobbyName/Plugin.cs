using BepInEx;
using UnityEngine;
using HarmonyLib;
using TMPro;
using BepInEx.Logging;
using UnityEngine.UI;
using BepInEx.Configuration;
using System.Collections;

namespace QuickMenuLobbyName
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static GameObject lobbyNameObj, QuickMenuObj;
        private static TextMeshProUGUI nameObj;
        private static string lobbyName;
        private readonly Harmony lobbyHarmony = new Harmony("command.QuickMenuLobbyName");
        public const string VERSION = "1.1.0";
        public const string GUID = "command.QuickMenuLobbyName";
        public const string NAME = "PauseMenu Lobby Name";

        public static ConfigEntry<bool> CopyNameOnDisconnect;
        public static ConfigEntry<float> lobbyNameFont;

        private void Awake()
        {
            CopyNameOnDisconnect = base.Config.Bind("Mod Settings", "Copy on disconnect", false, "Should the mod copy the name of the lobby you were in before you disconnect?");
            lobbyNameFont = base.Config.Bind("Mod Settings", "Font size", 16.5f, "The font of the lobby name text");
            lobbyHarmony.PatchAll(typeof(Plugin));
            base.Logger.LogInfo("QuickMenu patched to add the lobby name!");
        }

        //make the object when the quickmenumanager is started
        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPostfix]
        private static void GetLobbyNameAndPatch(ref QuickMenuManager __instance)
        {
            lobbyName = GameNetworkManager.Instance.steamLobbyName;

            if (QuickMenuObj == null) QuickMenuObj = GameObject.Find("/Systems/UI/Canvas/QuickMenu/");

            if(GameObject.Find("LobbyName") == null)
            {
                MakeObj(__instance);
            }
        }

        //in case anything goes wrong, make the object again!
        [HarmonyPatch(typeof(QuickMenuManager), "OpenQuickMenu")]
        [HarmonyPostfix]
        public static void MenuChecks(ref QuickMenuManager __instance)
        {
            if (!__instance.isMenuOpen) return;
            if(!lobbyNameObj)
            {
                MakeObj(__instance);
            }
            lobbyNameObj.SetActive(true);
            nameObj.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        [HarmonyPostfix]
        public static void CopyLobbyName()
        {
            if (CopyNameOnDisconnect.Value)
            {
                CopyName();
            }
        }

        [HarmonyPatch(typeof(QuickMenuManager), "Update")]
        [HarmonyPostfix]
        public static void NameDisplay(ref QuickMenuManager __instance)
        {
            lobbyName = GameNetworkManager.Instance.steamLobbyName;
            nameObj.text = lobbyName;

            if (__instance.mainButtonsPanel.activeSelf)
            {
                nameObj.color = new Color(1, 0.43921f, 0.0039f, 1);
                lobbyNameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
                nameObj.text = string.Format("Lobby Name:\n{0}", lobbyName);
            }
            else 
            { 
                lobbyNameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -20);
                if (CopyNameOnDisconnect.Value) nameObj.text = string.Format("(The lobby's name will be copied)\nLobby Name:\n{0}", lobbyName);
                if (!CopyNameOnDisconnect.Value) 
                { 
                    nameObj.text = string.Format("! Lobby Name:\n{0}", lobbyName);
                    nameObj.color = new Color(1, 0.16470588f, 0, 1);
                }
            }

            if (__instance.settingsPanel.activeSelf)
            {
                lobbyNameObj.SetActive(false);
            }
            else lobbyNameObj.SetActive(true);
        }
        

        private static void CopyName()
        {
            string text2 = (GUIUtility.systemCopyBuffer = lobbyName).ToString();
        }

        //object making!
        private static void MakeObj(QuickMenuManager menuManager)
        {
            lobbyNameObj = new GameObject("LobbyName");
            lobbyNameObj.transform.SetParent(QuickMenuObj.transform, false);
            lobbyNameObj.AddComponent<RectTransform>();
            lobbyNameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
            lobbyNameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
            lobbyNameObj.transform.localScale = new Vector3(1, 1, 1);
            nameObj = lobbyNameObj.AddComponent<TextMeshProUGUI>();
            nameObj.transform.SetParent(lobbyNameObj.transform, false);
            nameObj.overflowMode = TextOverflowModes.Overflow;
            nameObj.enabled = true;
            nameObj.fontSize = lobbyNameFont.Value;
            nameObj.alignment = TextAlignmentOptions.Center;
            nameObj.font = menuManager.settingsBackButton.font;
        }
    }
}

using AmongUsMenu;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AmongUsMenu
{
    [BepInPlugin("com.parsast.amongusmenu", "Among Us Menu", "v1.2.0")]
    [BepInProcess("Among Us.exe")]
    public class MainMod : BasePlugin
    {
        private static GameObject? menu;
        private static bool menuVisible = false;
        public static ConfigData configData = new ConfigData();

        public static readonly BepInEx.Logging.ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("AmongUsMenu");

        public override void Load()
        {
            Logger.LogInfo("Mod Initialized");

            // Load config settings
            var loadedConfig = ConfigLoader.LoadSettings();
            configData = loadedConfig ?? new ConfigData();

            // Initialize Harmony
            var harmony = new Harmony("com.parsast.amongusmenu");
            harmony.PatchAll();
        }

        private static void CreateMenu()
        {
            menu = new GameObject("Menu");
            var canvas = menu.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = menu.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            menu.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Panel");
            panel.transform.SetParent(menu.transform);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 400);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            AddButton(panel.transform, "Unlock All Cosmetics", new Vector2(0, 100), ToggleUnlockAllCosmetics);
            AddButton(panel.transform, "No-Clip", new Vector2(0, 50), ToggleNoClip);
            AddButton(panel.transform, "Anti-Ban", new Vector2(0, 0), ToggleAntiBan);
            AddButton(panel.transform, "Use Vents", new Vector2(0, -50), ToggleUseVents);
            AddButton(panel.transform, "Copy Chat Messages", new Vector2(0, -100), ToggleCopyChatMessages);
            AddButton(panel.transform, "Close Menu", new Vector2(0, -160), () => menu?.SetActive(false));

            menu.SetActive(false);
        }

        private static void AddButton(Transform parent, string buttonText, Vector2 position, Action onClickAction)
        {
            var button = new GameObject(buttonText.Replace(" ", "") + "Button");
            button.transform.SetParent(parent);
            var buttonImage = button.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(300, 50);
            buttonRect.anchoredPosition = position;

            var buttonComponent = button.AddComponent<Button>();
            buttonComponent.onClick.AddListener(new Action(onClickAction));

            var textObject = new GameObject("Text");
            textObject.transform.SetParent(button.transform);
            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = buttonText;
            textComponent.font = GetFont("Inter");
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.fontSize = 20;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;

            var textRect = textComponent.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(300, 50);
            textRect.anchoredPosition = Vector2.zero;
        }

        private static Font GetFont(string fontName)
        {
            try
            {
                var font = Font.CreateDynamicFontFromOSFont(fontName, 16);
                if (font != null)
                {
                    return font;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load font '{fontName}': {ex.Message}");
            }

            Logger.LogWarning("Using fallback font (Arial).");
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public static void ToggleMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }

            menuVisible = !menuVisible;
            menu?.SetActive(menuVisible);
        }

        private static void ToggleUnlockAllCosmetics()
        {
            configData.UnlockAllCosmetics = !configData.UnlockAllCosmetics;
            ConfigLoader.SaveSettings(configData);
            Logger.LogInfo($"Unlock All Cosmetics: {(configData.UnlockAllCosmetics ? "Activated" : "Deactivated")}");
        }

        private static void ToggleNoClip()
        {
            configData.NoClip = !configData.NoClip;
            ConfigLoader.SaveSettings(configData);
            Logger.LogInfo($"No-Clip: {(configData.NoClip ? "Activated" : "Deactivated")}");
        }

        private static void ToggleAntiBan()
        {
            configData.AntiBan = !configData.AntiBan;
            ConfigLoader.SaveSettings(configData);
            Logger.LogInfo($"Anti-Ban: {(configData.AntiBan ? "Activated" : "Deactivated")}");
        }

        private static void ToggleUseVents()
        {
            configData.UseVents = !configData.UseVents;
            ConfigLoader.SaveSettings(configData);
            Logger.LogInfo($"Use Vents: {(configData.UseVents ? "Activated" : "Deactivated")}");
        }

        private static void ToggleCopyChatMessages()
        {
            configData.CopyChatMessages = !configData.CopyChatMessages;
            ConfigLoader.SaveSettings(configData);
            Logger.LogInfo($"Copy Chat Messages: {(configData.CopyChatMessages ? "Activated" : "Deactivated")}");
        }

        [HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
        public class GetPurchasePatch
        {
            public static void Postfix(ref bool __result)
            {
                if (configData.UnlockAllCosmetics)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
        public static class PlayerPhysicsLateUpdate
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (PlayerControl.LocalPlayer?.Collider == null) return; // Prevents errors

                PlayerControl.LocalPlayer.Collider.enabled = !configData.NoClip;
            }
        }
    }

    [HarmonyPatch(typeof(AmongUs.Data.Player.PlayerBanData), nameof(AmongUs.Data.Player.PlayerBanData.IsBanned), MethodType.Getter)]
    public static class IsBannedPatch
    {
        public static void Postfix(ref bool __result)
        {
            if (MainMod.configData.AntiBan)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.LocalPlayer?.Data?.Role == null || __instance.ImpostorVentButton == null) return; // Prevents errors

            if (!PlayerControl.LocalPlayer.Data.Role.CanVent && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                __instance.ImpostorVentButton.gameObject.SetActive(MainMod.configData.UseVents);
            }
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentCanUse
    {
        public static void Postfix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
        {
            if (MainMod.configData.UseVents && !PlayerControl.LocalPlayer.Data.Role.CanVent && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                float num = Vector2.Distance(pc.Object.Collider.bounds.center, __instance.transform.position);

                canUse = num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(pc.Object.Collider, pc.Object.Collider.bounds.center, __instance.transform.position, Constants.ShipOnlyMask, false);
                couldUse = true;
                __result = num;
            }
        }
    }

    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Update))]
    public static class TextBoxTMPUpdate
    {
        public static void Postfix(TextBoxTMP __instance)
        {
            if (MainMod.configData.CopyChatMessages && __instance.hasFocus)
            {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
                {
                    ClipboardHelper.PutClipboardString(__instance.text);
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class UpdateHandler
        {
            public static void Postfix()
            {
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    MainMod.Logger.LogInfo("Menu showed!");
                    MainMod.ToggleMenu();
                }
            }
        }
    }
}

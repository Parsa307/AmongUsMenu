using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AmongUsMenu
{
    [BepInPlugin("com.parsast.amongusmenu", "Among Us Menu", "v1.0.0-dev.1")]
    [BepInProcess("Among Us.exe")]
    public class MainMod : BasePlugin
    {
        private static GameObject? menu;
        private static bool menuVisible = false;
        private static ConfigData configData = new ConfigData();

        public static readonly BepInEx.Logging.ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("AmongUsMenu");

        public override void Load()
        {
            Logger.LogInfo("Mod Initialized");

            // Load config settings
            var loadedConfig = ConfigLoader.LoadSettings();
            if (loadedConfig != null)
            {
                configData = loadedConfig;
            }
            else
            {
                Logger.LogError("Failed to load configuration settings. Using default values.");
            }

            // Initialize Harmony
            var harmony = new Harmony("com.parsast.amongusmenu");
            try
            {
                harmony.PatchAll();
                Logger.LogInfo("Harmony patches applied successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply Harmony patches: {ex}");
            }
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
            panelImage.color = new Color(0, 0, 0, 0.85f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 300);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            AddButton(panel.transform, "Unlock All Cosmetics", new Vector2(0, 50), ToggleUnlockAllCosmetics);
            AddButton(panel.transform, "Close Menu", new Vector2(0, -50), () => menu?.SetActive(false));

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
            textComponent.font = GetFont("Open Sans Bold");
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
            ConfigLoader.SaveSettings(configData.UnlockAllCosmetics);
            Logger.LogInfo($"Unlock All Cosmetics: {(configData.UnlockAllCosmetics ? "Activated" : "Deactivated")}");
        }

        [HarmonyPatch(typeof(PlayerPurchasesData), "GetPurchase")]
        public class GetPurchasePatch
        {
            public static bool Prefix(ref bool __result)
            {
                if (configData.UnlockAllCosmetics)
                {
                    __result = true;
                    return false;
                }
                return true;
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

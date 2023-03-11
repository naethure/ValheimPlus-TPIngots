using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ValheimPlus.Configurations;
using static MessageHud;

namespace ValheimPlus
{

    /// <summary>
    /// Removes damage flash
    /// </summary>
    [HarmonyPatch(typeof(MessageHud), "ShowMessage")]
    public static class MessageHud_ShowMessage_Patch
    {
        private static bool Prefix(MessageHud __instance, MessageType type, string text, int amount = 0, Sprite icon = null)
        {
            if (type == (MessageType)8)
            {
                text = Localization.instance.Localize(text);
                __instance.m_messageCenterText.text = text;
                __instance.m_messageCenterText.canvasRenderer.SetAlpha(1f);
                __instance.m_messageCenterText.CrossFadeAlpha(0f, 1f, ignoreTimeScale: true);
                return false;
            }
            return true;
        }
    }

}

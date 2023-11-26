using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public static class ScrollRectUtils
    {
        public static RectTransform GetViewport(ScrollRect sr)
        {
            return sr.viewport;
        }

        public static Vector2 GetNormalizedPos(ScrollRect sr)
        {
            return sr.normalizedPosition;
        }

        public static float GetNormalizedPosX(ScrollRect sr)
        {
            return sr.horizontalNormalizedPosition;
        }

        public static float GetNormalizedPoxY(ScrollRect sr)
        {
            return sr.verticalNormalizedPosition;
        }

        public static void SetNormalizedPox(ScrollRect sr, float x, float y)
        {
            sr.normalizedPosition = new Vector2(x, y);
        }

        public static float GetVelocityX(ScrollRect sr)
        {
            return sr.velocity.x;
        }

        public static float GetVelocityY(ScrollRect sr)
        {
            return sr.velocity.y;
        }

        public static void SetVelocity(ScrollRect sr, float x, float y)
        {
            sr.velocity = new Vector2(x, y);
        }

        public static bool IsHorizontal(ScrollRect sr)
        {
            return sr.horizontal;
        }

        public static bool IsVertical(ScrollRect sr)
        {
            return sr.vertical;
        }

        public static RectTransform GetVerticalScrollBarRect(ScrollRect sr)
        {
            var b = sr.verticalScrollbar;
            if (b == null) return null;
            return b.transform as RectTransform;
        }

        public static RectTransform GetContentRect(ScrollRect sr)
        {
            return sr.content;
        }
    }
}
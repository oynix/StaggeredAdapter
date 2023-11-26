using UnityEngine;

namespace Gaia
{
    public static class Extensions
    {
        public static T SetActive<T>(this T comp, bool active) where T : Component
        {
            if (comp != null && comp.gameObject != null)
            {
                comp.gameObject.SetActive(active);
            }

            return comp;
        }

        public static T SetAnchorPosition<T>(this T comp, Vector2 anchoredPosition) where T : Component
        {
            ((RectTransform) comp.transform).anchoredPosition = anchoredPosition;
            return comp;
        }

        public static Vector2 GetAnchorPosition<T>(this T comp) where T : Component
        {
            return ((RectTransform) comp.transform).anchoredPosition;
            ;
        }

        public static bool GetActive<T>(this T comp) where T : Component
        {
            return comp.gameObject.activeSelf;
        }
    }
}
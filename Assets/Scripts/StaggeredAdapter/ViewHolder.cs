using UnityEngine;

namespace Gaia
{
    public class ViewHolder
    {
        public readonly RectTransform Root;

        public int Position;
        public int Type;

        protected ViewHolder(int type, RectTransform view)
        {
            Type = type;
            Root = view;
        }

        public virtual void OnDispose()
        {
        }
    }
}
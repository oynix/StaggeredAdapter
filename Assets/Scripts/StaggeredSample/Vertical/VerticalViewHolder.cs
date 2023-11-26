using UnityEngine;

namespace Gaia
{
    public class VerticalViewHolder : ViewHolder
    {
        private readonly VItemViewIgnore _views;

        public VerticalViewHolder(int type, RectTransform view) : base(type, view)
        {
            _views = VItemViewIgnore.Get(view);
        }

        public void Bind(ItemData d)
        {
            _views.No_tmp.text = $"No:{d.Id}";
            _views.Name_tmp.text = $"Name:{d.Name}";
            _views.Score_tmp.text = $"Score:{d.Score}";
        }
    }
}
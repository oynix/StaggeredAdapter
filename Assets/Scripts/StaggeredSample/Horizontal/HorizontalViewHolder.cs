using UnityEngine;

namespace Gaia
{
    public class HorizontalViewHolder : ViewHolder
    {
        private readonly HItemViewIgnore _views;
        public HorizontalViewHolder(int type, RectTransform view) : base(type, view)
        {
            _views = HItemViewIgnore.Get(view);
        }

        public void Bind(ItemData d)
        {
            _views.No_tmp.text = $"No:{d.Id}";
            _views.Name_tmp.text = $"Name:{d.Name}";
            _views.Score_tmp.text = $"Score:{d.Score}";
            _views.BgFlunk_tr.SetActive(d.Score < 60);
        }
    }
}
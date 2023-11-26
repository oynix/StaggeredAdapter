using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class VerticalAdapter : StaggeredAdapter<VerticalViewHolder>
    {
        private readonly Dictionary<int, string> _itemNames = new Dictionary<int, string>()
        {
            {0, "VItemViewIgnore"}
        };

        private readonly List<ItemData> _data;

        public VerticalAdapter(List<ItemData> d, MonoBehaviour parentView, ScrollRect sr, bool scrollOrientationCenter = true) : base(
            parentView, sr, scrollOrientationCenter)
        {
            _data = d;
        }

        protected override int GetItemCount()
        {
            return _data.Count;
        }

        protected override int GetItemType(int position)
        {
            return 0;
        }

        protected override string GetItemName(int type)
        {
            return _itemNames[type];
        }

        protected override VerticalViewHolder OnCreateViewHolder(int type, RectTransform viewRoot)
        {
            return new VerticalViewHolder(type, viewRoot);
        }

        protected override void OnBindView(int position, VerticalViewHolder holder)
        {
            var d = _data[position];
            holder.Bind(d);
        }

        protected override void OnItemVisible(bool repeat, int num, VerticalViewHolder holder, bool completelyVisible)
        {
            holder.Root.SetActive(true);
        }

        protected override void OnItemInvisible(int position, VerticalViewHolder holder)
        {
            holder.Root.SetActive(false);
        }

        protected override void OnItemHighlight(int position, VerticalViewHolder holder)
        {
        }
    }
}
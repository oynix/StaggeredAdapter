
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class HorizontalAdapter : StaggeredAdapter<HorizontalViewHolder>
    {
        private readonly Dictionary<int, string> _itemViewName = new()
        {
            {0, "HItemViewIgnore"}
        };

        private readonly List<ItemData> _data;

        public HorizontalAdapter(List<ItemData> d, MonoBehaviour parentView, ScrollRect sr, bool scrollOrientationCenter = true) : base(parentView, sr, scrollOrientationCenter)
        {
            _data = d;
        }

        protected override int GetItemCount()
        {
            return _data.Count;
        }

        protected override int GetItemType(int position)
        {
            // var d = _data[position];
            // if (...) return 0;
            // if (...) return 1;
            // if (...) return 2;
            return 0;
        }

        protected override string GetItemName(int type)
        {
            return _itemViewName[type];
        }

        protected override HorizontalViewHolder OnCreateViewHolder(int type, RectTransform viewRoot)
        {
            return new HorizontalViewHolder(type, viewRoot);
        }

        protected override void OnBindView(int position, HorizontalViewHolder holder)
        {
            var d = _data[position];
            holder.Bind(d);
        }

        protected override void OnItemVisible(bool repeat, int num, HorizontalViewHolder holder, bool completelyVisible)
        {
            holder.Root.SetActive(true);
        }

        protected override void OnItemInvisible(int position, HorizontalViewHolder holder)
        {
            holder.Root.SetActive(false);
        }

        protected override void OnItemHighlight(int position, HorizontalViewHolder holder)
        {
        }
    }
}
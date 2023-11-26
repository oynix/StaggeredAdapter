using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Gaia
{
    public abstract class StaggeredAdapter<VH> where VH : ViewHolder
    {
        protected readonly MonoBehaviour ParentView;

        private readonly ScrollRect _scrollView;

        // 非滚动方向所有item居中显示，当item填不满viewport时，这个变量控制滚动方向的item是否居中显示
        private readonly bool _scrollOrientationCenter;

        private readonly Vector2 _scrollRect;
        private readonly Vector2 _viewportRect;
        private readonly bool _isHorizontal;
        private readonly RectTransform _contentRect;

        // 显示的view：{position, VH}
        protected readonly Dictionary<int, VH> ItemViews = new();
        private readonly List<int> _itemViewKeys = new();

        // 所有的view均衍生于此
        private readonly Dictionary<string, RectTransform> _zygoteViews = new();

        private readonly List<Coroutine> _coroutines = new();

        protected StaggeredAdapter(MonoBehaviour parentView, ScrollRect sr, bool scrollOrientationCenter = true)
        {
            ParentView = parentView;
            _scrollView = sr;
            _contentRect = ScrollRectUtils.GetContentRect(sr);
            _scrollOrientationCenter = scrollOrientationCenter;

            var scrollRect = sr.transform.GetComponent<RectTransform>().rect;
            _scrollRect = new Vector2(scrollRect.width, scrollRect.height);
            _isHorizontal = ScrollRectUtils.IsHorizontal(sr);
            if (_isHorizontal)
            {
            }
            else if (ScrollRectUtils.IsVertical(sr))
            {
                var barRect = ScrollRectUtils.GetVerticalScrollBarRect(sr);
                if (barRect != null)
                {
                    _scrollRect.x -= Math.Abs(barRect.anchoredPosition.x) + barRect.rect.width;
                }
            }
            else throw new ArgumentException("no orientation enabled of scroll view");

            var viewportRect = ScrollRectUtils.GetViewport(sr).rect;
            _viewportRect = new Vector2(viewportRect.width, viewportRect.height);

            for (var i = 0; i < _contentRect.childCount; ++i)
            {
                var child = _contentRect.GetChild(i).GetComponent<RectTransform>();
                child.SetActive(false);
                _zygoteViews[child.name] = child;
            }

            _scrollView.onValueChanged.AddListener(OnScroll);
        }

        #region public

        public void Show(bool init = false, bool onlyMeasure = false)
        {
            if (init)
            {
                _measuredRect = Vector4.zero;
                CacheAllViewHolder();
                Measure(true);
            }

            if (!_scrollView.GetActive() || onlyMeasure) return;
            Layout(false);
        }

        public void Hide()
        {
            for (var i = 0; i < _coroutines.Count; ++i)
            {
                ParentView.StopCoroutine(_coroutines[i]);
            }

            _coroutines.Clear();
            CacheAllViewHolder();
        }

        // position:指定位置，并且大小未发生改变，可以直接更新，否则需要重新测量和布局
        // 最好指定位置，按需更新，不要全部更新
        public void NotifyDataSetChanged(int position = -1)
        {
            if (position != -1)
            {
                if (!_viewRect.ContainsKey(position))
                    throw new ArgumentOutOfRangeException("position:" + position);

                // 在视野内 则检查更新，否则不检查
                if (ItemViews.ContainsKey(position))
                {
                    var viewRect = _viewRect[position];
                    var viewType = GetItemType(position);
                    var itemName = GetItemName(viewType);
                    var zygoteView = GetItemZygoteView(itemName).rect;

                    if (Math.Abs(viewRect.z - zygoteView.width) < 1 && Math.Abs(viewRect.w - zygoteView.height) < 1)
                    {
                        OnBindView(position, ItemViews[position]);
                    }
                    else
                    {
                        Show(true);
                    }
                }
            }
            else
            {
                Show(true);
            }
        }

        // 追加数据后需要做的事，测量&布局
        public void NotifyDataAppended()
        {
            _measuredRect = Vector4.zero;
            Measure(true);
            Layout(false);
        }

        // 只更新可视区域内的view item上显示的数据，如果updateVisibility为true，则把可见性也刷一下
        public void NotifyVisibleViewChanged(bool updateVisibility = false)
        {
            for (var i = 0; i < _itemViewKeys.Count; ++i)
            {
                var v = ItemViews[_itemViewKeys[i]];
                OnBindView(_itemViewKeys[i], v);
                if (updateVisibility) OnItemVisible(true, i, v, IsViewCompletelyVisible(i));
            }
        }

        // 将position拉进可视区域，center为true则拉到尽可能中间，为false则刚好完全显示
        public float ScrollTo(int position, float delay = 0, bool center = true)
        {
            var duration = 0f;
            const float speed = 0.008f;
            var target = GetContentAnchoredPos(position, center);
            if (_isHorizontal)
            {
                var distance = Math.Abs(_contentRect.anchoredPosition.x - target);
                duration = Math.Min(0.5f, distance * speed);
                _contentRect.DOKill();
                _contentRect.DOAnchorPosX(target, duration).SetDelay(delay).OnComplete(() =>
                {
                    if (ItemViews.ContainsKey(position))
                    {
                        var view = ItemViews[position];
                        OnItemHighlight(position, view);
                    }
                });
            }
            else
            {
                var distance = Math.Abs(_contentRect.anchoredPosition.y - target);
                duration = Math.Min(0.5f, distance * speed);
                _contentRect.DOKill();
                _contentRect.DOAnchorPosY(target, 0.5f).SetDelay(delay).OnComplete(() =>
                {
                    if (ItemViews.ContainsKey(position))
                    {
                        var view = ItemViews[position];
                        OnItemHighlight(position, view);
                    }
                });
            }

            return duration;
        }

        // 同ScrollTo，区别是没有动画，一步到位，一步到胃，一步到肾，一步到肝
        public void MoveTo(int position, bool center = true)
        {
            var target = GetContentAnchoredPos(position, center);
            var anchor = _contentRect.GetAnchorPosition();
            if (_isHorizontal)
            {
                _contentRect.DOKill();
                _contentRect.SetAnchorPosition(new Vector2(target, anchor.y));
            }
            else
            {
                _contentRect.DOKill();
                _contentRect.SetAnchorPosition(new Vector2(anchor.x, target));
            }

            Layout(false);
        }

        public VH GetHolder(int position)
        {
            if (ItemViews.ContainsKey(position))
            {
                return ItemViews[position];
            }

            return null;
        }

        #endregion

        #region protected

        protected void DelayAnim(float delay, Action action)
        {
            _coroutines.Add(ParentView.StartCoroutine(StartDelay(delay, action)));
        }

        private static IEnumerator StartDelay(float delay, Action action)
        {
            yield return new WaitForSecondsRealtime(delay);
            action.Invoke();
        }

        /// <summary>
        /// 滑动区域内的按钮，要把drag事件转发给外层的scroll view，体验会好些
        /// </summary>
        protected void RegisterClick(Transform btn, Action<PointerEventData> onClick)
        {
            EventListener.RegisterDragging(btn.gameObject, _scrollView.OnDrag);
            EventListener.RegisterBeginDrag(btn.gameObject, _scrollView.OnBeginDrag);
            EventListener.RegisterEndDrag(btn.gameObject, _scrollView.OnEndDrag);
            EventListener.RegisterClick(btn.gameObject, p =>
            {
                var vY = ScrollRectUtils.GetVelocityY(_scrollView);
                if (vY > -100 && vY < 100)
                {
                    ScrollRectUtils.SetVelocity(_scrollView, 0, 0);
                    onClick?.Invoke(p);
                }
            });
        }

        public void OnDispose()
        {
            for (var i = 0; i < _itemViewKeys.Count; ++i)
            {
                var v = ItemViews[_itemViewKeys[i]];
                v.OnDispose();
            }

            var cachedKeys = _cachedViews.Keys.ToList();
            for (var i = 0; i < cachedKeys.Count; ++i)
            {
                var views = _cachedViews[cachedKeys[i]];
                for (var j = 0; j < views.Count; ++j)
                {
                    views[j].OnDispose();
                }
            }
        }

        #endregion

        #region measure & layout

        // view的rect：[x,y,width,height]
        private readonly Dictionary<int, Vector4> _viewRect = new();

        // content的rect：[x,y,width,height]
        private Vector4 _measuredRect = Vector4.zero;

        private Vector2 _itemSpacing = Vector2.zero;

        // item间最大间隔
        private readonly Vector2 _itemMaxSpacing = new(5, 0);

        private int _rows;
        private int _columns;
        private int _touchedPosition;

        // 可视视野position
        private int _firstVisiblePosition;
        private int _lastVisiblePosition;
        private int _lastCompletelyVisiblePosition;

        // 可滑动距离 正数
        private float _maxScrolled;

        // 首次测量结束后，如果需要居中显示，则须再次测量
        private void Measure(bool resizeCheck = false)
        {
            _viewRect.Clear();

            _rows = 0;
            _columns = 0;

            var count = GetItemCount();
            if (count <= 0) return;

            _rows = 1;
            _columns = 1;
            var maxX = 0f;
            var maxY = 0f;

            var layoutAnchor = new Vector2(_measuredRect.x, _measuredRect.y);
            for (var i = 0; i < count; ++i)
            {
                var itemType = GetItemType(i);
                var itemName = GetItemName(itemType);
                var itemZygoteRect = GetItemZygoteView(itemName).rect;

                // 放不下就挪一下
                if (_isHorizontal)
                {
                    if (i > 0 && -layoutAnchor.y + itemZygoteRect.height > _scrollRect.y)
                    {
                        var pre = _viewRect[i - 1];
                        layoutAnchor.x += pre.z + _itemSpacing.x;
                        layoutAnchor.y = _measuredRect.y;
                        _columns += 1;
                    }
                }
                else
                {
                    if (i > 0 && layoutAnchor.x + itemZygoteRect.width > _scrollRect.x)
                    {
                        var pre = _viewRect[i - 1];
                        layoutAnchor.y -= pre.w + _itemSpacing.y;
                        layoutAnchor.x = _measuredRect.x;
                        _rows += 1;
                    }
                }

                var rect = new Vector4(layoutAnchor.x, layoutAnchor.y, itemZygoteRect.width, itemZygoteRect.height);
                _viewRect[i] = rect;

                maxX = Math.Max(maxX, Math.Abs(rect.x) + rect.z);
                maxY = Math.Max(maxY, Math.Abs(rect.y) + rect.w);

                if (_isHorizontal)
                    layoutAnchor.y -= rect.w + _itemSpacing.y;
                else
                    layoutAnchor.x += rect.z + _itemSpacing.x;
            }

            _measuredRect.z = maxX;
            _measuredRect.w = maxY;

            _contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _measuredRect.z);
            _contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _measuredRect.w);

            if (_isHorizontal)
                _maxScrolled = Math.Max(0, _measuredRect.z - _scrollRect.x);
            else
                _maxScrolled = Math.Max(0, _measuredRect.w - _scrollRect.y);

            if (resizeCheck)
            {
                var width = _scrollRect.x;
                var height = _scrollRect.y;

                var resized = false;

                // 非滚动方向居中处理，滚动方向是否居中受变量_scrollOrientationCenter控制
                if (_isHorizontal && _measuredRect.w < height || !_isHorizontal && _scrollOrientationCenter)
                {
                    resized = true;
                    var deltaY = height - _measuredRect.w;
                    if (deltaY > 0)
                    {
                        if (_rows > 1) _itemSpacing.y = Math.Min(_itemMaxSpacing.y, deltaY / (_rows - 1));
                        else _itemSpacing.y = 0;
                        deltaY -= (_rows - 1) * _itemSpacing.y;
                        if (deltaY > 0) _measuredRect.y = -(deltaY / 2);
                    }
                }

                if (!_isHorizontal && _measuredRect.z < width || _isHorizontal && _scrollOrientationCenter)
                {
                    resized = true;
                    var deltaX = width - _measuredRect.z;
                    if (deltaX > 0)
                    {
                        if (_columns > 1) _itemSpacing.x = Math.Min(_itemMaxSpacing.x, deltaX / (_columns - 1));
                        else _itemSpacing.x = 0;
                        deltaX -= (_columns - 1) * _itemSpacing.x;
                        if (deltaX > 0) _measuredRect.x = deltaX / 2;
                    }
                }

                if (resized) Measure();
            }
        }

        private void Layout(bool scrolling)
        {
            var start = GetScrolledOffset();
            float end;
            if (_isHorizontal)
            {
                end = start + _scrollRect.x;
            }
            else
            {
                start = -start;
                end = start - _scrollRect.y;
            }

            var count = GetItemCount();
            var firstPosition = 0;
            var lastPosition = count - 1;
            var firstFound = false;
            var lastCompletelyPosition = 0;
            for (var i = 0; i < count; ++i)
            {
                var rect = _viewRect[i];
                var visible = false;
                var completelyVisible = false;
                if (_isHorizontal)
                {
                    var left = rect.x;
                    var right = rect.x + rect.z;
                    if (left >= start && left < end || right > start && right <= end) visible = true;

                    if (visible)
                    {
                        if (left >= start && right <= end) completelyVisible = true;
                    }
                }
                else
                {
                    var top = rect.y;
                    var bottom = rect.y - rect.w;
                    if (top <= start && top > end || bottom < start && bottom >= end) visible = true;

                    if (visible)
                    {
                        if (top <= start && bottom >= end) completelyVisible = true;
                    }
                }

                if (visible)
                {
                    if (!firstFound)
                    {
                        firstFound = true;
                        firstPosition = i;
                    }
                }
                else
                {
                    if (firstFound)
                    {
                        lastPosition = i - 1;
                        break;
                    }
                }

                if (completelyVisible && i > lastCompletelyPosition) lastCompletelyPosition = i;
            }

            if (scrolling && _firstVisiblePosition == firstPosition && _lastVisiblePosition == lastPosition &&
                _lastCompletelyVisiblePosition == lastCompletelyPosition) return;

            _firstVisiblePosition = firstPosition;
            _lastVisiblePosition = lastPosition;
            _lastCompletelyVisiblePosition = lastCompletelyPosition;


            // 视野之外的回收，视野内类型对不上的也回收
            for (var i = _itemViewKeys.Count - 1; i >= 0; --i)
            {
                var p = _itemViewKeys[i];
                var recycle = false;
                if (p < firstPosition || p > lastPosition)
                {
                    recycle = true;
                }
                else
                {
                    var newType = GetItemType(p);
                    if (newType != ItemViews[p].Type)
                    {
                        recycle = true;
                    }
                }

                if (recycle)
                {
                    var h = ItemViews[p];
                    OnItemInvisible(p, h);
                    CacheViewHolder(h);

                    ItemViews.Remove(p);
                    _itemViewKeys.RemoveAt(i);
                }
            }

            // 视野之内的填坑
            var num = 0;
            for (var i = firstPosition; i <= lastPosition; ++i)
            {
                VH holder;

                var rect = _viewRect[i];
                var completelyVisible = false;
                if (_isHorizontal)
                {
                    var left = rect.x;
                    var right = rect.x + rect.z;
                    if (left >= start && left < end || right > start && right <= end)
                    {
                        if (left >= start && right <= end) completelyVisible = true;
                    }
                }
                else
                {
                    var top = rect.y;
                    var bottom = rect.y - rect.w;
                    if (top <= start && top > end || bottom < start && bottom >= end)
                    {
                        if (top <= start && bottom >= end) completelyVisible = true;
                    }
                }

                if (!ItemViews.ContainsKey(i))
                {
                    holder = CreateViewHolder(i);

                    ItemViews[i] = holder;
                    _itemViewKeys.Add(i);

                    holder.Position = i;
                    holder.Root.anchorMax = new Vector2(0, 1);
                    holder.Root.anchorMin = new Vector2(0, 1);
                    holder.Root.pivot = new Vector2(0, 1);
                    holder.Root.SetAnchorPosition(new Vector2(_viewRect[i].x, _viewRect[i].y));

                    var zygote = _zygoteViews[GetItemName(holder.Type)];
                    holder.Root.name = $"{zygote.name}_{i}";

                    OnBindView(i, holder);

                    OnItemVisible(i <= _touchedPosition, num, holder, completelyVisible);
                    num += 1;
                }
                else
                {
                    holder = ItemViews[i];
                    OnItemVisible(true, 0, holder, completelyVisible);
                }
            }

            _touchedPosition = Math.Max(_touchedPosition, _lastVisiblePosition);
        }

        private void OnScroll(Vector2 v)
        {
            if (_maxScrolled <= 0) return;
            Layout(true);
        }

        // 已经滑动的距离，始终为正数，范围：[0, _maxScrolled]
        private float GetScrolledOffset()
        {
            // 滑动了多少 起始点在左上角时，坐标最大的时候=0 最小的时候=1
            float offsetRate;
            if (_isHorizontal)
            {
                offsetRate = ScrollRectUtils.GetNormalizedPosX(_scrollView);
            }
            else
            {
                offsetRate = 1 - ScrollRectUtils.GetNormalizedPoxY(_scrollView);
            }

            offsetRate = Math.Min(1, Math.Max(offsetRate, 0));

            return _maxScrolled * offsetRate;
        }

        #endregion

        #region cached view

        // 缓存：{type, List}/{name, List}
        private readonly Dictionary<int, List<VH>> _cachedViews = new();

        private VH CreateViewHolder(int position)
        {
            var itemType = GetItemType(position);
            if (!_cachedViews.ContainsKey(itemType)) _cachedViews[itemType] = new List<VH>();

            var cached = _cachedViews[itemType];
            if (cached.Count > 0)
            {
                var holder = cached[0];
                cached.RemoveAt(0);
                return holder;
            }
            else
            {
                var itemName = GetItemName(itemType);
                var viewRoot = Object.Instantiate(GetItemZygoteView(itemName), _contentRect);
                viewRoot.SetActive(false);
                var holder = OnCreateViewHolder(itemType, viewRoot);

                return holder;
            }
        }

        private void CacheViewHolder(VH holder)
        {
            if (!_cachedViews.ContainsKey(holder.Type)) _cachedViews[holder.Type] = new List<VH>();
            _cachedViews[holder.Type].Add(holder);
            holder.Root.SetActive(false);
            holder.Root.name += "_cached";
        }

        private void CacheAllViewHolder()
        {
            for (var i = 0; i < _itemViewKeys.Count; ++i)
            {
                CacheViewHolder(ItemViews[_itemViewKeys[i]]);
            }

            ItemViews.Clear();
            _itemViewKeys.Clear();
            _touchedPosition = -1;
        }

        #endregion

        #region private

        private RectTransform GetItemZygoteView(string itemName)
        {
            if (!_zygoteViews.ContainsKey(itemName))
                throw new ArgumentException("no item found for name:" + itemName);

            return _zygoteViews[itemName];
        }

        // 将position移动到尽可能居中位置，content的anchored坐标值
        // 将position移动到刚好完全显示，content的anchored坐标值
        private float GetContentAnchoredPos(int position, bool center)
        {
            var delta = 0f;
            var viewRect = _viewRect[position];
            if (_isHorizontal)
            {
                // 起点在左上角，content anchor x都小于0，item anchor x都大于0，处理over scroll情况
                if (center)
                {
                    var target = _scrollRect.x / 2 - (viewRect.x + viewRect.z / 2);
                    delta = Math.Max(Math.Min(0, target), -_maxScrolled);
                }
                else
                {
                    // 坐标系转化
                    var axisOffset = GetScrolledOffset();
                    var left = viewRect.x - axisOffset;
                    var right = left + viewRect.z;
                    var viewport = new Vector2(0, _viewportRect.x);
                    var offset = 0f;
                    if (left < viewport.x) offset = viewport.x - left;
                    else if (right > viewport.y) offset = viewport.y - right;
                    delta = -axisOffset + offset;
                }
            }
            else
            {
                // 起点在左上角，content anchor y都大于0，item anchor y都小于0，也要处理over scroll情况
                if (center)
                {
                    var target = -_scrollRect.y / 2 - (viewRect.y - viewRect.w / 2);
                    delta = Math.Min(Math.Max(0, target), _maxScrolled);
                }
                else
                {
                    // 坐标系转化
                    var axisOffset = GetScrolledOffset();
                    var top = viewRect.y + axisOffset;
                    var bottom = top - viewRect.w;
                    var viewport = new Vector2(0, -_viewportRect.y);
                    var offset = 0f;
                    if (top > viewport.x) offset = viewport.x - top;
                    else if (bottom < viewport.y) offset = viewport.y - bottom;
                    delta = axisOffset + offset;
                }
            }

            return delta;
        }

        private bool IsViewCompletelyVisible(int position)
        {
            if (!_itemViewKeys.Contains(position)) return false;
            var target = GetContentAnchoredPos(position, false);
            var current = _isHorizontal ? _contentRect.anchoredPosition.x : _contentRect.anchoredPosition.y;

            return Math.Abs(current - target) <= 1;
        }

        #endregion

        protected abstract int GetItemCount();
        protected abstract int GetItemType(int position);
        protected abstract string GetItemName(int type);
        protected abstract VH OnCreateViewHolder(int type, RectTransform viewRoot);
        protected abstract void OnBindView(int position, VH holder);

        /// <summary>
        /// 当item view变得可见时，会调用这个方法，可分为2种情况：
        /// 第一种是首次出现，此时可按需播放出现动画，此时repeat为false，
        /// 第二种是滑出可视区域后，再次回到可视区域，属于重复出现，此时repeat为true。
        /// 此外，调用NotifyVisibleViewChanged时传入true强制刷新可见性，也可触发第二种
        ///
        /// 当一行/一列有多个item view时，在创建/滑动过程中会同时出现多个item view，
        /// 而num表示此item view在本批次出现的item view中的序号，可用作动画延时，做出
        /// 波浪效果
        /// </summary>
        protected abstract void OnItemVisible(bool repeat, int num, VH holder, bool completelyVisible);

        protected abstract void OnItemInvisible(int position, VH holder);
        protected abstract void OnItemHighlight(int position, VH holder);
    }
}
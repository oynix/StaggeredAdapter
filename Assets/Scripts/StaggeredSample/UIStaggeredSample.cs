using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gaia
{
    public partial class UIStaggeredSample : MonoBehaviour
    {
        private void Awake()
        {
            Bind(transform);
        }

        private void Start()
        {
            InitHorizontalList();

            InitVerticalList();
        }

        private void InitHorizontalList()
        {
            var data = new List<ItemData>();
            for (var i = 0; i < 100; ++i)
            {
                data.Add(new ItemData(i, $"Student{i + 1}", Random.Range(30, 101)));
            }

            var adapter = new HorizontalAdapter(data, this, HScrollView_tr.GetComponent<ScrollRect>());
            adapter.Show(true);
        }

        private void InitVerticalList()
        {
            var data = new List<ItemData>();
            for (var i = 0; i < 100; ++i)
            {
                data.Add(new ItemData(i, $"Student{i + 1}", Random.Range(30, 101)));
            }

            var vertical = new VerticalAdapter(data, this, VScrollView_tr.GetComponent<ScrollRect>());
            vertical.Show(true);
        }
    }
}
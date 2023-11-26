using UnityEngine;
using UnityEngine.UI;


namespace Gaia
{
    public partial class UIStaggeredSample
    {
        // public GameObject GameObject;
        // public Transform Root;
        
		public Transform HScrollView_tr;
		public Transform VScrollView_tr;


        public static UIStaggeredSample Get(GameObject g)
        {
            var p = new UIStaggeredSample();
            p.Bind(g.transform);
            return p;
        }
        
        public static UIStaggeredSample Get(Transform t)
        {
            var p = new UIStaggeredSample();
            p.Bind(t);
            return p;
        }

        private UIStaggeredSample()
        {
        }
        
        private void Bind(Transform root)
        {

			HScrollView_tr = root.Find("HScrollView_tr").GetComponent<Transform>();
			VScrollView_tr = root.Find("VScrollView_tr").GetComponent<Transform>();

        }
        
    }
}
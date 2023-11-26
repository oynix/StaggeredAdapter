using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Gaia
{
    public partial class VItemViewIgnore
    {
        // public GameObject GameObject;
        // public Transform Root;
        
		public TMP_Text No_tmp;
		public TMP_Text Name_tmp;
		public TMP_Text Score_tmp;


        public static VItemViewIgnore Get(GameObject g)
        {
            var p = new VItemViewIgnore();
            p.Bind(g.transform);
            return p;
        }
        
        public static VItemViewIgnore Get(Transform t)
        {
            var p = new VItemViewIgnore();
            p.Bind(t);
            return p;
        }

        private VItemViewIgnore()
        {
        }
        
        private void Bind(Transform root)
        {

			No_tmp = root.Find("No_tmp").GetComponent<TMP_Text>();
			Name_tmp = root.Find("Name_tmp").GetComponent<TMP_Text>();
			Score_tmp = root.Find("Score_tmp").GetComponent<TMP_Text>();

        }
        
    }
}
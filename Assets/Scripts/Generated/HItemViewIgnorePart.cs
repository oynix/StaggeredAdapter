using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Gaia
{
    public class HItemViewIgnore
    {
		public Transform BgFlunk_tr;
		public TMP_Text No_tmp;
		public TMP_Text Name_tmp;
		public TMP_Text Score_tmp;

		public static HItemViewIgnore Get(Transform t)
        {
            var p = new HItemViewIgnore();
            p.Bind(t);
            return p;
        }

		private void Bind(Transform root)
        {
	        BgFlunk_tr = root.Find("BgFlunk_tr").GetComponent<Transform>();
			No_tmp = root.Find("No_tmp").GetComponent<TMP_Text>();
			Name_tmp = root.Find("Name_tmp").GetComponent<TMP_Text>();
			Score_tmp = root.Find("Score_tmp").GetComponent<TMP_Text>();
        }
    }
}
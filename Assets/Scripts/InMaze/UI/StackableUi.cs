using UnityEngine;

namespace Assets.Scripts.InMaze.UI
{
    public class StackableUi : MonoBehaviour
    {
        protected StackableUi LowerNode;
        protected StackableUi UpperNode;

        public void PopUpwards()
        {
            if (LowerNode != null)
            {
				LowerNode.PopUpwards();
                // Move to upper place
                LowerNode.transform.parent.localPosition =
                    transform.parent.localPosition;
            }
        }

        public void OnDestroy()
        {
            if (UpperNode != null)
                UpperNode.LowerNode = LowerNode;
            PopUpwards();
        }
    }
}

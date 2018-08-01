using System;
using System.Windows.Forms;

namespace Cool
{
    /// <summary>
    /// Provide a "cool" listview.
    /// </summary>
    public class CoolListView : ListView
    {
        public CoolListView()
        {
            base.DoubleBuffered = true;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            WindowUtils.HideAnnoyingFocusRectangles(this);
            // the Explorer-style listview looks cooler than the standard listview.
            WindowUtils.SetExplorerStyleControl(this);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            WindowUtils.HideAnnoyingFocusRectangles(this);
        }
    }
}

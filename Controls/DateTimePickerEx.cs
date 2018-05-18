using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Cool
{
    /// <summary>
    /// A nullable DateTimePicker.
    /// </summary>
    [DefaultEvent("ValueChanged")]
    class DateTimePickerEx : DateTimePicker
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DateTimePickerEx()
        {
            base.ShowCheckBox = true;
        }

        /// <summary>
        /// Get or set the current date/time value.
        /// </summary>
        [Browsable(true), Category("Behavior")]
        [Description("The current date/time value for this control.")]
        [DefaultValue(null)]
        public new DateTime? Value
        {
            get
            {
                if (base.Checked)
                {
                    return base.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    base.Checked = true;
                    base.Value = value.Value;
                }
                else
                {
                    base.Checked = false;
                    base.OnValueChanged(new EventArgs());
                }
            }
        }

        /// <summary>
        /// Get or set the value for displaying a check box.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DefaultValue(true)]
        public new bool ShowCheckBox
        {
            // the check box cannot be hidden.
            // if the check box is annoying, use the standard DateTimePicker instead.
            get { return true; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using NLog;

namespace PerfectoLabPackage.ToolWindows.ClientCommon.Controls
{
    public class PasswordTextBox : TextBox
    {
        #region Ctor
        public PasswordTextBox()
        {
            Password = string.Empty;
        }
        #endregion

        #region Const
        private const string PASSWORD_CHAR = "•"; 
        #endregion

        #region Members
        private bool _isEditMode;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region DependencyProperty - Password
        public string Password
        {
            get { return GetValue(PasswordProperty) as string; }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeparatorVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordTextBox),
                new UIPropertyMetadata(null, OnPasswordPropertyChanged));

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordTextBox = d as PasswordTextBox;
            if (e.NewValue != null)
                SetTextHidden(passwordTextBox, e.NewValue.ToString());
        }
        #endregion


        #region Methods
        protected override void OnTextChanged(TextChangedEventArgs e)
        {

            try
            {
                base.OnTextChanged(e);

                if (!_isEditMode)
                {
                    if (Password == null)
                        Password = string.Empty;

                    var additions = e.Changes.FirstOrDefault(tc => tc.AddedLength > 0);
                    if (additions != null)
                    {
                        var changeStart = additions.Offset;
                        var changeEnd = changeStart + additions.AddedLength + (Password.Length - Text.Length);

                        var startOriginalText = Password.Substring(0, additions.Offset);
                        var changedText = Text.Substring(changeStart, additions.AddedLength);
                        var endOriginalText = Password.Substring(changeEnd, Password.Length - changeEnd);

                        Password = string.Concat(startOriginalText, changedText, endOriginalText);
                        CaretIndex = changeStart + additions.AddedLength;
                    }
                    else
                    {
                        var removes = e.Changes.FirstOrDefault(tc => tc.RemovedLength > 0);
                        if (removes != null)
                        {
                            Password = Password.Remove(removes.Offset, removes.RemovedLength);
                            CaretIndex = removes.Offset;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private static void SetTextHidden(PasswordTextBox passwordTextBox, string newValue)
        {
            try
            {
                passwordTextBox._isEditMode = true;
                var newvalueArray = newValue.Select(c => PASSWORD_CHAR);
                if (newvalueArray.Any())
                    passwordTextBox.Text = newvalueArray.Aggregate((c, n) => string.Concat(c, n));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                passwordTextBox._isEditMode = false;
            }
        } 
        #endregion
    }
}

namespace WpfControls.Editors
{

    using System;
    using System.Collections;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// A Text box with automatic suggestion functionality.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control" />
    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartSelector, Type = typeof(Selector))]
    public class AutoCompleteTextBox : Control
    {

        #region "Fields"

        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";

        public const string PartSelector = "PART_Selector";
        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(200));
        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register("DisplayMember", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register("IconPlacement", typeof(IconPlacement), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(IconPlacement.Left));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty LoadingContentProperty = DependencyProperty.Register("LoadingContent", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register("Provider", typeof(ISuggestionProvider), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));

        private bool _isUpdatingText;

        private bool _selectionCancelled;

        private SuggestionsAdapter _suggestionsAdapter;


        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes the <see cref="AutoCompleteTextBox"/> class.
        /// </summary>
        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        #endregion

        #region "Properties"

        /// <summary>
        /// Gets or sets the binding evaluator.
        /// </summary>
        /// <value>
        /// The binding evaluator.
        /// </value>
        public BindingEvaluator BindingEvaluator { get; set; }

        /// <summary>
        /// Gets or sets the delay by which suggestion list must open.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }

            set { SetValue(DelayProperty, value); }
        }

        /// <summary>
        /// Gets or sets the display member.
        /// </summary>
        /// <value>
        /// The display member.
        /// </value>
        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }

            set { SetValue(DisplayMemberProperty, value); }
        }

        public TextBox Editor { get; set; }

        public DispatcherTimer FetchTimer { get; set; }

        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>
        /// The icon.
        /// </value>
        public object Icon
        {
            get { return GetValue(IconProperty); }

            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon placement.
        /// </summary>
        /// <value>
        /// The icon placement.
        /// </value>
        public IconPlacement IconPlacement
        {
            get { return (IconPlacement)GetValue(IconPlacementProperty); }

            set { SetValue(IconPlacementProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon visibility.
        /// </summary>
        /// <value>
        /// The icon visibility.
        /// </value>
        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }

            set { SetValue(IconVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is drop down open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is drop down open; otherwise, <c>false</c>.
        /// </value>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }

            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether suggestion list is loading.
        /// </summary>
        /// <value>
        /// <c>true</c> if suggestion list is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }

            set { SetValue(IsLoadingProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }

            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the items selector.
        /// </summary>
        /// <value>
        /// The items selector.
        /// </value>
        public Selector ItemsSelector { get; set; }

        /// <summary>
        /// Gets or sets the item template.
        /// </summary>
        /// <value>
        /// The item template.
        /// </value>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }

            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the item template selector.
        /// </summary>
        /// <value>
        /// The item template selector.
        /// </value>
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return ((DataTemplateSelector)(GetValue(ItemTemplateSelectorProperty))); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the content of the loading.
        /// </summary>
        /// <value>
        /// The content of the loading.
        /// </value>
        public object LoadingContent
        {
            get { return GetValue(LoadingContentProperty); }

            set { SetValue(LoadingContentProperty, value); }
        }

        public Popup Popup { get; set; }

        /// <summary>
        /// Gets or sets the suggestion provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public ISuggestionProvider Provider
        {
            get { return (ISuggestionProvider)GetValue(ProviderProperty); }

            set { SetValue(ProviderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }

            set { SetValue(SelectedItemProperty, value); }
        }

        public SelectionAdapter SelectionAdapter { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }

            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the watermark.
        /// </summary>
        /// <value>
        /// The watermark.
        /// </value>
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }

            set { SetValue(WatermarkProperty, value); }
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Called when [selected item changed].
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var act = d as AutoCompleteTextBox;
            if (act != null)
            {
                if (act.Editor != null & !act._isUpdatingText)
                {
                    act._isUpdatingText = true;
                    act.Editor.Text = act.BindingEvaluator.Evaluate(e.NewValue);
                    act._isUpdatingText = false;
                }
            }
        }

        private void ScrollToSelectedItem()
        {
            var listBox = ItemsSelector as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
                listBox.ScrollIntoView(listBox.SelectedItem);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Editor = Template.FindName(PartEditor, this) as TextBox;
            Popup = Template.FindName(PartPopup, this) as Popup;
            ItemsSelector = Template.FindName(PartSelector, this) as Selector;
            BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));
            GotFocus += AutoCompleteTextBox_GotFocus;
            if (Editor != null)
            {
                Editor.TextChanged += OnEditorTextChanged;
                Editor.PreviewKeyDown += OnEditorKeyDown;
                Editor.LostFocus += OnEditorLostFocus;

                if (SelectedItem != null)
                {
                    Editor.Text = BindingEvaluator.Evaluate(SelectedItem);
                }

            }

            if (Popup != null)
            {
                Popup.StaysOpen = false;
                Popup.Opened += OnPopupOpened;
                Popup.Closed += OnPopupClosed;
            }
            if (ItemsSelector != null)
            {
                SelectionAdapter = new SelectionAdapter(ItemsSelector);
                SelectionAdapter.Commit += OnSelectionAdapterCommit;
                SelectionAdapter.Cancel += OnSelectionAdapterCancel;
                SelectionAdapter.SelectionChanged += OnSelectionAdapterSelectionChanged;
            }
        }

        void AutoCompleteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Editor == null) return;
            Editor.Focus();
            Editor.SelectAll();
        }
        private string GetDisplayText(object dataItem)
        {
            if (BindingEvaluator == null)
            {
                BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));
            }
            if (dataItem == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(DisplayMember))
            {
                return dataItem.ToString();
            }
            return BindingEvaluator.Evaluate(dataItem);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (SelectionAdapter == null) return;
            if (IsDropDownOpen)
                SelectionAdapter.HandleKeyDown(e);
            else
                IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.F4;
        }

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                IsDropDownOpen = false;
            }
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText)
                return;
            if (FetchTimer == null)
            {
                FetchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Delay) };
                FetchTimer.Tick += OnFetchTimerTick;
            }
            FetchTimer.IsEnabled = false;
            FetchTimer.Stop();
            SetSelectedItem(null);
            if (Editor.Text.Length > 0)
            {
                IsLoading = true;
                IsDropDownOpen = true;
                ItemsSelector.ItemsSource = null;
                FetchTimer.IsEnabled = true;
                FetchTimer.Start();
            }
            else
            {
                IsDropDownOpen = false;
            }
        }

        private void OnFetchTimerTick(object sender, EventArgs e)
        {
            FetchTimer.IsEnabled = false;
            FetchTimer.Stop();
            if (Provider != null && ItemsSelector != null)
            {
                Filter = Editor.Text;
                if (_suggestionsAdapter == null)
                {
                    _suggestionsAdapter = new SuggestionsAdapter(this);
                }
                _suggestionsAdapter.GetSuggestions(Filter);
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (!_selectionCancelled)
            {
                OnSelectionAdapterCommit();
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            _selectionCancelled = false;
            ItemsSelector.SelectedItem = SelectedItem;
        }

        private void OnSelectionAdapterCancel()
        {
            _isUpdatingText = true;
            Editor.Text = SelectedItem == null ? Filter : GetDisplayText(SelectedItem);
            Editor.SelectionStart = Editor.Text.Length;
            Editor.SelectionLength = 0;
            _isUpdatingText = false;
            IsDropDownOpen = false;
            _selectionCancelled = true;
        }

        private void OnSelectionAdapterCommit()
        {
            if (ItemsSelector.SelectedItem != null)
            {
                SelectedItem = ItemsSelector.SelectedItem;
                _isUpdatingText = true;
                Editor.Text = GetDisplayText(ItemsSelector.SelectedItem);
                SetSelectedItem(ItemsSelector.SelectedItem);
                _isUpdatingText = false;
                IsDropDownOpen = false;
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            _isUpdatingText = true;
            Editor.Text = ItemsSelector.SelectedItem == null ? Filter : GetDisplayText(ItemsSelector.SelectedItem);
            Editor.SelectionStart = Editor.Text.Length;
            Editor.SelectionLength = 0;
            ScrollToSelectedItem();
            _isUpdatingText = false;
        }

        private void SetSelectedItem(object item)
        {
            _isUpdatingText = true;
            SelectedItem = item;
            _isUpdatingText = false;
        }
        #endregion

        #region "Nested Types"

        private class SuggestionsAdapter
        {

            #region "Fields"

            private readonly AutoCompleteTextBox _actb;

            private string _filter;
            #endregion

            #region "Constructors"

            public SuggestionsAdapter(AutoCompleteTextBox actb)
            {
                _actb = actb;
            }

            #endregion

            #region "Methods"

            public void GetSuggestions(string searchText)
            {
                _filter = searchText;
                _actb.IsLoading = true;
                var thInfo = new ParameterizedThreadStart(GetSuggestionsAsync);
                var th = new Thread(thInfo);
                th.Start(new object[] {
				searchText,
				_actb.Provider
			});
            }

            private void DisplaySuggestions(IEnumerable suggestions, string filter)
            {
                if (_filter != filter)
                {
                    return;
                }
                if (_actb.IsDropDownOpen)
                {
                    _actb.IsLoading = false;
                    _actb.ItemsSelector.ItemsSource = suggestions;
                    _actb.IsDropDownOpen = _actb.ItemsSelector.HasItems;
                }

            }

            private void GetSuggestionsAsync(object param)
            {
                var args = param as object[];
                if (args == null) return;
                var searchText = Convert.ToString(args[0]);
                var provider = args[1] as ISuggestionProvider;
                if (provider == null) return;
                var list = provider.GetSuggestions(searchText);
                _actb.Dispatcher.BeginInvoke(new Action<IEnumerable, string>(DisplaySuggestions), DispatcherPriority.Background, new object[] {
                    list,
                    searchText
                });
            }

            #endregion

        }

        #endregion

    }

}
